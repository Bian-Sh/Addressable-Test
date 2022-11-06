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

        // �ڼ����µ� catalog ֮ǰ������
        // ��֤ÿ��demo�� GetDownloadSizeAsync() ���ص�ֵ���� 0
        // �����⣬��API �ĵ��ý����ڲ���
        // ����ǰ�����ȱ�֤ locator �Ѿ�����
        var result = Caching.ClearCache();
        Debug.Log($"{nameof(GameManager)}:  {(result ? "������ɹ�" : "������ʧ��,something in use!")}");
        //var handler = Addressables.CleanBundleCache();
        //if (handler.IsValid())
        //{
        //    await handler;
        //}

        var size = await Addressables.GetDownloadSizeAsync(key);
        Debug.Log($"{nameof(GameManager)}:  ��Դ��С ��{size / 1024} KB");

        var depHandler = Addressables.DownloadDependenciesAsync(key, true);
        var cachedSize = 0f;
        while (!depHandler.IsDone)
        {
            var state = depHandler.GetDownloadStatus();
            var speed = (state.DownloadedBytes - cachedSize) / 1024 / Time.deltaTime;
            cachedSize = state.DownloadedBytes;
            // ��ȡ addressables �����ؽ��ȡ������ٶ�
            Debug.Log($"{nameof(GameManager)}: ������ {state.Percent:0.00%} - {speed} KB/s");
            await UniTask.Yield();
        }
        Debug.Log($"{nameof(GameManager)}: ������ɣ����ڽ��볡����");
        sceneInstance = await Addressables.LoadSceneAsync(key, LoadSceneMode.Single);
        Debug.Log($"{nameof(GameManager)}: ����������ɣ�");
    }
}
