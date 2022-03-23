using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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


internal class PhotoCommandBuilder
{
    public static PhotoCommandBuilder<THandler, TArgs> Create<THandler, TArgs>(string name, string? description = null) where THandler : ICommandHandler<TArgs>
    {
        return new PhotoCommandBuilder<THandler, TArgs>(name, description);
    }
}

internal class PhotoCommandBuilder<THandler, TArgs> where THandler : ICommandHandler<TArgs>
{
    private readonly string _name;
    private readonly string? _description;

    private readonly List<Option> _options = new();

    public PhotoCommandBuilder(string name, string? description = null)
    {
        _name = name;
        _description = description;
    }

    public PhotoCommandBuilder<THandler, TArgs> AddOption(Option option)
    {
        _options.Add(option);
        return this;
    }

    public Command Create()
    {
        var cmd = new Command(_name, _description);

        foreach (var option in _options)
            cmd.AddOption(option);

        cmd.Handler = CommandHandler.Create<TArgs, IHost, CancellationToken>(async (args, host, token) =>
        {
            await using var scope = host.Services.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<THandler>();
            await handler.Handle(args, token);
        });

        return cmd;
    }
}
