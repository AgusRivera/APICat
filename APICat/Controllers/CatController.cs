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
        /// Obtiene todas las razas de gatos registradas en la base de datos local.
        /// </summary>
        /// <returns>Lista de razas.</returns>
        /// <response code="200">Retorna la lista de razas exitosamente.</response>
        /// <response code="401">No autorizado.</response>
        [HttpGet]
        [Route("GetAllFromDb")]
        [ProducesResponseType(typeof(IOperationResult<List<BreedsDto>>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IOperationResult<List<BreedsDto>>>> GetAllFromDb()
        {
            var result = await _catService.GetAllBreedsFromDbAsync();

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

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

        /// <summary>
        /// Elimina una raza de gato de la base de datos local por su ID (Guid).
        /// </summary>
        /// <param name="id">El GUID de la raza a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        /// <response code="200">El registro fue eliminado o se procesó la solicitud.</response>
        /// <response code="404">No se encontró el registro con ese ID.</response>
        /// <response code="401">No autorizado.</response>
        [HttpDelete]
        [Route("Delete/{id}")]
        [ProducesResponseType(typeof(IOperationResult), 200)]
        [ProducesResponseType(typeof(IOperationResult), 404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IOperationResult>> DeleteBreed(Guid id)
        {
            var result = await _catService.DeleteBreedAsync(id);

            if (!result.IsSuccess)
            {
                if (result.Message.Contains("No se encontró"))
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
