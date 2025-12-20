using APICat.Application.Common;
using APICat.Application.Interfaces;
using APICat.Application.Models.Dtos;
using APICat.Domain.Entities;
using APICat.Domain.Entities.Base;
using APICat.Infraestructure.Contexts;
using APICat.Infraestructure.Repositories;
using APICat.Logging.Attributes;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace APICat.Application.Services
{
    public class CatService : ICatService
    {
        private readonly CatContext _context;
        private readonly HttpClient _client;
        private readonly Repository<Breed, Guid> _repo;
        private readonly ILogger _logger;
        private readonly IValidator<BreedsDto> _breedValidator;

        public CatService(HttpClient client, CatContext context, ILogger<CatService> logger, IValidator<BreedsDto> breedValidator)
        {
            _client = client;
            _context = context;
            _repo = new Repository<Breed, Guid>(_context);
            _logger = logger;
            _breedValidator = breedValidator;
        }

        [LogExecution("Obteniendo listado razas de gatos")]
        public async Task<IOperationResult<BreedsDto>> GetBreedByIdAsync(string id)
        {
            try
            {
                if (_client != null)
                {
                    var url = string.Concat(_client.BaseAddress, string.Format("breeds/{0}",id));
                    var request = await _client.GetStringAsync(url);
                    var response = JsonConvert.DeserializeObject<BreedsDto>(request);

                    if (response == null) { return OperationResult.Fail<BreedsDto>("No se pudo obtener la respuesta del WS, intente nuevamente en unos momentos"); }
                    var result = OperationResult.Success<BreedsDto>(response);

                    return result;
                }
                return OperationResult.Fail<BreedsDto>("Ocurrió un error en la comunicación con el WS");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail<BreedsDto>($"Ocurrió un error mientras se ejecutaba la consulta, no se pueden recuperar datos del WS. Código de Error: {ex.Message}");
            }
        }

        [LogExecution("Obteniendo razas de gatos por ID")]
        public async Task<IOperationResult<List<BreedsDto>>>GetBreedsAsync()
        {
            try
            {
                if (_client != null)
                {
                    var url = string.Concat(_client.BaseAddress, string.Format("breeds/"));
                    var request = await _client.GetStringAsync(url);
                    var response = JsonConvert.DeserializeObject<List<BreedsDto>>(request);

                    if (response == null) { return OperationResult.Fail<List<BreedsDto>>("No se pudo obtener la respuesta del WS, intente nuevamente en unos momentos"); }
                    var result = OperationResult.Success(response);

                    return result;
                }
                return OperationResult.Fail<List<BreedsDto>>("Ocurrió un error en la comunicación con el WS");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail<List<BreedsDto>>($"Ocurrió un error mientras se ejecutaba la consulta, no se pueden recuperar datos del WS. Código de Error: {ex.Message}");
            }
        }

        [LogExecution("Insertando nueva raza a la tabla en DB")]
        public async Task<IOperationResult> PostBreedAsync(BreedsDto breed)
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var valid = await _breedValidator.ValidateAsync(breed);

                if (!valid.IsValid)
                {
                    return OperationResult.Fail<BreedsDto>("No se pudieron validar los datos ingresados", valid.Errors.Select(x => x.ErrorMessage.ToString()));
                }

               var newBreed = new Breed() 
               {
                   Id = Guid.NewGuid(),
                   Name = breed.Name,
                   Origin = breed.Origin,
                   Temperament = breed.Temperament,
                   Description = breed.Description,
                   Wikipedia_url = breed.Wikipedia_url,
               };

                await _repo.AddAsync(newBreed, cancellationToken: default);
                await _repo.SaveChangesAsync();
                transaction.Commit();

                return OperationResult.Success($"La inserción de la raza de gato fue exitosa, se genero con el ID: {newBreed.Id}");
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return OperationResult.Fail( $"Ocurrió un error en la inserción del registro, {ex.Message}");
            }
        }
    }
}
