using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LevelPart : MonoBehaviour
{
    [System.Serializable]
    public struct ObstacleLine
    {
        public Transform rightSocket;
        public Transform leftSocket;
    }

    public float halfLength;
    public float width;
    public float coinSpacing;
    public LayerMask obstacleCheckMask;
    public int lastTrajectoryElement = 1;
    public int consequentForward = 0;
    public ObstacleLine[] levelSockets;

    private IList<GameObject> obstaclesPrefabs;
    private GameObject coinPrefab;
    [SerializeField]
    private int[] trajectory; 
    private GameObject RandomObstaclePrefab { get => obstaclesPrefabs[Random.Range(0, obstaclesPrefabs.Count)]; }
    private async void Awake()
    {
        trajectory = new int[levelSockets.Length];
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
        for (int i = 0; i < trajectory.Length; i++)
        {
            if (trajectory[i] == -1)
            {
                Instantiate(coinPrefab, levelSockets[i].leftSocket).transform.Translate(Vector3.forward * coinSpacing, Space.World);
                Instantiate(coinPrefab, levelSockets[i].leftSocket).transform.Translate(Vector3.back * coinSpacing, Space.World);
            } 
            else
            {
                Instantiate(coinPrefab, levelSockets[i].rightSocket).transform.Translate(Vector3.forward * coinSpacing, Space.World);
                Instantiate(coinPrefab, levelSockets[i].rightSocket).transform.Translate(Vector3.back * coinSpacing, Space.World);
            }
            await UniTask.Yield();
        }
        coinPrefab = null;
        if (operation.IsValid())
            operation.Release();
    }
    private async UniTaskVoid SpawnObstacles(AsyncOperationHandle<IList<GameObject>> operation)
    {
        if (trajectory.Length == 0)
            return;

        trajectory[0] = lastTrajectoryElement;
        if (Random.Range(0f, 1f) > 0.5f * Mathf.Pow(0.8f, consequentForward))
        {
            trajectory[0] *= -1;
            consequentForward = 0;
        }
        for (int i = 1; i < trajectory.Length; i++)
        {
            trajectory[i] = trajectory[i - 1];
            consequentForward++;
            if ((trajectory[i] == 1 && levelSockets[i].rightSocket == null) || (trajectory[i] == -1 && levelSockets[i].leftSocket == null))
            {
                trajectory[i] *= -1;
                consequentForward = 0;
            }
            else if (Random.Range(0f, 1f) > 0.5f * Mathf.Pow(0.75f, consequentForward))
            {
                if ((trajectory[i] == -1 && levelSockets[i].rightSocket != null) || (trajectory[i - 1] == 1 && levelSockets[i].leftSocket != null))
                {
                    trajectory[i] *= -1;
                    consequentForward = 0;
                }
            }
        }
        lastTrajectoryElement = trajectory[trajectory.Length - 1];

        await UniTask.Yield();

        for (int i = 0; i < levelSockets.Length; i++)
        {
            if (trajectory[i] == 1 && levelSockets[i].leftSocket)
                Instantiate(RandomObstaclePrefab, levelSockets[i].leftSocket, instantiateInWorldSpace: false);
            else if (trajectory[i] == -1 && levelSockets[i].rightSocket)
                Instantiate(RandomObstaclePrefab, levelSockets[i].rightSocket, instantiateInWorldSpace: false);
            await UniTask.Yield();
        }
        obstaclesPrefabs = null;
        if (operation.IsValid())
            operation.Release();
    }
}