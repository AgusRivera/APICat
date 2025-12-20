using APICat.Application.Common;
using APICat.Application.Interfaces;
using APICat.Application.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APICat.Controllers
{
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

        [HttpGet]
        [Route("Breeds")]
        public async Task<ActionResult<IOperationResult<BreedsDto>>> GetBreeds()
        {
            var result = await _catService.GetBreedsAsync();
            return Ok(result);
        }

        [HttpGet]
        [Route("BreedById")]
        public async Task<ActionResult<IOperationResult<BreedsDto>>> GetBreedById(string id)
        {
            var result = await _catService.GetBreedByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<ActionResult<IOperationResult<BreedsDto>>> InsertBreed(BreedsDto breeds)
        {
            var result = await _catService.PostBreedAsync(breeds);
            return Ok(result);
        }
    }
}
