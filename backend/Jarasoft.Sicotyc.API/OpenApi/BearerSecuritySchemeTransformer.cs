using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Jarasoft.Sicotyc.API.OpenApi;

public sealed class BearerSecuritySchemeTransformer
    : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var endpointMetadata =
            context.Description.ActionDescriptor
                .EndpointMetadata;

        var hasAuthorize =
            endpointMetadata
                .OfType<IAuthorizeData>()
                .Any();

        var allowAnonymous =
            endpointMetadata
                .OfType<IAllowAnonymous>()
                .Any();

        if (!hasAuthorize || allowAnonymous)
        {
            return Task.CompletedTask;
        }

        operation.Security ??=
            new List<OpenApiSecurityRequirement>();

        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference(
                        "Bearer",
                        context.Document)
                ] = []
            });

        return Task.CompletedTask;
    }
}