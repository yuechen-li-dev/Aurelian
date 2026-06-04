namespace Aurelian.Graphics.Vulkan.Presentation;

public static class VulkanPresentationDiagnosticCodes
{
    public const string WindowCreationUnavailable = "AGPR1001";
    public const string SurfaceCreationFailed = "AGPR1002";
    public const string SwapchainExtensionMissing = "AGPR1003";
    public const string SurfaceSupportMissing = "AGPR1004";
    public const string NoSurfaceFormats = "AGPR1005";
    public const string NoPresentModes = "AGPR1006";
    public const string SwapchainCreationFailed = "AGPR1007";
    public const string SwapchainImageQueryFailed = "AGPR1008";
    public const string ImageViewCreationFailed = "AGPR1009";
    public const string AcquireFailed = "AGPR1010";
    public const string PresentFailed = "AGPR1011";
    public const string PresentationDisposed = "AGPR1012";
    public const string HeadlessEnvironment = "AGPR1013";
    public const string AcquirePresentDeferred = "AGPR1014";
}
