using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LevelPart : MonoBehaviour
{
    public float halfLength;
    public float width;
    public float coinSpacing;
    public LayerMask obstacleCheckMask;
    public Transform[] levelSockets;

    private IList<GameObject> obstaclesPrefabs;
    private GameObject coinPrefab;
    private bool[] freeSockets;
    private GameObject RandomObstaclePrefab { get => obstaclesPrefabs[Random.Range(0, obstaclesPrefabs.Count)]; }
    private async void Awake()
    {
        freeSockets = new bool[levelSockets.Length];
        System.Array.Fill(freeSockets, true);
        AsyncOperationHandle<IList<GameObject>> loadBasePartsHandle = Addressables.LoadAssetsAsync<GameObject>("Obstacles");
        await loadBasePartsHandle;
        OnLoadObstaclesHandle_Completed(loadBasePartsHandle);

        AsyncOperationHandle<GameObject> loadCoinPrefabHandle = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/CoinPrefab.prefab");
        loadCoinPrefabHandle.Completed += OnLoadCoinHandle_Completed;
    }
    private void OnLoadObstaclesHandle_Completed(AsyncOperationHandle<IList<GameObject>> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            obstaclesPrefabs = operation.Result;
            SpawnObstacles(operation).Forget();
        }
        else
            Debug.LogError("Failed to load obstacles!");
    }
    private void OnLoadCoinHandle_Completed(AsyncOperationHandle<GameObject> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            coinPrefab = operation.Result;
            SpawnCoins(operation).Forget();
        }
        else
            Debug.LogError("Failed to load coin object!");
    }
    private async UniTaskVoid SpawnCoins(AsyncOperationHandle<GameObject> operation)
    {
        for (int i = 0; i < freeSockets.Length; i++)
        {
            if (freeSockets[i])
            {
                Instantiate(coinPrefab, levelSockets[i]);
                Instantiate(coinPrefab, levelSockets[i]).transform.Translate(Vector3.forward * coinSpacing, Space.World);
                Instantiate(coinPrefab, levelSockets[i]).transform.Translate(Vector3.back * coinSpacing, Space.World);
            }
            await UniTask.Yield();
        }
        coinPrefab = null;
        if (operation.IsValid())
            operation.Release();
    }
    private async UniTaskVoid SpawnObstacles(AsyncOperationHandle<IList<GameObject>> operation)
    {
        for (int i = 0; i < levelSockets.Length; i++)
        {
            if (Random.Range(0f, 1f) > 0.5f && !Physics.Raycast(levelSockets[i].position + levelSockets[i].forward * width - levelSockets[i].right * 7, levelSockets[i].right, 14, obstacleCheckMask))
            {
                freeSockets[i] = false;
                Instantiate(RandomObstaclePrefab, levelSockets[i], instantiateInWorldSpace: false);
            }
            await UniTask.Yield();
        }
        obstaclesPrefabs = null;
        if (operation.IsValid())
            operation.Release();
    }
}