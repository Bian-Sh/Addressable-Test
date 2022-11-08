using System.IO;
using UnityEditor.AddressableAssets.HostingServices;

/// <summary>
/// 简易 webserver，仅仅 hosting  ServerData/[BuildTarget] 根目录
/// </summary>
public class MyWebServer : HttpHostingService
{
    /// <inheritdoc/>
    public override void StartHostingService()
    {
        var root = $"ServerData/{UnityEditor.BuildTarget.StandaloneWindows64}";
        HostingServiceContentRoots.Clear();
        HostingServiceContentRoots.Add(root);
        base.StartHostingService();
    }

    /// <inheritdoc/>
    protected override string FindFileInContentRoots(string relativePath)
    {
        var root = $"ServerData/{UnityEditor.BuildTarget.StandaloneWindows64}";
        relativePath = relativePath.TrimStart('/');
        relativePath = relativePath.TrimStart('\\');
        var fullPath = Path.Combine(root, relativePath).Replace('\\', '/');
        if (File.Exists(fullPath))
            return fullPath;
        return null;
    }
}
