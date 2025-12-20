using APICat.Logging.Proxy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APICat.Logging.Factory
{
    public static class LoggingProxyFactory
    {
        public static T Create<T>(T decorated, ILogger logger)
        {
            object proxy = DispatchProxy.Create<T, LoggingProxy<T>>();
            ((LoggingProxy<T>)proxy).Decorated = decorated;
            ((LoggingProxy<T>)proxy).Logger = logger;
            return (T)proxy;
        }
    }
}
