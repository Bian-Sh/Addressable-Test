using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AASceneLoader : MonoBehaviour
{
    public string primaryversion = "1.0.0";
    public string updateversion = "1.0.0";
    public string key = "Assets/Scenes/SampleScene.unity";
    public Button button;
    void Start() => button.onClick.AddListener(OnButtonClicked);

    private async void OnButtonClicked()
    {
        VersionProvider.Version = updateversion;
        AddressablesRuntimeProperties.ClearCachedPropertyValues();
        await Addressables.InitializeAsync();
       // var host = AddressablesRuntimeProperties.EvaluateString("http://[PrivateIpAddress]:[HostingServicePort]/{VersionProvider.Version}");
        var host = AddressablesRuntimeProperties.EvaluateString("http://127.0.0.1:8080/{VersionProvider.Version}");
        Debug.Log($"{nameof(AASceneLoader)}: host = {host}");
        var catalog = $"{host}/catalog_{primaryversion}.json";
        var locator = await Addressables.LoadContentCatalogAsync(catalog, true);
        Addressables.ClearResourceLocators();
        Addressables.AddResourceLocator(locator);
        var size = await Addressables.GetDownloadSizeAsync(key);
        Debug.Log($"{nameof(AASceneLoader)}: size = {size}");
        await Addressables.LoadSceneAsync(key, LoadSceneMode.Single);
        Debug.Log($"{nameof(AASceneLoader)}: scene loaded");
    }
}
