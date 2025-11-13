using Api.JiraIntegration.Options;
using FastEndpoints;
using FluentValidation.Results;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;

namespace Api.Options;

[PublicAPI]
public class JiraEnabledFlag(IOptions<JiraOptions> options) : IFeatureFlag
{
    public async Task<bool> IsEnabledAsync(IEndpoint endpoint)
    {
        if (!string.IsNullOrEmpty(options.Value.ClientId) &&
            !string.IsNullOrEmpty(options.Value.ClientSecret)) return true;
        
        await endpoint.HttpContext.Response.SendErrorsAsync([
            new ValidationFailure("featureDisabled", "Jira is not enabled in the current instance")
        ]); 
        return false;

    }

    public string? Name { get; set; }
}