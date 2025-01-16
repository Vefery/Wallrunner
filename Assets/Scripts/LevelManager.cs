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
    private GameObject RandomPartPrefab { get => levelPartPrefabs[Random.Range(0, levelPartPrefabs.Count)]; }

    private void Awake()
    {
        AsyncOperationHandle<IList<GameObject>> loadBasePartsHandle = Addressables.LoadAssetsAsync<GameObject>("BaseLevelParts");
        loadBasePartsHandle.Completed += OnLoadBasePartsHandle_Completed;
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
        levelPartsOperation.Release();
    }
    public void OnRevertableGameOver()
    {
        isLevelPaused = true;
    }
    private void UpdateLevelParts()
    {
        LevelPart lastPart = activeLevelParts.Last();
        float lastHalfLength = lastPart.halfLength;
        Destroy(activeLevelParts[0].gameObject);
        activeLevelParts.RemoveAt(0);
        upcomingPart.transform.localPosition = lastPart.transform.position + Vector3.forward * (lastHalfLength + upcomingPart.halfLength);
        activeLevelParts.Add(upcomingPart);
        firstPartHalfLength = activeLevelParts[0].halfLength;
        upcomingPart = Instantiate(RandomPartPrefab, levelPartBufferPosition.position, Quaternion.identity, transform).GetComponent<LevelPart>();
    }
    private void OnLoadBasePartsHandle_Completed(AsyncOperationHandle<IList<GameObject>> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            isLevelPaused = false;
            levelPartPrefabs = operation.Result;
            upcomingPart = Instantiate(RandomPartPrefab, levelPartBufferPosition.position, Quaternion.identity, transform).GetComponent<LevelPart>();
        }
        else
            Debug.LogError("Failed to load base parts of the level!");
        levelPartsOperation = operation;
    }
}
