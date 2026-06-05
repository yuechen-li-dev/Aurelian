using Aurelian.Core.Engine.Frames;

namespace Aurelian.VisibleTriangle;

internal sealed class VisibleTriangleFrameInputProvider : IAurelianFrameInputProvider
{
    private readonly Func<AurelianFrameId, AurelianFrameInput> inputFactory;
    private readonly int maxFrames;
    private int suppliedFrames;

    public VisibleTriangleFrameInputProvider(AurelianFrameInput preparedInput)
        : this(_ => preparedInput, maxFrames: 1)
    {
    }

    public VisibleTriangleFrameInputProvider(
        Func<AurelianFrameId, AurelianFrameInput> inputFactory,
        int maxFrames = 1)
    {
        ArgumentNullException.ThrowIfNull(inputFactory);
        if (maxFrames <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxFrames), "Visible triangle frame input provider must supply at least one frame input.");

        this.inputFactory = inputFactory;
        this.maxFrames = maxFrames;
    }

    public ValueTask<AurelianFrameInput?> GetNextFrameInputAsync(
        AurelianFrameId frameId,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<AurelianFrameInput?>(cancellationToken);
        }

        if (suppliedFrames >= maxFrames)
        {
            return ValueTask.FromResult<AurelianFrameInput?>(null);
        }

        suppliedFrames++;
        return ValueTask.FromResult<AurelianFrameInput?>(inputFactory(frameId));
    }
}
