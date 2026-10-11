using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;

namespace ECommerce.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "E-Commerce Web API",
                Version = "v1",
                Description = "Production-Grade E-Commerce REST API with JWT Bearer Authentication and Clean Architecture."
            });

            // Define JWT Bearer security scheme
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter JWT Bearer token: Bearer {token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            options.AddSecurityDefinition("Bearer", securityScheme);

            var securityRequirement = new OpenApiSecurityRequirement
            {
                {
                    securityScheme,
                    Array.Empty<string>()
                }
            };

            options.AddSecurityRequirement(securityRequirement);

            // Group controllers nicely in OpenAPI document
            options.TagActionsBy(api =>
            {
                if (api.GroupName != null)
                {
                    return new[] { api.GroupName };
                }

                var controllerActionDescriptor = api.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
                if (controllerActionDescriptor != null)
                {
                    var controllerName = controllerActionDescriptor.ControllerName;
                    if (controllerName.StartsWith("Admin"))
                    {
                        return new[] { $"Admin: {controllerName.Substring(5)}" };
                    }
                    return new[] { controllerName };
                }

                return new[] { "Default" };
            });

            options.DocInclusionPredicate((name, api) => true);
        });

        return services;
    }
}
