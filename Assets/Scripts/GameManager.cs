using System;
using System.Collections;
using System.IO;
using System.Linq;
using MessagePack;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

[MessagePackObject]
public class GameData
{
    private int _recordScore = 0;
    [Key("recordScore")]
    public int RecordScore
    {
        get => _recordScore; 
        set
        {
            if (value >= 0 && value <= int.MaxValue)
                _recordScore = value;
        }
    }
}
public class GameManager : MonoBehaviour
{
    private AudioSource[] musicSources;
    private AudioSource[] soundSources;
    private AsyncOperationHandle<OnGameOverChannel> gameOverChannelOperation;
    private OnGameOverChannel gameOverChannel;
    private string savePath;
    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "SaveData");
        musicSources = GameObject.FindGameObjectsWithTag("MusicSource").Select(x => x.GetComponent<AudioSource>()).ToArray();
        soundSources = GameObject.FindGameObjectsWithTag("SoundSource").Select(x => x.GetComponent<AudioSource>()).ToArray();

        var gameOverChannelHandle = Addressables.LoadAssetAsync<OnGameOverChannel>("Assets/EventChannels/GameOver Channel.asset");
        gameOverChannelHandle.Completed += OnLoadGameOverChannel_Completed;

        UpdateSettings();
        SaveManager.Setup(FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<IObjectWithData>().ToArray());
        SaveManager.Load();
    }
    private void OnLoadGameOverChannel_Completed(AsyncOperationHandle<OnGameOverChannel> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            gameOverChannel = operation.Result;
        }
        else
            Debug.LogError("Failed to load base parts of the level!");
        gameOverChannelOperation = operation;
    }
    public void RestartGame()
    {
        SaveManager.Save();
        gameOverChannel.TriggerRestartGame();
        RestartLevel();
    }
    public void Resurrect()
    {
        gameOverChannel.TriggerResurrect();
    }
    public void GoToMenu()
    {
        SaveManager.Save();
        LoadLevel("Menu");
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
        if (gameOverChannelOperation.IsValid())
            gameOverChannelOperation.Release();
    }
}
