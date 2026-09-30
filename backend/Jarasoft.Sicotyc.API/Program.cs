using Jarasoft.Sicotyc.API.Exceptions;
using Jarasoft.Sicotyc.API.OpenApi;
using Jarasoft.Sicotyc.Application;
using Jarasoft.Sicotyc.Infrastructure;
using Jarasoft.Sicotyc.Infrastructure.Identity;
using Jarasoft.Sicotyc.Infrastructure.Persistence;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

//builder.Services.AddOpenApi();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer(
        (document, context, cancellationToken) =>
        {
            document.Components ??=
                new OpenApiComponents();

            document.Components.SecuritySchemes ??=
                new Dictionary<
                    string,
                    IOpenApiSecurityScheme>();

            document.Components.SecuritySchemes["Bearer"] =
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description =
                        "JWT obtenido desde /api/auth/login."
                };

            return Task.CompletedTask;
        });
    options.AddOperationTransformer<
        BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

if (app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();

    var context =
        scope.ServiceProvider
            .GetRequiredService<SicotycDbContext>();

    await context.Database.EnsureCreatedAsync();
}

await IdentitySeeder.SeedRolesAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "CRM-Sicotyc API v1"
            );
    });
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

// Este es el orden: Primero determinamos quién es el usuario y después evaluamos qué puede hacer.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


