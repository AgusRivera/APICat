using APICat.Application.Interfaces;
using APICat.Application.Services;
using System.Runtime.CompilerServices;

namespace ApiCat.Extensions
{
    public static class ExternalServiceCollectionExtension
    {
        public static void RegisterExternalServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddHttpClient<ICatService, CatService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ApiCat:BaseUrl"));
                client.DefaultRequestHeaders.Add("x-api-key", builder.Configuration.GetValue<string>("ApiCat:ApiKey"));
            });
        }
    }
}
