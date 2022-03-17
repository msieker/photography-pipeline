using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace PhotoPipeline.Framework.Blocks;
public interface IPipelineBlock
{
    string BlockName { get; }
    int BlockVersion { get; }

    Task<PipelinePhoto?> Run(PipelinePhoto photo, CancellationToken token);
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
    public static IServiceCollection AddPipelineBlocks(this IServiceCollection services)
    {
        var baseType = typeof(IPipelineBlock);

        var implementations = baseType.Assembly.GetTypes()
            .Where(t => baseType.IsAssignableFrom(t) && !t.IsInterface);

        foreach (var i in implementations)
        {
            services.AddTransient(baseType, i);
        }

        return services;
    }
}