using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using static UnityEngine.InputManagerEntry;

public class ShopHandler : MonoBehaviour
{
    [Header("General")]
    public Image itemIcon;
    public AudioSource audioSource;
    [Header("UI")]
    public TMP_Text coinsDisplay;
    public GameObject buyButton;

    private GameManager gameManager;
    private TMP_Text priceText;
    private ShopItem[] items;
    private AudioClip clickSound;
    private AudioClip purchaseSound;
    private int index;
    private int maxIndex;
    private AsyncOperationHandle<AudioClip> clickSoundHandle;
    private AsyncOperationHandle<AudioClip> purchaseSoundHandle;
    private AsyncOperationHandle<IList<ShopItem>> itemsHandle;

    private void Awake()
    {;
        gameManager = FindFirstObjectByType<GameManager>();
        priceText = buyButton.GetComponentInChildren<TMP_Text>();

        itemsHandle = Addressables.LoadAssetsAsync<ShopItem>("ShopItem");
        itemsHandle.Completed += OnLoadItemsHandle_Completed;

        clickSoundHandle = Addressables.LoadAssetAsync<AudioClip>("Assets/Sounds/Click.wav");
        clickSoundHandle.Completed += OnClickSoundHandle_Completed;

        purchaseSoundHandle = Addressables.LoadAssetAsync<AudioClip>("Assets/Sounds/Purchase.wav");
        purchaseSoundHandle.Completed += OnPurchaseSoundHandle_Completed;
    }
    private void OnLoadItemsHandle_Completed(AsyncOperationHandle<IList<ShopItem>> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            items = operation.Result.ToArray();
            itemIcon.sprite = items[0].itemIcon;
            priceText.SetText($"{items[0].price}$");
            index = 0;
        }
        else
            Debug.LogError("Failed to load shop Items!");
    }
    private void OnClickSoundHandle_Completed(AsyncOperationHandle<AudioClip> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            clickSound = operation.Result;
        }
        else
            Debug.LogError("Failed to load UI sound!");
    }
    private void OnPurchaseSoundHandle_Completed(AsyncOperationHandle<AudioClip> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            purchaseSound = operation.Result;
        }
        else
            Debug.LogError("Failed to load UI sound!");
    }
    private void OnDestroy()
    {
        if (clickSoundHandle.IsValid())
            clickSoundHandle.Release();
        if (purchaseSoundHandle.IsValid())
            purchaseSoundHandle.Release();
    }
}
