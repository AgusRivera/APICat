namespace APICat.Extensions
{
    public static class WebHostEnvironmentExtension
    {
        public static bool IsDevelopment(this IWebHostEnvironment host)
        {
            return host.IsEnvironment("Local") || host.IsEnvironment("Development");
        }
    }
}
