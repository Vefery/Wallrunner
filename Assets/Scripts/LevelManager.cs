using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LevelManager : MonoBehaviour
{
    public Transform playerStartingPosition;
    public float levelPartDistanceLimit;
    public float levelSpeed;
    public bool isLevelPaused = true;

    private float firstPartHalfLength;
    private List<LevelPart> activeLevelParts = new();
    private IList<GameObject> levelPartPrefabs;
    private GameObject RandomPartPrefab { get => levelPartPrefabs[Random.Range(0, levelPartPrefabs.Count)]; }

    private void Awake()
    {
        // RELEASE HANDLE ON GAMEOVER
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
        foreach (LevelPart part in activeLevelParts)
        {
            part.transform.Translate(levelSpeed * Time.deltaTime * -Vector3.forward, Space.Self);
        }
        if (activeLevelParts[0].transform.localPosition.z < -(firstPartHalfLength + levelPartDistanceLimit))
            UpdateLevelParts();
    }
    private void UpdateLevelParts()
    {
        LevelPart lastPart = activeLevelParts.Last();
        float lastHalfLength = lastPart.halfLength;
        Destroy(activeLevelParts[0].gameObject);
        activeLevelParts.RemoveAt(0);
        LevelPart newPiece = Instantiate(RandomPartPrefab, lastPart.transform.position, Quaternion.identity, transform).GetComponent<LevelPart>();
        newPiece.transform.Translate(Vector3.forward * (lastHalfLength + newPiece.halfLength), Space.Self);
        activeLevelParts.Add(newPiece);
        firstPartHalfLength = activeLevelParts[0].halfLength;
    }
    private void OnLoadBasePartsHandle_Completed(AsyncOperationHandle<IList<GameObject>> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            isLevelPaused = false;
            levelPartPrefabs = operation.Result;
        }
        else
            Debug.LogError("Failed to load base parts of the level!");
    }
}
