using Cysharp.Threading.Tasks;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public string primaryversion = "1.0.0";
    public string currentVersion = "1.0.0";
    public string host = "http://127.0.0.1:8080";
    public string key = "Assets/Scenes/SampleScene.unity";
    public Button load_bt;
    public Button back_bt;
    public TMP_Dropdown dropdown;

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
        dropdown.onValueChanged.AddListener(OnDropDownValueChanged);
        load_bt.onClick.AddListener(OnButtonClicked);
        back_bt.onClick.AddListener(OnBackButtonClicked);
    }

    private void OnDropDownValueChanged(int arg0)
    {
        currentVersion = dropdown.options[arg0].text;
    }

    private async void OnBackButtonClicked()
    {
        await Addressables.UnloadSceneAsync(sceneInstance);
        load_bt.gameObject.SetActive(true);
        dropdown.gameObject.SetActive(true);
        back_bt.gameObject.SetActive(false);
        await SceneManager.LoadSceneAsync("InitScene");
    }

    private SceneInstance sceneInstance;
    private async void OnButtonClicked()
    {
        dropdown.gameObject.SetActive(false);
        load_bt.gameObject.SetActive(false);
        back_bt.gameObject.SetActive(true);
        VersionProvider.Version = currentVersion;
        AddressablesRuntimeProperties.ClearCachedPropertyValues();
        await Addressables.InitializeAsync();

        var catalog = $"{host}/{VersionProvider.Version}/catalog_{primaryversion}.json";
        Debug.Log($"{nameof(GameManager)}: catalog = {catalog}");
        var locator = await Addressables.LoadContentCatalogAsync(catalog, true);
        Addressables.ClearResourceLocators();
        Addressables.AddResourceLocator(locator);

        // 在加载新的 catalog 之前清理缓存
        // 保证每次demo中 GetDownloadSizeAsync() 返回的值不是 0
        // 请留意，此API 的调用仅用于测试
        // 调用前，请先保证 locator 已经载入
        var result = Caching.ClearCache();
        Debug.Log($"{nameof(GameManager)}:  {(result ? "清理缓存成功" : "清理缓存失败,something in use!")}");
        //var handler = Addressables.CleanBundleCache();
        //if (handler.IsValid())
        //{
        //    await handler;
        //}

        var size = await Addressables.GetDownloadSizeAsync(key);
        Debug.Log($"{nameof(GameManager)}:  资源大小 ：{size / 1024} KB");

        var depHandler = Addressables.DownloadDependenciesAsync(key, true);
        var cachedSize = 0f;
        while (!depHandler.IsDone)
        {
            var state = depHandler.GetDownloadStatus();
            var speed = (state.DownloadedBytes - cachedSize) / 1024 / Time.deltaTime;
            cachedSize = state.DownloadedBytes;
            // 获取 addressables 的下载进度、下载速度
            Debug.Log($"{nameof(GameManager)}: 下载中 {state.Percent:0.00%} - {speed} KB/s");
            await UniTask.Yield();
        }
        Debug.Log($"{nameof(GameManager)}: 下载完成，正在进入场景！");
        sceneInstance = await Addressables.LoadSceneAsync(key, LoadSceneMode.Single);
        Debug.Log($"{nameof(GameManager)}: 场景加载完成！");
    }
}
