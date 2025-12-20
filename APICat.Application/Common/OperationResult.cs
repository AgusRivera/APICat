using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APICat.Application.Common
{
    public class OperationResult : IOperationResult
    {
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
        public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

        public static IOperationResult Success(string? message = null)
        {
            return new OperationResult()
            {
                IsSuccess = true,
                Message = message
            };
        }

        public static IOperationResult<TObject> Success<TObject>(TObject value, string? message = null)
        {
            return new OperationResult<TObject>()
            {
                IsSuccess = true,
                Message = message,
                Value = value
            };
        }

        public static IOperationResult Fail(string? errorMessage = null)
        {
            return new OperationResult()
            {
                IsSuccess = false,
                Message = errorMessage
            };
        }

        public static IOperationResult Fail(string? errorMessage, IEnumerable<string> errors)
        {
            return new OperationResult()
            {
                IsSuccess = false,
                Message = errorMessage,
                Errors = errors
            };
        }

        public static IOperationResult<TObject> Fail<TObject>(string? errorMessage, IEnumerable<string> errors)
        {
            return new OperationResult<TObject>()
            {
                IsSuccess = false,
                Message = errorMessage,
                Errors = errors,
                Value = default
            };
        }

        public static IOperationResult<TObject> Fail<TObject>(string? errorMessage)
        {
            return new OperationResult<TObject>()
            {
                IsSuccess = false,
                Message = errorMessage,
                Value = default
            };
        }
    }

    public class OperationResult<TObject> : OperationResult, IOperationResult<TObject>
    {
        public TObject? Value { get; set; }
    }
}
