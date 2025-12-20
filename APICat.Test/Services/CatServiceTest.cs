using APICat.Application.Models.Dtos;
using APICat.Application.Services;
using APICat.Domain.Entities;
using APICat.Domain.Interfaces.Repositories;
using APICat.Infraestructure.Contexts;
using APICat.Tests.Helpers; // Importante para usar el helper que creamos
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using Xunit;

namespace APICat.Tests.Services
{
    public class CatServiceTests
    {
        // Dependency Mocks
        private readonly Mock<CatContext> _contextMock;
        private readonly Mock<IRepository<Breed, Guid>> _repoMock;
        private readonly Mock<ILogger<CatService>> _loggerMock;
        private readonly Mock<IValidator<BreedsDto>> _validatorMock;

        private readonly Mock<IDbContextTransaction> _transactionMock;
        private readonly Mock<Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade> _databaseFacadeMock;

        public CatServiceTests()
        {
            _contextMock = new Mock<CatContext>(new DbContextOptions<CatContext>());
            _repoMock = new Mock<IRepository<Breed, Guid>>();
            _loggerMock = new Mock<ILogger<CatService>>();
            _validatorMock = new Mock<IValidator<BreedsDto>>();
            _transactionMock = new Mock<IDbContextTransaction>();

            ////////////////////////////////////
            // Configuración compleja para mockear _context.Database.BeginTransactionAsync()
            // Esto es necesario para mi código ya que usé Transacciones explícitas.
            _databaseFacadeMock = new Mock<Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade>(_contextMock.Object);

            // Cuando pidan la Transacción, devolvemos nuestro Mock
            _databaseFacadeMock.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                               .ReturnsAsync(_transactionMock.Object);

            // Asignamos el Facade al Contexto
            _contextMock.Setup(c => c.Database).Returns(_databaseFacadeMock.Object);
            ///////////////////////////////////
        }

        #region Tests GetBreedByIdAsync

        [Fact]
        public async Task GetBreedByIdAsync_DebeRetornarExito_CuandoApiRespondeOK()
        {
            // ARRANGE
            var jsonResponse = "{ \"id\": \"abys\", \"name\": \"Abyssinian\", \"origin\": \"Egypt\" }";
            var handlerMock = MockHttpMessageHandler.SetupReturn(jsonResponse, HttpStatusCode.OK);
            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.fake.com/") };

            var service = new CatService(httpClient, _contextMock.Object, _loggerMock.Object, _validatorMock.Object, _repoMock.Object);

            // ACT
            var result = await service.GetBreedByIdAsync("abys");

            // ASSERT
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("Abyssinian", result.Value.Name);
        }

        [Fact]
        public async Task GetBreedByIdAsync_DebeRetornarFallo_CuandoApiFalla()
        {
            //Error 500 from server
            var handlerMock = MockHttpMessageHandler.SetupReturn("", HttpStatusCode.InternalServerError);
            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.fake.com/") };

            var service = new CatService(httpClient, _contextMock.Object, _loggerMock.Object, _validatorMock.Object, _repoMock.Object);

           
            var result = await service.GetBreedByIdAsync("error_id");


            Assert.False(result.IsSuccess);
        }

        #endregion

        #region Tests GetBreedsAsync

        [Fact]
        public async Task GetBreedsAsync_DebeRetornarLista_CuandoApiRespondeOK()
        {
            
            var jsonResponse = "[{ \"id\": \"abys\", \"name\": \"Abyssinian\" }, { \"id\": \"beng\", \"name\": \"Bengal\" }]";
            var handlerMock = MockHttpMessageHandler.SetupReturn(jsonResponse, HttpStatusCode.OK);
            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.fake.com/") };

            var service = new CatService(httpClient, _contextMock.Object, _loggerMock.Object, _validatorMock.Object, _repoMock.Object);

            
            var result = await service.GetBreedsAsync();

            
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
        }

        #endregion

        #region Tests PostBreedAsync

        [Fact]
        public async Task PostBreedAsync_DebeRetornarFallo_CuandoValidacionFalla()
        {
            var dto = new BreedsDto { Name = "" };
            var httpClient = new HttpClient();


            var validationFailure = new ValidationFailure("Name", "El nombre es requerido");
            var validationResult = new ValidationResult(new[] { validationFailure });

            _validatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(validationResult);

            var service = new CatService(httpClient, _contextMock.Object, _loggerMock.Object, _validatorMock.Object, _repoMock.Object);

            var result = await service.PostBreedAsync(dto);

            Assert.False(result.IsSuccess);
            Assert.Contains("No se pudieron validar los datos", result.Message);

            // Verify = NUNCA se intentó guardar en BD
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Breed>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task PostBreedAsync_DebeGuardarYCommitear_CuandoDatosSonValidos()
        {

            var dto = new BreedsDto { Name = "Gato Test", Origin = "Testland" };
            var httpClient = new HttpClient();

            // 1. Validación Exitosa
            _validatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult()); // IsValid = true por defecto

            // 2. Configurar Repositorio para que no falle al agregar
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Breed>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);

