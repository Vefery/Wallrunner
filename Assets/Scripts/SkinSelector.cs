using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SkinSelector : MonoBehaviour, IObjectWithData
{
    [Header("Camera")]
    public Transform initialCameraPoint;
    public Transform skinSelectCameraPoint;
    [Header("General")]
    public Transform skinsContainer;
    [Header("UI")]
    public TMP_Text coinsDisplay;

    private Transform mainCamera;
    private Coroutine cameraTurnIEnumerator;
    private List<GameObject> skins;
    private int index;
    private int maxIndex;
    private string primarySkinName;
    private int coins;
    private void Awake()
    {
        mainCamera = Camera.main.transform;

        AsyncOperationHandle<IList<GameObject>> loadBasePartsHandle = Addressables.LoadAssetsAsync<GameObject>("Skin");
        loadBasePartsHandle.Completed += OnLoadSkinsHandle_Completed;
    }
    public void SkinSelection(bool activate)
    {
        if (cameraTurnIEnumerator != null)
            StopCoroutine(cameraTurnIEnumerator);

        cameraTurnIEnumerator = StartCoroutine(TurnCamera(activate ? skinSelectCameraPoint : initialCameraPoint));
    }
    public void NextSkin()
    {
        if (index == maxIndex)
            return;

        skins[index].SetActive(false);
        index++;
        skins[index].transform.position = Vector3.zero;
        skins[index].SetActive(true);
    }
    public void PreviousSkin()
    {
        if (index == 0)
            return;

        skins[index].SetActive(false);
        index--;
        skins[index].transform.position = Vector3.zero;
        skins[index].SetActive(true);
    }
    public void Confirm()
    {
        primarySkinName = skins[index].GetComponent<PlayerModelController>().SkinName;
        SaveManager.Save();

        if (cameraTurnIEnumerator != null)
            StopCoroutine(cameraTurnIEnumerator);

        cameraTurnIEnumerator = StartCoroutine(TurnCamera(initialCameraPoint));
    }
    private IEnumerator TurnCamera(Transform destination)
    {
        mainCamera.transform.GetPositionAndRotation(out Vector3 startPos, out Quaternion startRot);
        float progress = 0f;
        while (progress < 1f)
        {
            mainCamera.transform.SetPositionAndRotation(Vector3.Lerp(startPos, destination.position, progress), Quaternion.Lerp(startRot, destination.rotation, progress));
            progress += Time.deltaTime * 3f;
            yield return null;
        }
        mainCamera.SetPositionAndRotation(destination.position, destination.rotation);
        cameraTurnIEnumerator = null;
    }
    private void OnLoadSkinsHandle_Completed(AsyncOperationHandle<IList<GameObject>> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            skins = new(operation.Result.Count);
            maxIndex = operation.Result.Count - 1;
            SpawnSkins(operation.Result, operation).Forget();
        }
        else
            Debug.LogError("Failed to load skins!");
    }
    private async UniTaskVoid SpawnSkins(IList<GameObject> skinPrefabs, AsyncOperationHandle<IList<GameObject>> skinsOperation)
    {
        foreach (GameObject skinPrefab in skinPrefabs)
        {
            GameObject skin = Instantiate(skinPrefab, skinsContainer);
            skin.SetActive(false);
            skins.Add(skin);
            if (skin.GetComponent<PlayerModelController>().SkinName == primarySkinName)
                index = skins.IndexOf(skin);
            await UniTask.Yield();
        }
        if (skinsOperation.IsValid())
            skinsOperation.Release();

        GameObject primarySkin = skins[index];
        primarySkin.transform.position = Vector3.zero;
        primarySkin.SetActive(true);
    }

    public void LoadData(GameData data)
    {
        primarySkinName = data.primarySkinName;
        coinsDisplay.SetText($"Coins: {data.Coins}");
        coins = data.Coins;
    }

    public void FetchData(GameData data)
    {
        data.primarySkinName = primarySkinName;
        data.Coins = coins;
    }
}
