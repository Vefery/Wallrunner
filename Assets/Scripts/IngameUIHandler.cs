using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[RequireComponent(typeof(MenuManager))]
public class IngameUIHandler : MonoBehaviour, IObjectWithData
{
    public TMP_Text ingameScoreText;
    public TMP_Text finalScoreText;
    public TMP_Text recordScoreText;
    public int CurrentScore { get => (int)_currentScore; }

    private MenuManager menuManager;
    private OnGameOverChannel gameOverChannel;
    private float _currentScore;
    private int recordScore;
    private float speed = 0f;
    private bool isGameOver = false;
    private AsyncOperationHandle<OnGameOverChannel> gameOverChannelOperation;
    private void Awake()
    {
        speed = FindFirstObjectByType<LevelManager>().levelSpeed / 2f;
        menuManager = GetComponent<MenuManager>();
        var gameOverChannelHandle = Addressables.LoadAssetAsync<OnGameOverChannel>("Assets/EventChannels/GameOver Channel.asset");
        gameOverChannelHandle.Completed += OnLoadGameOverChannel_Completed;
    }
    private void Start()
    {
        menuManager.OpenMenu("GamePanel");
    }
    private void Update()
    {
        if (isGameOver)
            return;

        _currentScore += Time.deltaTime * speed;
        ingameScoreText.SetText($"Score: {CurrentScore}m");
    }
    public void OnGameOver()
    {
        isGameOver = true;
        menuManager.OpenMenu("Loading");
    }
    public void OnRevertableGameOver()
    {
        isGameOver = true;
        menuManager.OpenMenu("GameOver");
        finalScoreText.SetText($"Your score\n{CurrentScore}m");
        recordScoreText.SetText($"Record score\n{recordScore}");
    }
    public void LoadData(GameData data)
    {
        recordScore = data.RecordScore;
    }

    public void SaveData(GameData data)
    {
        if (CurrentScore > recordScore)
            data.RecordScore = CurrentScore;
    }
    private void OnLoadGameOverChannel_Completed(AsyncOperationHandle<OnGameOverChannel> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            gameOverChannel = operation.Result;
            gameOverChannel.OnRevertableGameOver += OnRevertableGameOver;
            gameOverChannel.OnGameOver += OnGameOver;
        }
        else
            Debug.LogError("Failed to load base parts of the level!");
        gameOverChannelOperation = operation;
    }
    private void OnDestroy()
    {
        if (gameOverChannelOperation.IsValid())
            gameOverChannelOperation.Release();
    }
}
