using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APICat.Application.Common
{
    public interface IOperationResult
    {
        string? Message { get; set; }
        bool IsSuccess { get; set; }
        IEnumerable<string> Errors { get; set; }
    }

    public interface IOperationResult<TObject> : IOperationResult
    {
        TObject? Value { get; set; }
    }
}
