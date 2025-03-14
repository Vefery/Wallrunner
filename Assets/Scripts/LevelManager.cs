using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEngine.Rendering.HDROutputUtils;

public class LevelManager : MonoBehaviour
{
    public Transform playerStartingPosition;
    public Transform levelPartBufferPosition;
    public float levelPartDistanceLimit;
    public float levelSpeed;
    public bool isLevelPaused = true;

    private float firstPartHalfLength;
    private List<LevelPart> activeLevelParts = new();
    private IList<GameObject> levelPartPrefabs;
    private LevelPart upcomingPart;
    private AsyncOperationHandle<IList<GameObject>> levelPartsOperation;
    private AsyncOperationHandle<IngameChannel> ingameChannelOperation;
    private IngameChannel ingameChannel;
    private GameObject RandomPartPrefab { get => levelPartPrefabs[Random.Range(0, levelPartPrefabs.Count)]; }

    private void Awake()
    {
        levelPartsOperation = Addressables.LoadAssetsAsync<GameObject>("BaseLevelParts");
        levelPartsOperation.Completed += OnLoadBasePartsHandle_Completed;

        ingameChannelOperation = Addressables.LoadAssetAsync<IngameChannel>("Assets/EventChannels/Ingame Channel.asset");
        ingameChannelOperation.Completed += OnLoadGameOverChannel_Completed;
    }
    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
            activeLevelParts.Add(transform.GetChild(i).GetComponent<LevelPart>());
        firstPartHalfLength = activeLevelParts[0].halfLength;
    }
    private void Update()
    {
        if (!isLevelPaused)
        {
            foreach (LevelPart part in activeLevelParts)
            {
                part.transform.Translate(levelSpeed * Time.deltaTime * -Vector3.forward, Space.Self);
            }
            if (activeLevelParts[0].transform.localPosition.z < -(firstPartHalfLength + levelPartDistanceLimit))
                UpdateLevelParts();
        }
    }
    public void OnGameOver()
    {
        isLevelPaused = true;
    }
    public void PauseLevel(bool isPaused)
    {
        isLevelPaused = isPaused;
    }
    public void OnResurrect(int keysLeft)
    {
        Collider[] nearObstacles = Physics.OverlapSphere(activeLevelParts[0].transform.position, activeLevelParts[0].halfLength + 10, layerMask: LayerMask.GetMask("Obstacles"));
        foreach (Collider collider in nearObstacles)
            Destroy(collider.gameObject);

        isLevelPaused = false;
    }
    private void UpdateLevelParts()
    {
        LevelPart lastPart = activeLevelParts.Last();
        Destroy(activeLevelParts[0].gameObject);
        activeLevelParts.RemoveAt(0);
        upcomingPart.transform.localPosition = lastPart.transform.position + Vector3.forward * (lastPart.halfLength + upcomingPart.halfLength);
        activeLevelParts.Add(upcomingPart);
        lastPart = upcomingPart;
        firstPartHalfLength = activeLevelParts[0].halfLength;
        upcomingPart = Instantiate(RandomPartPrefab, levelPartBufferPosition.position, Quaternion.identity, transform).GetComponent<LevelPart>();
        upcomingPart.lastTrajectoryElement = lastPart.lastTrajectoryElement;
        upcomingPart.consequentForward = lastPart.consequentForward;
    }
    private void OnLoadBasePartsHandle_Completed(AsyncOperationHandle<IList<GameObject>> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            isLevelPaused = false;
            levelPartPrefabs = operation.Result;
            upcomingPart = Instantiate(RandomPartPrefab, levelPartBufferPosition.position, Quaternion.identity, transform).GetComponent<LevelPart>();
            upcomingPart.lastTrajectoryElement = activeLevelParts[2].lastTrajectoryElement;
            upcomingPart.consequentForward = activeLevelParts[2].consequentForward;
        }
        else
            Debug.LogError("Failed to load base parts of the level!");
    }
    private void OnLoadGameOverChannel_Completed(AsyncOperationHandle<IngameChannel> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            ingameChannel = operation.Result;
            ingameChannel.OnGameOver.AddListener(OnGameOver);
            ingameChannel.OnResurrect.AddListener(OnResurrect);
            ingameChannel.OnPause.AddListener(PauseLevel);
        }
        else
            Debug.LogError("Failed to load base parts of the level!");
    }
    private void OnDestroy()
    {
        if (levelPartsOperation.IsValid())
            levelPartsOperation.Release();
        if (ingameChannelOperation.IsValid())
            ingameChannelOperation.Release();
    }
}
