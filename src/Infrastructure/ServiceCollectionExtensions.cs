﻿using Infrastructure.Common;
using Infrastructure.Identity;
using Infrastructure.Middleware;
using Infrastructure.Presistence;
using Infrastructure.Presistence.Context;
using Infrastructure.Presistence.Database.Initializer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class ServiceCollectionExtensions
{

    /// <summary>
    /// 注册基础服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        return services
                  .AddExceptionMiddleware()
                  .AddIdentity()
                  .AddHealthCheck()
                  .AddPersistence(config)
                  .AddServices();
    }
    /// <summary>
    /// 调用基础服务
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder builder, IConfiguration config)
    {
        return builder
                    .UseExceptionMiddleware()
                    .UseAuthentication()
                    .UseAuthorization();

    }
    public static IServiceCollection AddIdentity(
        this IServiceCollection services)
    {
        return services
                    .AddIdentity<ApplicationUser, ApplicationRole>(option =>
                    {
                        option.Password.RequiredLength = 6;
                        option.Password.RequireDigit = false;
                        option.Password.RequireLowercase = false;
                        option.Password.RequireNonAlphanumeric = false;
                        option.Password.RequireUppercase = false;
                        option.User.RequireUniqueEmail = false;
                    })
               .AddEntityFrameworkStores<ApplicationDbContext>()
               .AddDefaultTokenProviders().Services;
    }
    public static async Task InitializeDatabasesAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        // Create a new scope to retrieve scoped services
        using var scope = services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>()
            .InitializeDatabasesAsync(cancellationToken);
    }
    private static IServiceCollection AddHealthCheck(this IServiceCollection services) =>
      services.AddHealthChecks().Services;
    /// <summary>
    /// 终结点配置
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapControllers().RequireAuthorization();
        builder.MapHealthCheck();
        return builder;
    }
    /// <summary>
    /// 健康检查路由地址,验证后才能访问
    /// </summary>
    /// <param name="endpoints"></param>
    /// <returns></returns>
    private static IEndpointConventionBuilder MapHealthCheck(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapHealthChecks("/api/ping").RequireAuthorization();
}
