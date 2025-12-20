using APICat.Application.Common;
using APICat.Application.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APICat.Application.Interfaces
{
    public interface ICatService
    {
        Task<IOperationResult<List<BreedsDto>>> GetBreedsAsync();
        Task<IOperationResult<BreedsDto>> GetBreedByIdAsync(string id);
        Task<IOperationResult> PostBreedAsync(BreedsDto breed);
        Task<IOperationResult> DeleteBreedAsync(Guid id);
    }
}
