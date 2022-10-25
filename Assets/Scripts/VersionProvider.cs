using UnityEngine;

public static class VersionProvider
{
    static string version;
    public static string Version
    {
        get
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings.OverridePlayerVersion;
            }
            else
#endif
                return version;
        }
        set => version = value;
    }
}