            // 3. Configurar SaveChanges del repositorio
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(1);

            var service = new CatService(httpClient, _contextMock.Object, _loggerMock.Object, _validatorMock.Object, _repoMock.Object);


            var result = await service.PostBreedAsync(dto);


            Assert.True(result.IsSuccess);

            // Verify = 
            // 1. Se validó
            _validatorMock.Verify(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
            // 2. Se inició transacción
            _contextMock.Verify(c => c.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            // 3. Se agregó al repositorio
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Breed>(), It.IsAny<CancellationToken>()), Times.Once);
            // 4. Se hizo commit de la transacción
            _transactionMock.Verify(t => t.Commit(), Times.Once);
        }

        [Fact]
        public async Task PostBreedAsync_DebeHacerRollback_CuandoOcurreExcepcion()
        {

            var dto = new BreedsDto { Name = "Gato Error" };
            var httpClient = new HttpClient();

            // Validación Exitosa
            _validatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            // Simulamos que el Repositorio FALLA al guardar
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Breed>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new Exception("Error de base de datos simulado"));

            var service = new CatService(httpClient, _contextMock.Object, _loggerMock.Object, _validatorMock.Object, _repoMock.Object);


            var result = await service.PostBreedAsync(dto);

            Assert.False(result.IsSuccess);
            Assert.Contains("Error de base de datos simulado", result.Message);

            // Verify = Rollback
            _transactionMock.Verify(t => t.Rollback(), Times.Once);
            _transactionMock.Verify(t => t.Commit(), Times.Never);
        }

        #endregion

        #region Tests DeleteBreedAsync

        [Fact]
        public async Task DeleteBreedAsync_DebeEliminarYCommitear_CuandoRegistroExiste()
        {
            var id = Guid.NewGuid();
            var httpClient = new HttpClient();

            _repoMock.Setup(r => r.ExecuteDeleteByIdAsync(id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(1);

            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(1);

            var service = new CatService(httpClient, _contextMock.Object, _loggerMock.Object, _validatorMock.Object, _repoMock.Object);


            var result = await service.DeleteBreedAsync(id);


            Assert.True(result.IsSuccess);
            Assert.Contains("eliminado correctamente", result.Message);

            // Verify =
            // 1. Se inicia la transacción
            _contextMock.Verify(c => c.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);

            // 2. Se llama al método de borrado del repositorio
            _repoMock.Verify(r => r.ExecuteDeleteByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);

            // 3. Se hace commit
            _transactionMock.Verify(t => t.Commit(), Times.Once);
        }

        [Fact]
        public async Task DeleteBreedAsync_DebeRetornarFallo_CuandoRegistroNoExiste()
        {
            var id = Guid.NewGuid();
            var httpClient = new HttpClient();

            _repoMock.Setup(r => r.ExecuteDeleteByIdAsync(id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(0);

            var service = new CatService(httpClient, _contextMock.Object, _loggerMock.Object, _validatorMock.Object, _repoMock.Object);

            var result = await service.DeleteBreedAsync(id);

            Assert.False(result.IsSuccess);
            Assert.Contains("No se encontró", result.Message);

            // Verify = 
            // Return antes de hacer commit, por lo que Commit NUNCA debe llamarse
            _transactionMock.Verify(t => t.Commit(), Times.Never);
            // Tampoco a SaveChanges
            _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteBreedAsync_DebeHacerRollback_CuandoOcurreExcepcion()
        {
            var id = Guid.NewGuid();
            var httpClient = new HttpClient();

            // Simulamos una excepción en el repositorio
            _repoMock.Setup(r => r.ExecuteDeleteByIdAsync(id, It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new Exception("Error crítico de DB"));

            var service = new CatService(httpClient, _contextMock.Object, _loggerMock.Object, _validatorMock.Object, _repoMock.Object);

            var result = await service.DeleteBreedAsync(id);

            Assert.False(result.IsSuccess);
            Assert.Contains("Error crítico de DB", result.Message);

            // Verify = 
            // Al fallar en el try, debe ir al catch y ejecutar Rollback
            _transactionMock.Verify(t => t.Rollback(), Times.Once);
            // Aseguramos que NO se hizo commit
            _transactionMock.Verify(t => t.Commit(), Times.Never);
        }

        #endregion
    }
}