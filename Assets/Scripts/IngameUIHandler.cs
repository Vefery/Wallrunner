using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

[RequireComponent(typeof(MenuManager))]
public class IngameUIHandler : MonoBehaviour, IDataLoader, IDataFetcher
{
    public TMP_Text ingameScoreText;
    public TMP_Text finalScoreText;
    public TMP_Text recordScoreText;
    public TMP_Text coinsText;
    public TMP_Text resurrectionKeysText;
    public Button resurrectButton;
    public int CurrentScore { get => (int)_currentScore; }

    private MenuManager menuManager;
    private GameManager gameManager;
    private IngameChannel ingameChannel;
    private float _currentScore;
    private int recordScore;
    private float speed = 0f;
    private bool isScoreStopped = false;
    private int collectedCoins = 0;
    private AsyncOperationHandle<IngameChannel> ingameChannelHandle;
    private void Awake()
    {
        speed = FindFirstObjectByType<LevelManager>().levelSpeed / 2f;
        gameManager = FindFirstObjectByType<GameManager>();
        menuManager = GetComponent<MenuManager>();
        ingameChannelHandle = Addressables.LoadAssetAsync<IngameChannel>("Assets/EventChannels/Ingame Channel.asset");
        ingameChannelHandle.Completed += OnLoadGameOverChannel_Completed;
    }
    private void Start()
    {
        menuManager.OpenMenu("GamePanel");
        resurrectionKeysText.SetText($"Keys:\n{gameManager.ResurrectionKeys}");
    }
    private void Update()
    {
        if (isScoreStopped)
            return;

        _currentScore = Mathf.Clamp(_currentScore + Time.deltaTime * speed, 0, int.MaxValue);
        ingameScoreText.SetText($"Score: {CurrentScore}m");
    }
    private void OnGameOver()
    {
        isScoreStopped = true;
        resurrectButton.interactable = gameManager.ResurrectionKeys >= gameManager.resurrectionKeysUsage;
        menuManager.OpenMenu("GameOver");
        finalScoreText.SetText($"Your score\n{CurrentScore}m");
        recordScoreText.SetText($"Record score\n{recordScore}");
    }
    public void LoadData(GameData data)
    {
        recordScore = data.recordScore;
    }
    private void OnCollectedCoin(int amount)
    {
        collectedCoins += amount;
        coinsText.SetText($"Coins: {collectedCoins}");
    }
    private void OnResurrect(int keysLeft)
    {
        isScoreStopped = false;
        resurrectionKeysText.SetText($"Keys:\n{keysLeft}");
    }
    private void OnPause(bool isPaused)
    {
        isScoreStopped = isPaused;
        if (isPaused)
            menuManager.OpenMenu("Pause");
        else
            menuManager.OpenMenu("GamePanel");
    }
    public void FetchData(GameData data)
    {
        if (CurrentScore > recordScore)
            data.recordScore = CurrentScore;
    }
    private void OnLoadGameOverChannel_Completed(AsyncOperationHandle<IngameChannel> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            ingameChannel = operation.Result;
            ingameChannel.OnGameOver.AddListener(OnGameOver);
            ingameChannel.OnResurrect.AddListener(OnResurrect);
            ingameChannel.OnPause.AddListener(OnPause);
            ingameChannel.OnCollectedCoin.AddListener(OnCollectedCoin);
        }
        else
            Debug.LogError("Failed to load base parts of the level!");
    }
    private void OnDestroy()
    {
        if (ingameChannelHandle.IsValid())
            ingameChannelHandle.Release();
    }
}
