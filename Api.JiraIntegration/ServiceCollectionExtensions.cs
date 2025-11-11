using Api.JiraIntegration.Clients;
using Api.JiraIntegration.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Api.JiraIntegration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureJira(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<JiraOptions>(configuration.GetSection(JiraOptions.SectionName));
    }
    
    public static IServiceCollection AddJiraClients(this IServiceCollection services,  IConfiguration configuration)
    {
        var jiraOptions = configuration.GetSection(JiraOptions.SectionName).Get<JiraOptions>() ?? new JiraOptions();
        services.AddRefitClient<IJiraApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri(jiraOptions.ApiBaseUrl));
        services.AddRefitClient<IJiraAuthApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri(jiraOptions.AuthBaseUrl));
        return services;
    }
    
    
}