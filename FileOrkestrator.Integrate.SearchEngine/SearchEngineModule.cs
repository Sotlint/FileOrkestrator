using FileOrkestrator.Integrate.SearchEngine.Generated;
using FileOrkestrator.Integrate.SearchEngine.Mocks;
using FileOrkestrator.Integrate.SearchEngine.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;

namespace FileOrkestrator.Integrate.SearchEngine;

/// <summary>
/// Регистрация Refit-клиента и скоупного <see cref="ISearchEngineClient"/> в DI.
/// </summary>
public static class SearchEngineModule
{
    /// <summary>
    /// Регистрирует Refit-клиент или mock в зависимости от <see cref="Options.SearchEngineOptions.IsTest"/>.
    /// </summary>
    public static IServiceCollection AddSearchEngineIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        // Режим без реального HTTP — для локальной разработки и тестов.
        var isTest = configuration.GetValue<bool>($"{SearchEngineOptions.SectionName}:IsTest");

        services
            .AddOptions<SearchEngineOptions>()
            .Bind(configuration.GetSection(SearchEngineOptions.SectionName))
            .Validate(
                o => o.IsTest || Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out _),
                "SearchEngine:BaseUrl must be an absolute URI when IsTest is false.")
            .ValidateOnStart();

        if (isTest)
        {
            services.AddScoped<ISearchEngineClient, MockSearchEngineClient>();
            return services;
        }

        services
            .AddRefitClient<ISearchEngineApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<SearchEngineOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = options.RequestTimeout;

                client.DefaultRequestHeaders.Remove(SearchEngineOptions.ApiKeyHeaderName);
                if (!string.IsNullOrWhiteSpace(options.ApiKey))
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        SearchEngineOptions.ApiKeyHeaderName,
                        options.ApiKey);
                }
            });

        services.AddScoped<ISearchEngineClient, SearchEngineClient>();

      

        return services;
    }
}
