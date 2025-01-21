using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[RequireComponent(typeof(MenuManager))]
public class IngameUIHandler : MonoBehaviour, IDataLoader, IDataFetcher
{
    public TMP_Text ingameScoreText;
    public TMP_Text finalScoreText;
    public TMP_Text recordScoreText;
    public TMP_Text coinsText;
    public int CurrentScore { get => (int)_currentScore; }

    private MenuManager menuManager;
    private IngameChannel ingameChannel;
    private float _currentScore;
    private int recordScore;
    private float speed = 0f;
    private bool isScoreStopped = false;
    private int collectedCoins = 0;
    private AsyncOperationHandle<IngameChannel> ingameChannelOperation;
    private void Awake()
    {
        speed = FindFirstObjectByType<LevelManager>().levelSpeed / 2f;
        menuManager = GetComponent<MenuManager>();
        var gameOverChannelHandle = Addressables.LoadAssetAsync<IngameChannel>("Assets/EventChannels/Ingame Channel.asset");
        gameOverChannelHandle.Completed += OnLoadGameOverChannel_Completed;
    }
    private void Start()
    {
        menuManager.OpenMenu("GamePanel");
    }
    private void Update()
    {
        if (isScoreStopped)
            return;

        _currentScore += Time.deltaTime * speed;
        ingameScoreText.SetText($"Score: {CurrentScore}m");
    }
    private void OnGameOver()
    {
        isScoreStopped = true;
        menuManager.OpenMenu("GameOver");
        finalScoreText.SetText($"Your score\n{CurrentScore}m");
        recordScoreText.SetText($"Record score\n{recordScore}");
    }
    public void LoadData(GameData data)
    {
        recordScore = data.RecordScore;
    }
    private void OnCollectedCoin(int amount)
    {
        collectedCoins += amount;
        coinsText.SetText($"Coins: {collectedCoins}");
    }
    public void FetchData(GameData data)
    {
        if (CurrentScore > recordScore)
            data.RecordScore = CurrentScore;
    }
    private void OnLoadGameOverChannel_Completed(AsyncOperationHandle<IngameChannel> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            ingameChannel = operation.Result;
            ingameChannel.OnGameOver.AddListener(OnGameOver);
            ingameChannel.OnResurrect.AddListener(() => isScoreStopped = false);
            ingameChannel.OnPause.AddListener((isPaused) => isScoreStopped = isPaused);
            ingameChannel.OnCollectedCoin.AddListener(OnCollectedCoin);
        }
        else
            Debug.LogError("Failed to load base parts of the level!");
        ingameChannelOperation = operation;
    }
    private void OnDestroy()
    {
        if (ingameChannelOperation.IsValid())
            ingameChannelOperation.Release();
    }
}
