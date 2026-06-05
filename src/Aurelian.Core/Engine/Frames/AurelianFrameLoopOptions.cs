namespace Aurelian.Core.Engine.Frames;

public sealed record AurelianFrameLoopOptions(
    int? MaxFrames = 1,
    bool PresentAfterCompletedFrame = true,
    bool StopOnFrameFailure = true);
