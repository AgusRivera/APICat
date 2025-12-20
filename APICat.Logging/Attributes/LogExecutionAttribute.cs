using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APICat.Logging.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LogExecutionAttribute : Attribute
    {
        public string Message { get; }
        public LogExecutionAttribute(string message = "")
        {
            Message = message;
        }
    }
}
