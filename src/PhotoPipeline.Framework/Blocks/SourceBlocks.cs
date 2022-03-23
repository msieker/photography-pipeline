using System.Threading.Tasks.Dataflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhotoPipeline.Common;
using PhotoPipeline.Database;
using PhotoPipeline.Database.Entities;

namespace PhotoPipeline.Framework.Blocks;
public class SourceBlocks
{
    private readonly ILogger<SourceBlocks> _logger;
    private readonly PhotoPipelineConfig _config;
    private readonly IServiceProvider _serviceProvider;

    public SourceBlocks(ILogger<SourceBlocks> logger, PhotoPipelineConfig config, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _config = config;
        _serviceProvider = serviceProvider;
    }
    private ExecutionDataflowBlockOptions MakeOptions(CancellationToken token) => new()
    {
        MaxDegreeOfParallelism = _config.MaxParallelism,
        BoundedCapacity = _config.MaxParallelism * 2,
        EnsureOrdered = false,
        CancellationToken = token
    };

    public TransformManyBlock<string, string> ListDirectory(CancellationToken token = default)
        => new(path => Sources.ListDirectory.Action(path, _logger, token),
            new ExecutionDataflowBlockOptions { CancellationToken = token, EnsureOrdered = false });

    public TransformBlock<string, PipelinePhoto> ReadFile(CancellationToken token = default)
        => new(path => Sources.ReadFile.Action(path, _logger, token),
            MakeOptions(token));

    public DatabaseSourceBlock FromDatabase(Func<IQueryable<Photo>,IQueryable<Photo>> filterQuery, CancellationToken token = default) 
        => new(_serviceProvider, filterQuery, MakeOptions(token));
}

public class DatabaseSourceBlock : ISourceBlock<PipelinePhoto>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<IQueryable<Photo>,IQueryable<Photo>> _filterQuery;
    private readonly BufferBlock<PipelinePhoto> _buffer;

    public DatabaseSourceBlock(IServiceProvider serviceProvider, Func<IQueryable<Photo>,IQueryable<Photo>> filterQuery, DataflowBlockOptions options)
    {
        _buffer = new BufferBlock<PipelinePhoto>(new DataflowBlockOptions{ BoundedCapacity = 200, EnsureOrdered = false, CancellationToken = options.CancellationToken});
        _serviceProvider = serviceProvider;
        _filterQuery = filterQuery;
    }

    public async Task RunQuery(CancellationToken token = default)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<PhotoDbContext>();

        var transform = new TransformBlock<Photo, PipelinePhoto>((p) => new PipelinePhoto(p), new ExecutionDataflowBlockOptions {CancellationToken = token});

        var query = context.Photos
            .Include(p => p.PipelineSteps)
            .Include(p => p.Hashes)
            .Include(p => p.Metadata)
            .AsSplitQuery()
            .AsQueryable();

        query = _filterQuery(query);
        var results = await query.ToListAsync(token);
        foreach (var p in results)
        {
            await _buffer.SendAsync(new PipelinePhoto(p), token);
            //_buffer.Post(new PipelinePhoto(p));
        }
        Complete();
    }

    public void Complete() => _buffer.Complete();
    public void Fault(Exception exception) => ((IDataflowBlock)_buffer).Fault(exception);
    public Task Completion => _buffer.Completion;

    PipelinePhoto? ISourceBlock<PipelinePhoto>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<PipelinePhoto> target, out bool messageConsumed)
        => ((ISourceBlock<PipelinePhoto>)_buffer).ConsumeMessage(messageHeader, target, out messageConsumed);

    IDisposable ISourceBlock<PipelinePhoto>.LinkTo(ITargetBlock<PipelinePhoto> target, DataflowLinkOptions linkOptions)
        => _buffer.LinkTo(target, linkOptions);

    void ISourceBlock<PipelinePhoto>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<PipelinePhoto> target)
        => ((ISourceBlock<PipelinePhoto>)_buffer).ReleaseReservation(messageHeader, target);

    bool ISourceBlock<PipelinePhoto>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<PipelinePhoto> target)
        => ((ISourceBlock<PipelinePhoto>)_buffer).ReserveMessage(messageHeader, target);
}