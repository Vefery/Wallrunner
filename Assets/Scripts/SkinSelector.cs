using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SkinSelector : MonoBehaviour, IDataLoader, IDataFetcher
{
    private struct SelectorSkinData
    {
        public GameObject skinObject;
        public bool isUnlocked;
        public SkinInfo info;
        public SelectorSkinData(GameObject skinObject)
        {
            this.skinObject = skinObject;
            isUnlocked = false;
            info = skinObject.GetComponent<PlayerModelController>().skinInfo;
        }
    }

    [Header("Camera")]
    public Transform initialCameraPoint;
    public Transform skinSelectCameraPoint;
    [Header("General")]
    public Transform skinsContainer;
    public AudioSource audioSource;
    [Header("UI")]
    public TMP_Text coinsDisplay;
    public GameObject confirmButton;
    public GameObject buyButton;

    private Transform mainCamera;
    private Coroutine cameraTurnIEnumerator;
    private GameManager gameManager;
    private TMP_Text priceText;
    private List<SelectorSkinData> skins;
    private AudioClip clickSound;
    private AudioClip purchaseSound;
    private int index;
    private int maxIndex;
    private string primarySkinName;
    private List<string> unlockedSkins;
    private AsyncOperationHandle<AudioClip> clickSoundOperation;
    private AsyncOperationHandle<AudioClip> purchaseSoundOperation;

    private void Awake()
    {
        mainCamera = Camera.main.transform;
        gameManager = FindFirstObjectByType<GameManager>();
        priceText = buyButton.GetComponentInChildren<TMP_Text>();

        AsyncOperationHandle<IList<GameObject>> loadBasePartsHandle = Addressables.LoadAssetsAsync<GameObject>("Skin");
        loadBasePartsHandle.Completed += OnLoadSkinsHandle_Completed;
        AsyncOperationHandle<AudioClip> clickSoundHandle = Addressables.LoadAssetAsync<AudioClip>("Assets/Sounds/Click.wav");
        clickSoundHandle.Completed += OnClickSoundHandle_Completed;
        AsyncOperationHandle<AudioClip> purchaseSoundHandle = Addressables.LoadAssetAsync<AudioClip>("Assets/Sounds/Purchase.wav");
        purchaseSoundHandle.Completed += OnPurchaseSoundHandle_Completed;
    }
    public void SkinSelection(bool activate)
    {
        if (cameraTurnIEnumerator != null)
            StopCoroutine(cameraTurnIEnumerator);

        cameraTurnIEnumerator = StartCoroutine(TurnCamera(activate ? skinSelectCameraPoint : initialCameraPoint));
    }
    public void NextSkin()
    {
        audioSource.clip = clickSound;
        audioSource.Play();
        if (index == maxIndex)
            return;

        skins[index].skinObject.SetActive(false);
        index++;
        skins[index].skinObject.transform.position = Vector3.zero;
        skins[index].skinObject.SetActive(true);

        UpdateMainButton();
    }
    public void PreviousSkin()
    {
        audioSource.clip = clickSound;
        audioSource.Play();
        if (index == 0)
            return;

        skins[index].skinObject.SetActive(false);
        index--;
        skins[index].skinObject.transform.position = Vector3.zero;
        skins[index].skinObject.SetActive(true);

        UpdateMainButton();
    }
    private void UpdateMainButton()
    {
        if (skins[index].isUnlocked)
        {
            confirmButton.SetActive(true);
            buyButton.SetActive(false);
        }
        else
        {
            buyButton.SetActive(true);
            confirmButton.SetActive(false);
            priceText.SetText($"{skins[index].info.price}$");
        }
    }
    public void Confirm()
    {
        audioSource.clip = clickSound;
        audioSource.Play();
        primarySkinName = skins[index].info.skinName;
        SaveManager.Save();

        if (cameraTurnIEnumerator != null)
            StopCoroutine(cameraTurnIEnumerator);

        cameraTurnIEnumerator = StartCoroutine(TurnCamera(initialCameraPoint));
    }
    public void BuySkin()
    {
        audioSource.clip = purchaseSound;
        audioSource.Play();
        SelectorSkinData skinData = skins[index];
        if (gameManager.Coins >= skinData.info.price)
        {
            gameManager.Coins -= skinData.info.price;
            coinsDisplay.SetText($"Coins: {gameManager.Coins}");
            skinData.isUnlocked = true;

            skins[index] = skinData;
            unlockedSkins.Add(skins[index].info.skinName);

            confirmButton.SetActive(true);
            buyButton.SetActive(false);
            SaveManager.Save();
        }
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
    private void OnClickSoundHandle_Completed(AsyncOperationHandle<AudioClip> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            clickSound = operation.Result;
        }
        else
            Debug.LogError("Failed to load UI sound!");
        clickSoundOperation = operation;
    }
    private void OnPurchaseSoundHandle_Completed(AsyncOperationHandle<AudioClip> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            purchaseSound = operation.Result;
        }
        else
            Debug.LogError("Failed to load UI sound!");
        purchaseSoundOperation = operation;
    }
    private async UniTaskVoid SpawnSkins(IList<GameObject> skinPrefabs, AsyncOperationHandle<IList<GameObject>> skinsOperation)
    {
        for (int i = 0; i < skinPrefabs.Count; i++)
        {
            GameObject skinObject = Instantiate(skinPrefabs[i], skinsContainer);
            skinObject.SetActive(false);
            SelectorSkinData skinData = new(skinObject);
            if (skinData.info.skinName == primarySkinName)
                index = i;
            if (unlockedSkins.Contains(skinData.info.skinName))
                skinData.isUnlocked = true;

            skins.Add(skinData);
            await UniTask.Yield();
        }
        if (skinsOperation.IsValid())
            skinsOperation.Release();

        GameObject primarySkin = skins[index].skinObject;
        primarySkin.transform.position = Vector3.zero;
        primarySkin.SetActive(true);
        confirmButton.SetActive(true);
        buyButton.SetActive(false);
    }

    public void LoadData(GameData data)
    {
        primarySkinName = data.primarySkinName;
        unlockedSkins = data.unlockedSkins;
        coinsDisplay.SetText($"Coins: {data.coins}");
    }

    public void FetchData(GameData data)
    {
        data.primarySkinName = primarySkinName;
        data.unlockedSkins = unlockedSkins;
    }
    private void OnDestroy()
    {
        if (clickSoundOperation.IsValid())
            clickSoundOperation.Release();
        if (purchaseSoundOperation.IsValid())
            purchaseSoundOperation.Release();
    }
}
