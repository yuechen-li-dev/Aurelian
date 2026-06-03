namespace Aurelian.Shaders.Language.External.Dxc;

public static class DxcDiscovery
{
    public static DxcExecutable? FindDxc()
    {
        var configured = Environment.GetEnvironmentVariable("AURELIAN_DXC");
        if (!string.IsNullOrWhiteSpace(configured) && IsExecutableFile(configured))
        {
            return new DxcExecutable(Path.GetFullPath(configured));
        }

        return FindOnPath("dxc") ?? FindOnPath("dxc.exe");
    }

    private static DxcExecutable? FindOnPath(string fileName)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try
            {
                var candidate = Path.Combine(directory, fileName);
                if (IsExecutableFile(candidate))
                {
                    return new DxcExecutable(Path.GetFullPath(candidate));
                }
            }
            catch (ArgumentException)
            {
                // Ignore malformed PATH entries. Discovery must never make normal tests fail.
            }
            catch (NotSupportedException)
            {
                // Ignore malformed PATH entries. Discovery must never make normal tests fail.
            }
        }

        return null;
    }

    private static bool IsExecutableFile(string path)
    {
        try
        {
            return File.Exists(path);
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }
}
