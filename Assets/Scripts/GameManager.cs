using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public string primaryversion = "1.0.0";
    public string updateversion = "1.0.0";
    public string host = "http://127.0.0.1:8080";
    public string key = "Assets/Scenes/SampleScene.unity";
    public Button load_bt;
    public Button back_bt;

    #region Internal Singleton
    static GameManager _instance;
    private void Awake()
    {
        if (_instance && _instance.gameObject != gameObject)
        {
            DestroyImmediate(gameObject);
        }
        else if (!_instance)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    #endregion

    void Start()
    {
        load_bt.onClick.AddListener(OnButtonClicked);
        back_bt.onClick.AddListener(OnBackButtonClicked);
    }

    private async void OnBackButtonClicked()
    {
        await Addressables.UnloadSceneAsync(sceneInstance);
        load_bt.gameObject.SetActive(true);
        back_bt.gameObject.SetActive(false);
        await SceneManager.LoadSceneAsync("InitScene");
    }

    private SceneInstance sceneInstance;
    private async void OnButtonClicked()
    {
        load_bt.gameObject.SetActive(false);
        VersionProvider.Version = updateversion;
        AddressablesRuntimeProperties.ClearCachedPropertyValues();
        await Addressables.InitializeAsync();
        var catalog = $"{host}/{VersionProvider.Version}/catalog_{primaryversion}.json";
        Debug.Log($"{nameof(GameManager)}: catalog = {catalog}");
        var locator = await Addressables.LoadContentCatalogAsync(catalog, true);
        Addressables.ClearResourceLocators();
        Addressables.AddResourceLocator(locator);
        var size = await Addressables.GetDownloadSizeAsync(key);
        Debug.Log($"{nameof(GameManager)}: size = {size}");
        sceneInstance = await Addressables.LoadSceneAsync(key, LoadSceneMode.Single);
        Debug.Log($"{nameof(GameManager)}: scene loaded");
        back_bt.gameObject.SetActive(true);
    }
}
