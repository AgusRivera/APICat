using APICat.Logging.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APICat.Logging.Proxy
{
    public class LoggingProxy<T> : DispatchProxy
    {
        public T Decorated { get; set; }
        public ILogger Logger { get; set; }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            // Buscar el método real en la clase concreta
            var methodImpl = Decorated.GetType().GetMethod(
                targetMethod.Name,
                targetMethod.GetParameters().Select(p => p.ParameterType).ToArray()
            );

            var logAttr = methodImpl?.GetCustomAttribute<LogExecutionAttribute>();
            if (logAttr == null)
            {
                try
                {
                    return targetMethod.Invoke(Decorated, args);
                }
                catch (TargetInvocationException ex)
                {
                    Logger.LogError(ex.InnerException, $"[ERROR] Método: {targetMethod.Name} falló sin atributo LogExecution.");
                    throw ex.InnerException;
                }
            }

            var tipo = Decorated.GetType().Name;
            var methodName = targetMethod.Name;
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var taskId = Task.CurrentId?.ToString() ?? "No Task";

            Logger.LogInformation($"[INICIO] Servicio: {tipo}, Método: {methodName}, ThreadId: {threadId}, TaskId: {taskId}, Mensaje: {logAttr.Message}");

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = targetMethod.Invoke(Decorated, args);

                if (result is Task taskResult)
                {
                    var returnType = targetMethod.ReturnType;
                    if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        var resultType = returnType.GenericTypeArguments[0];
                        var method = typeof(LoggingProxy<T>)
                            .GetMethod(nameof(HandleAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Instance)
                            .MakeGenericMethod(resultType);
                        return method.Invoke(this, new object[] { result, tipo, methodName, stopwatch, logAttr.Message });
                    }

                    return HandleAsync(taskResult, tipo, methodName, stopwatch, logAttr.Message);
                }

                stopwatch.Stop();
                Logger.LogInformation($"[FIN] Servicio: {tipo}, Método: {methodName}, Duración: {stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            catch (TargetInvocationException ex)
            {
                stopwatch.Stop();
                Logger.LogError(ex.InnerException, $"[ERROR] Servicio: {tipo}, Método: {methodName}, Duración: {stopwatch.ElapsedMilliseconds}ms");
                throw ex.InnerException;
            }
        }

        private async Task HandleAsync(Task task, string tipo, string methodName, Stopwatch stopwatch, string message)
        {
            try
            {
                await task.ConfigureAwait(false);
                stopwatch.Stop();
                Logger.LogInformation($"[FIN] Servicio: {tipo}, Método: {methodName}, Duración: {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Logger.LogError(ex, $"[ERROR] Servicio: {tipo}, Método: {methodName}, Duración: {stopwatch.ElapsedMilliseconds}ms, Mensaje: {message}");
                throw;
            }
        }

        private async Task<TRes> HandleAsyncGeneric<TRes>(Task<TRes> task, string tipo, string methodName, Stopwatch stopwatch, string message)
        {
            try
            {
                var result = await task.ConfigureAwait(false);
                stopwatch.Stop();
                Logger.LogInformation($"[FIN] Servicio: {tipo}, Método: {methodName}, Duración: {stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Logger.LogError(ex, $"[ERROR] Servicio: {tipo}, Método: {methodName}, Duración: {stopwatch.ElapsedMilliseconds}ms, Mensaje: {message}");
                throw;
            }
        }
    }
}
