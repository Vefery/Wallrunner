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
using static GameData;

[MessagePackObject]
public class GameData
{
    [MessagePackObject]
    public struct OwnedItemPair
    {
        [Key(0)]
        public string name;
        [Key(1)]
        public int owned;
    }
    [Key("recordScore")]
    public int recordScore = 0;
    [Key("primarySkin")]
    public string primarySkinName = "Default";
    [Key("coins")]
    public int coins = 0;
    [Key("ownedItemsDict")]
    public Dictionary<string, OwnedItemPair> ownedItemsDict = new();
    [Key("unlockedSkins")]
    public List<string> unlockedSkins = new() { "Default" };
}
public class GameManager : MonoBehaviour, IDataLoader, IDataFetcher
{
    public int Coins
    {
        get => _coins;
        set => _coins = Mathf.Clamp(value, 0, int.MaxValue);
    }
    public int resurrectionKeysUsage = 1;

    private AudioMixer masterMixer;
    private AsyncOperationHandle<IngameChannel> ingameChannelHandle;
    private AsyncOperationHandle<AudioMixer> audioMixerHandle;
    private IngameChannel ingameChannel;
    private Dictionary<string, OwnedItemPair> ownedItemsDict = new();
    [SerializeField]
    private int _coins;
    private async void Awake()
    {
        SaveManager.Setup(
            FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<IDataLoader>().ToArray(),
            FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<IDataFetcher>().ToArray()
        );
        SaveManager.Load();

        ingameChannelHandle = Addressables.LoadAssetAsync<IngameChannel>("Assets/EventChannels/Ingame Channel.asset");
        ingameChannelHandle.Completed += OnLoadGameOverChannel_Completed;
        audioMixerHandle = Addressables.LoadAssetAsync<AudioMixer>("Assets/Sounds/MasterMixer.mixer");
        audioMixerHandle.Completed += OnAudioMixerChannel_Completed;
        await audioMixerHandle;

        UpdateSettings();
    }

    private void OnAudioMixerChannel_Completed(AsyncOperationHandle<AudioMixer> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            masterMixer = operation.Result;
        }
        else
            Debug.LogError("Failed to load audio mixer!");
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
    }
    public void RestartGame()
    {
        SaveManager.Save();
        ingameChannel.TriggerRestartGame();
        RestartLevel();
    }
    public bool UseItem(string itemName, int quantity = 1)
    {
        bool succsess = false;
        if (ownedItemsDict.TryGetValue(itemName, out OwnedItemPair item))
        {
            if (item.owned >= quantity)
            {
                succsess = true;
                item.owned -= quantity;
                if (item.owned == 0)
                    ownedItemsDict.Remove(itemName);
                else
                    ownedItemsDict[itemName] = item;
            }
        }

        return succsess;
    }
    public int GetItemQuantity(string itemName)
    {
        if (ownedItemsDict.TryGetValue(itemName, out OwnedItemPair item))
            return item.owned;
        else
            return 0;
    }
    public void Resurrect()
    {
        if (UseItem("resurrectionKey", resurrectionKeysUsage))
        {
            resurrectionKeysUsage *= 2;
            ingameChannel.TriggerResurrect(GetItemQuantity("resurrectionKey"));
            SaveManager.Save();
        }
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
        if (ingameChannelHandle.IsValid())
            ingameChannelHandle.Release();

        if (audioMixerHandle.IsValid())
            audioMixerHandle.Release();
    }

    public void LoadData(GameData data)
    {
        Coins = data.coins;
        ownedItemsDict = data.ownedItemsDict;
    }

    public void FetchData(GameData data)
    {
        data.coins = Coins;
        data.ownedItemsDict = ownedItemsDict;
    }
}
