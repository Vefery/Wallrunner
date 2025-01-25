using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePack;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.HDROutputUtils;

[MessagePackObject]
public class GameData
{
    [Key("recordScore")]
    public int recordScore = 0;
    [Key("primarySkin")]
    public string primarySkinName = "Black";
    [Key("coins")]
    public int coins = 0;
    [Key("keys")]
    public int resurrectionKeys = 0;
    [Key("unlockedSkins")]
    public List<string> unlockedSkins = new() { "Black" };
}
public class GameManager : MonoBehaviour, IDataLoader, IDataFetcher
{
    public int Coins
    {
        get => _coins;
        set => _coins = Mathf.Clamp(value, 0, int.MaxValue);
    }
    public int ResurrectionKeys
    {
        get => _resurrectionKeys;
        set => _resurrectionKeys = Mathf.Clamp(value, 0, int.MaxValue);
    }
    public int resurrectionKeysUsage = 1;

    private AudioMixer masterMixer;
    private AsyncOperationHandle<IngameChannel> ingameChannelOperation;
    private AsyncOperationHandle<AudioMixer> audioMixerOperation;
    private IngameChannel ingameChannel;
    [SerializeField]
    private int _coins;
    [SerializeField]
    private int _resurrectionKeys;
    private async void Awake()
    {
        var gameOverChannelHandle = Addressables.LoadAssetAsync<IngameChannel>("Assets/EventChannels/Ingame Channel.asset");
        gameOverChannelHandle.Completed += OnLoadGameOverChannel_Completed;
        var audioMizerHandle = Addressables.LoadAssetAsync<AudioMixer>("Assets/Sounds/MasterMixer.mixer");
        audioMizerHandle.Completed += OnAudioMixerChannel_Completed;
        await audioMizerHandle;

        UpdateSettings();
        SaveManager.Setup(
            FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<IDataLoader>().ToArray(),
            FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<IDataFetcher>().ToArray()
            );
        SaveManager.Load();
    }

    private void OnAudioMixerChannel_Completed(AsyncOperationHandle<AudioMixer> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            masterMixer = operation.Result;
        }
        else
            Debug.LogError("Failed to load audio mixer!");
        audioMixerOperation = operation;
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
        ResurrectionKeys -= resurrectionKeysUsage;
        resurrectionKeysUsage *= 2;
        ingameChannel.TriggerResurrect(ResurrectionKeys);
        SaveManager.Save();
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
        float musicVolume = 80f * Mathf.Log10(PlayerPrefs.GetFloat("musicVolume", 1f));
        float soundVolume = 80f * Mathf.Log10(PlayerPrefs.GetFloat("soundsVolume", 1f));
        masterMixer.SetFloat("musicVolume", musicVolume);
        masterMixer.SetFloat("soundsVolume", soundVolume);
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

        if (audioMixerOperation.IsValid())
            audioMixerOperation.Release();
    }

    public void LoadData(GameData data)
    {
        Coins = data.coins;
        ResurrectionKeys = data.resurrectionKeys;
    }

    public void FetchData(GameData data)
    {
        data.coins = Coins;
        data.resurrectionKeys = ResurrectionKeys;
    }
}
