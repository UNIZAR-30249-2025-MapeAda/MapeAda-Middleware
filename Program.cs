using System.Text;
using FluentValidation;
using MapeAda_Middleware;
using MapeAda_Middleware.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MapeAda_Middleware.Configuration;
using MapeAda_Middleware.SharedModels.Users;
using MapeAda_Middleware.Swagger;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using SystemTextJsonPatch.Converters;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AuthConfiguration>(builder.Configuration.GetRequiredSection(Constants.AuthConfigurationKey));
builder.Services.Configure<BackendConfiguration>(builder.Configuration.GetRequiredSection(Constants.BackendConfigurationKey));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MapeAda public API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce el token JWT con el esquema 'Bearer' (sin comillas). Ejemplo: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
    
    options.DocumentFilter<JsonPatchDocumentFilter>();
    options.EnableAnnotations();
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        AuthConfiguration jwtOptions = builder.Configuration.GetRequiredSection(Constants.AuthConfigurationKey).Get<AuthConfiguration>()!;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Constants.GerenteOnlyPolicyName, policy =>
        policy.RequireClaim(Constants.JwtRolKey, Rol.Gerente.ToString(), Rol.GerenteDocenteInvestigador.ToString()));
});


builder.Services.AddValidatorsFromAssemblyContaining<IApiMarker>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.RegisterEndpointsFromAssemblyContaining<IApiMarker>();
builder.Services.AddHttpClient(Constants.BackendHttpClientName, (serviceProvider, client) =>
{
    BackendConfiguration backendOptions = serviceProvider.GetRequiredService<IOptions<BackendConfiguration>>().Value;

    client.BaseAddress = new Uri(backendOptions.BaseUrl);
});

builder.Services.ConfigureHttpJsonOptions(opts =>
    opts.SerializerOptions.Converters.Add(new JsonPatchDocumentConverterFactory()));

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API V1");
    c.RoutePrefix = string.Empty;
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(exceptionApp =>
    {
        exceptionApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            
            IExceptionHandlerPathFeature? exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

            ProblemDetails problem = new()
            {
                Title = "Ha ocurrido un error inesperado.",
                Status = StatusCodes.Status500InternalServerError,
                Detail = exceptionHandlerPathFeature?.Error.Message,
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problem);
        });
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();