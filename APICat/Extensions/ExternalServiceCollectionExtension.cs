public static class ExternalServiceCollectionExtension
{
    public static void RegisterExternalServices(this WebApplicationBuilder builder)
    {
        var baseUrl = builder.Configuration["ApiCat:BaseUrl"];
        var apiKey = builder.Configuration["ApiCat:ApiKey"];

        if (string.IsNullOrEmpty(baseUrl)) throw new ArgumentNullException("ApiCat:BaseUrl");

        builder.Services.AddHttpClient("ApiExt", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        });
    }
}