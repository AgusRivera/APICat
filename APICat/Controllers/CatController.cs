using APICat.Application.Common;
using APICat.Application.Interfaces;
using APICat.Application.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APICat.Controllers
{
    /// <summary>
    /// Controlador para la gestión de razas de gatos.
    /// </summary>
    /// <remarks>
    /// **Autenticación:**
    /// Este controlador requiere un token JWT válido.
    /// 
    /// 1. Obtenga el token en POST /api/auth/Login
    /// 2. Copie el token.
    /// 3. Haga clic en el botón **Authorize** (arriba a la derecha).
    /// 4. Pegue el token y confirme.
    /// </remarks>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class CatController : ControllerBase
    {
        private readonly ICatService _catService;

        public CatController(ICatService catServices)
        {
            _catService = catServices;
        }

        /// <summary>
        /// Obtiene todas las razas de gatos disponibles en API externa.
        /// </summary>
        /// <returns>Lista de razas.</returns>
        /// <response code="200">Retorna la lista de razas exitosamente.</response>
        /// <response code="401">No autorizado. Token faltante o inválido.</response>
        [HttpGet]
        [Route("Breeds")]
        [ProducesResponseType(typeof(IOperationResult<List<BreedsDto>>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IOperationResult<BreedsDto>>> GetBreeds()
        {
            var result = await _catService.GetBreedsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Obtiene todas la raza de gato segun el ID especificado en API externa.
        /// </summary>
        /// <returns>Lista de razas.</returns>
        /// <response code="200">Retorna la lista de razas exitosamente.</response>
        /// <response code="401">No autorizado. Token faltante o inválido.</response>
        [HttpGet]
        [Route("BreedById")]
        [ProducesResponseType(typeof(IOperationResult<List<BreedsDto>>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IOperationResult<BreedsDto>>> GetBreedById(string id)
        {
            var result = await _catService.GetBreedByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Crea una nueva raza de gato en la base de datos.
        /// </summary>
        /// <param name="breeds">Datos de la raza a crear.</param>
        /// <remarks>
        /// Ejemplo de Request:
        /// 
        ///     POST /Cat/Insert
        ///     {
        ///        "name": "Gato Galáctico",
        ///        "origin": "Marte",
        ///        "temperament": "Curioso",
        ///        "description": "Un gato que brilla en la oscuridad"
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("Insert")]
        [ProducesResponseType(typeof(IOperationResult), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IOperationResult<BreedsDto>>> InsertBreed(BreedsDto breeds)
        {
            var result = await _catService.PostBreedAsync(breeds);
            return Ok(result);
        }
    }
}
