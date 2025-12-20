using APICat.Logging.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APICat.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        string? GenerateToken(string username, string password);
    }
}
