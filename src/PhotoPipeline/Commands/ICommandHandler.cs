using Microsoft.Extensions.DependencyInjection;

namespace PhotoPipeline.Commands;

internal interface ICommandHandler<in TArgs>
{
    Task Handle(TArgs args, CancellationToken token);
}

public static class ServiceCollectionExtensions
{
    private static IEnumerable<Type> GetAllTypes(Type genericType)
    {
        if (!genericType.IsGenericTypeDefinition)
            throw new ArgumentException("Specified type must be a generic type definition.", nameof(genericType));

        return genericType.Assembly
            .GetTypes()
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType &&
                          i.GetGenericTypeDefinition() == genericType));
    }
    public static IServiceCollection AddCommandHandlers(this IServiceCollection services)
    {
        var baseType = typeof(ICommandHandler<>);

        var implementations = GetAllTypes(baseType);

        foreach (var i in implementations)
        {
            services.AddScoped(i);
        }

        return services;
    }
}