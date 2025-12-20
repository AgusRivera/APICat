using APICat.Application.Common;
using APICat.Application.Interfaces;
using APICat.Application.Models.Dtos;
using APICat.Domain.Entities;
using APICat.Domain.Entities.Base;
using APICat.Domain.Interfaces.Repositories;
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
        private readonly IRepository<Breed, Guid> _repo;
        private readonly ILogger _logger;
        private readonly IValidator<BreedsDto> _breedValidator;

        public CatService(HttpClient client, CatContext context, ILogger<CatService> logger, IValidator<BreedsDto> breedValidator, IRepository<Breed, Guid> repo)
        {
            _client = client;
            _context = context;
            _repo = repo;
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

        [LogExecution("Obteniendo todas las razas almacenadas en la DB")]
        public async Task<IOperationResult<List<BreedsDto>>> GetAllBreedsFromDbAsync()
        {
            try
            {
                var entities = await _repo.GetAllAsync();

                var dtos = entities.Select(e => new BreedsDto
                {
                    Id = e.Id.ToString(),
                    Name = e.Name,
                    Origin = e.Origin,
                    Temperament = e.Temperament,
                    Description = e.Description,
                    Wikipedia_url = e.Wikipedia_url
                }).ToList();

                return OperationResult.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las razas de la DB");
                return OperationResult.Fail<List<BreedsDto>>($"Ocurrió un error al recuperar los datos: {ex.Message}");
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

        [LogExecution("Eliminando raza de gato de la DB")]
        public async Task<IOperationResult> DeleteBreedAsync(Guid id)
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var rowsAffected = await _repo.ExecuteDeleteByIdAsync(id);

                if (rowsAffected == 0)
                {
                    return OperationResult.Fail($"No se encontró ninguna raza con el ID: {id}");
                }

                await _repo.SaveChangesAsync();

                transaction.Commit();

                return OperationResult.Success($"El registro {id} fue eliminado correctamente.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error al eliminar la raza {Id}", id);
                return OperationResult.Fail($"Ocurrió un error al intentar eliminar el registro: {ex.Message}");
            }
        }
    }
}
