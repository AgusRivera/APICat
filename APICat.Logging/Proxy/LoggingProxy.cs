using APICat.Logging.Attributes;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace APICat.Logging.Proxy
{
    public class LoggingProxy<T> : DispatchProxy
    {
        public T Decorated { get; set; }
        public ILogger Logger { get; set; }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            try
            {
                // Busqueda.
                // Interfaz.
                var logAttr = targetMethod.GetCustomAttribute<LogExecutionAttribute>();

                // Implementación (Clase concreta)
                if (logAttr == null)
                {
                    try
                    {
                        var implementationMethod = Decorated.GetType().GetMethod(
                            targetMethod.Name,
                            targetMethod.GetParameters().Select(p => p.ParameterType).ToArray()
                        );

                        if (implementationMethod != null)
                        {
                            logAttr = implementationMethod.GetCustomAttribute<LogExecutionAttribute>();
                        }
                    }
                    catch (Exception debugEx)
                    {
                        // Si falla la reflexión, solo lo mostramos en consola de debug, no rompemos la app
                        Console.WriteLine($">> DEBUG PROXY: Error buscando atributo en implementación: {debugEx.Message}");
                    }
                }

                // CASO A: Si después de buscar, sigue siendo NULL -> Ejecutar sin log
                if (logAttr == null)
                {
                    // Habilita esta línea si quieres ver qué métodos se están escapando
                    // Console.WriteLine($">> DEBUG PROXY: El método {targetMethod.Name} NO tiene atributo [LogExecution]. Se ejecuta sin log.");
                    return targetMethod.Invoke(Decorated, args);
                }

                // CASO B: Tiene atributo -> Ejecutar con Log
                return InvokeWithLogging(targetMethod, args, logAttr);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"[PROXY ERROR] Falló el proxy interno para {targetMethod.Name}");
                throw;
            }
        }

        private object InvokeWithLogging(MethodInfo targetMethod, object[] args, LogExecutionAttribute logAttr)
        {
            var tipo = Decorated.GetType().Name;
            var methodName = targetMethod.Name;
            var message = logAttr.Message;

            Logger.LogInformation($"[INICIO] Servicio: {tipo}, Método: {methodName} | Mensaje: {message}");


            Console.WriteLine($">> DEBUG SERILOG: [INICIO] {methodName}");

            var stopwatch = Stopwatch.StartNew();

            var result = targetMethod.Invoke(Decorated, args);

            // Manejo de Asincronía (Tasks)
            if (result is Task taskResult)
            {
                var returnType = targetMethod.ReturnType;

                // Si devuelve Task<T>
                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var resultType = returnType.GenericTypeArguments[0];
                    var method = typeof(LoggingProxy<T>)
                        .GetMethod(nameof(HandleAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(resultType);

                    return method.Invoke(this, new object[] { result, tipo, methodName, stopwatch });
                }

                // Si devuelve Task (void)
                return HandleAsync(taskResult, tipo, methodName, stopwatch);
            }

            // Manejo Síncrono
            stopwatch.Stop();
            Logger.LogInformation($"[FIN] Servicio: {tipo}, Método: {methodName} | Duración: {stopwatch.ElapsedMilliseconds}ms");
            return result;
        }

        // --- Helpers para esperar Tasks ---

        private async Task HandleAsync(Task task, string tipo, string methodName, Stopwatch stopwatch)
        {
            try
            {
                await task.ConfigureAwait(false);
                stopwatch.Stop();
                Logger.LogInformation($"[FIN] Servicio: {tipo}, Método: {methodName} | Duración: {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($">> DEBUG SERILOG: [FIN] {methodName} ({stopwatch.ElapsedMilliseconds}ms)");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Logger.LogError(ex, $"[ERROR] Servicio: {tipo}, Método: {methodName} falló tras {stopwatch.ElapsedMilliseconds}ms");
                throw;
            }
        }

        private async Task<TRes> HandleAsyncGeneric<TRes>(Task<TRes> task, string tipo, string methodName, Stopwatch stopwatch)
        {
            try
            {
                var result = await task.ConfigureAwait(false);
                stopwatch.Stop();
                Logger.LogInformation($"[FIN] Servicio: {tipo}, Método: {methodName} | Duración: {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($">> DEBUG SERILOG: [FIN] {methodName} ({stopwatch.ElapsedMilliseconds}ms)");
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Logger.LogError(ex, $"[ERROR] Servicio: {tipo}, Método: {methodName} falló tras {stopwatch.ElapsedMilliseconds}ms");
                throw;
            }
        }
    }
}


//using APICat.Logging.Attributes;
//using Microsoft.Extensions.Logging;
//using System.Diagnostics;
//using System.Reflection;

//namespace APICat.Logging.Proxy
//{
//    public class LoggingProxy<T> : DispatchProxy
//    {
//        public T Decorated { get; set; }
//        public ILogger Logger { get; set; }

//        protected override object Invoke(MethodInfo targetMethod, object[] args)
//        {
//            try
//            {
//                // 1.INTERFAZ
//                var logAttr = targetMethod.GetCustomAttribute<LogExecutionAttribute>();

//                // 2.IMPLEMENTACIÓN
//                if (logAttr == null)
//                {
//                    var implementationMethod = GetImplementationMethod(targetMethod);
//                    if (implementationMethod != null)
//                    {
//                        logAttr = implementationMethod.GetCustomAttribute<LogExecutionAttribute>();
//                    }
//                }

//                // CASO A: No tiene atributo -> Ejecutar sin log
//                if (logAttr == null)
//                {
//                    return targetMethod.Invoke(Decorated, args);
//                }

//                // CASO B: Tiene atributo -> Ejecutar con Log
//                return InvokeWithLogging(targetMethod, args, logAttr);
//            }
//            catch (TargetInvocationException ex)
//            {
//                throw ex.InnerException ?? ex;
//            }
//            catch (Exception ex)
//            {
//                Logger.LogError(ex, $"[PROXY ERROR] Falló el proxy para {targetMethod.Name}");
//                throw;
//            }
//        }

//        private object InvokeWithLogging(MethodInfo targetMethod, object[] args, LogExecutionAttribute logAttr)
//        {
//            var tipo = Decorated.GetType().Name;
//            var methodName = targetMethod.Name;

//            var correlationId = Guid.NewGuid().ToString().Substring(0, 8);

//            Logger.LogInformation($"[INICIO] [{correlationId}] Servicio: {tipo}, Método: {methodName}. Mensaje: {logAttr.Message}");

//            var stopwatch = Stopwatch.StartNew();


//            var result = targetMethod.Invoke(Decorated, args);

//            // Manejo de Asincronía (Task y Task<T>)
//            if (result is Task taskResult)
//            {
//                var returnType = targetMethod.ReturnType;

//                // Si es Task<T>
//                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
//                {
//                    var resultType = returnType.GenericTypeArguments[0];
//                    var method = typeof(LoggingProxy<T>)
//                        .GetMethod(nameof(HandleAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Instance)
//                        .MakeGenericMethod(resultType);

//                    return method.Invoke(this, new object[] { result, tipo, methodName, stopwatch, correlationId });
//                }

//                // Si es solo Task (void asíncrono)
//                return HandleAsync(taskResult, tipo, methodName, stopwatch, correlationId);
//            }

//            // Manejo Síncrono
//            stopwatch.Stop();
//            Logger.LogInformation($"[FIN] [{correlationId}] Servicio: {tipo}, Método: {methodName}. Duración: {stopwatch.ElapsedMilliseconds}ms");
//            return result;
//        }


//        // --- Helpers

//        private async Task HandleAsync(Task task, string tipo, string methodName, Stopwatch stopwatch, string correlationId)
//        {
//            try
//            {
//                await task.ConfigureAwait(false);
//                stopwatch.Stop();
//                Logger.LogInformation($"[FIN] [{correlationId}] Servicio: {tipo}, Método: {methodName}. Duración: {stopwatch.ElapsedMilliseconds}ms");
//            }
//            catch (Exception ex)
//            {
//                stopwatch.Stop();
//                Logger.LogError(ex, $"[ERROR] [{correlationId}] Servicio: {tipo}, Método: {methodName}. Falló tras {stopwatch.ElapsedMilliseconds}ms");
//                throw;
//            }
//        }

//        private async Task<TRes> HandleAsyncGeneric<TRes>(Task<TRes> task, string tipo, string methodName, Stopwatch stopwatch, string correlationId)
//        {
//            try
//            {
//                var result = await task.ConfigureAwait(false);
//                stopwatch.Stop();
//                Logger.LogInformation($"[FIN] [{correlationId}] Servicio: {tipo}, Método: {methodName}. Duración: {stopwatch.ElapsedMilliseconds}ms");
//                return result;
//            }
//            catch (Exception ex)
//            {
//                stopwatch.Stop();
//                Logger.LogError(ex, $"[ERROR] [{correlationId}] Servicio: {tipo}, Método: {methodName}. Falló tras {stopwatch.ElapsedMilliseconds}ms");
//                throw;
//            }
//        }

//        // --- Helper de Reflexión (FIX) ---

//        private MethodInfo GetImplementationMethod(MethodInfo interfaceMethod)
//        {
//            // Esta lógica mapea el método de la interfaz al método exacto de la clase concreta
//            // Usar GetMethod por nombre es frágil. GetInterfaceMap es robusto.
//            try
//            {
//                var map = Decorated.GetType().GetInterfaceMap(interfaceMethod.DeclaringType);
//                var index = Array.IndexOf(map.InterfaceMethods, interfaceMethod);
//                return map.TargetMethods[index];
//            }
//            catch
//            {
//                // Fallback por si acaso (o si no implementa la interfaz directamente, aunque DispatchProxy obliga a ello)
//                return Decorated.GetType().GetMethod(interfaceMethod.Name,
//                    interfaceMethod.GetParameters().Select(p => p.ParameterType).ToArray());
//            }
//        }
//    }
//}