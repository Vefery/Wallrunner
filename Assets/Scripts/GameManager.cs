using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

[MessagePackObject]
public class GameData
{
    private int _recordScore = 0;
    [Key("primarySkin")]
    public string primarySkinName = "Black";
    [Key("coins")]
    public int coins = 0;
    [Key("unlockedSkins")]
    public List<string> unlockedSkins = new() { "Black" };
    [Key("recordScore")]
    public int RecordScore
    {
        get => _recordScore;
        set => _recordScore = Mathf.Clamp(value, 0, int.MaxValue);
    }
}
public class GameManager : MonoBehaviour, IDataLoader, IDataFetcher
{
    public int Coins
    {
        get => _coins;
        set => _coins = Mathf.Clamp(value, 0, int.MaxValue);
    }
    private AudioSource[] musicSources;
    private AudioSource[] soundSources;
    private AsyncOperationHandle<IngameChannel> ingameChannelOperation;
    private IngameChannel ingameChannel;
    [SerializeField]
    private int _coins;
    private void Awake()
    {
        musicSources = GameObject.FindGameObjectsWithTag("MusicSource").Select(x => x.GetComponent<AudioSource>()).ToArray();
        soundSources = GameObject.FindGameObjectsWithTag("SoundSource").Select(x => x.GetComponent<AudioSource>()).ToArray();

        var gameOverChannelHandle = Addressables.LoadAssetAsync<IngameChannel>("Assets/EventChannels/Ingame Channel.asset");
        gameOverChannelHandle.Completed += OnLoadGameOverChannel_Completed;

        UpdateSettings();
        SaveManager.Setup(
            FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<IDataLoader>().ToArray(),
            FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<IDataFetcher>().ToArray()
            );
        SaveManager.Load();
    }
    private void OnLoadGameOverChannel_Completed(AsyncOperationHandle<IngameChannel> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            ingameChannel = operation.Result;
            ingameChannel.OnCollectedCoin.AddListener((value) => Coins += value);
        }
        else
            Debug.LogError("Failed to load base parts of the level!");
        ingameChannelOperation = operation;
    }
    public void RestartGame()
    {
        SaveManager.Save();
        ingameChannel.TriggerRestartGame();
        RestartLevel();
    }
    public void Resurrect()
    {
        ingameChannel.TriggerResurrect();
    }
    public void GoToMenu()
    {
        SaveManager.Save();
        LoadLevel("Menu");
    }
    public void Pause(bool isPaused)
    {
        ingameChannel.TriggerPause(isPaused);
    }
    public void UpdateSettings()
    {
        float musicVolume = PlayerPrefs.GetFloat("musicVolume", 1f);
        float soundVolume = PlayerPrefs.GetFloat("soundsVolume", 1f);
        foreach (AudioSource source in musicSources)
            source.volume = musicVolume;
        foreach (AudioSource source in soundSources)
            source.volume = soundVolume;
        Application.targetFrameRate = Convert.ToBoolean(PlayerPrefs.GetInt("batterySaveMode", 1)) ? 30 : 60;
    }
    private void RestartLevel()
    {
        Addressables.LoadSceneAsync($"Assets/Scenes/{SceneManager.GetActiveScene().name}.unity", LoadSceneMode.Single);
    }
    private void LoadLevel(string levelName)
    {
        Addressables.LoadSceneAsync($"Assets/Scenes/{levelName}.unity", LoadSceneMode.Single);
    }
    private void OnDestroy()
    {
        if (ingameChannelOperation.IsValid())
            ingameChannelOperation.Release();
    }

    public void LoadData(GameData data)
    {
        Coins = data.coins;
    }

    public void FetchData(GameData data)
    {
        data.coins = Coins;
    }
}
