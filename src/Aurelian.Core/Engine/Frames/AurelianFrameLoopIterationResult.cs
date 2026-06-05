namespace Aurelian.Core.Engine.Frames;

public sealed record AurelianFrameLoopIterationResult(
    AurelianFrameId FrameId,
    AurelianFrameResult FrameResult,
    bool Presented);
