using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LevelPart : MonoBehaviour
{
    public float halfLength;
    public float width;
    public LayerMask obstacleCheckMask;
    public Transform[] obstacleSockets;

    private IList<GameObject> obstaclesPrefabs;
    private GameObject RandomObstaclePrefab { get => obstaclesPrefabs[Random.Range(0, obstaclesPrefabs.Count)]; }
    private void Awake()
    {
        AsyncOperationHandle<IList<GameObject>> loadBasePartsHandle = Addressables.LoadAssetsAsync<GameObject>("Obstacles");
        loadBasePartsHandle.Completed += OnLoadObstaclesHandle_Completed;
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
    private async UniTaskVoid SpawnObstacles(AsyncOperationHandle<IList<GameObject>> operation)
    {
        foreach (Transform socket in obstacleSockets)
        {
            if (Random.Range(0f, 1f) > 0.5f && !Physics.Raycast(socket.position + socket.forward * width - socket.right * 7, socket.right, 14, obstacleCheckMask))
            {
                Instantiate(RandomObstaclePrefab, socket, instantiateInWorldSpace: false);
            }
            await UniTask.Yield();
        }
        obstaclesPrefabs = null;
        operation.Release();
    }
}