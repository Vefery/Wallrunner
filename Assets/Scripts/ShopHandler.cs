using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ShopHandler : MonoBehaviour, IDataFetcher, IDataLoader
{
    private struct ShopItemData
    {
        public int quantity;
        public ShopItem info;
        public ShopItemData(ShopItem shopItem)
        {
            info = shopItem;
            quantity = 0;
        }
    }

    [Header("General")]
    public AudioSource audioSource;
    [Header("UI")]
    public Image itemIcon;
    public TMP_Text coinsDisplay;
    public TMP_Text quantityDisplay;
    public GameObject buyButton;

    private GameManager gameManager;
    private TMP_Text priceText;
    private ShopItem[] itemsInfo;
    private AudioClip clickSound;
    private AudioClip purchaseSound;
    private List<ShopItemData> shopItems;
    private List<GameData.OwnedItemPair> ownedItems;
    private int index;
    private int maxIndex;
    private AsyncOperationHandle<AudioClip> clickSoundHandle;
    private AsyncOperationHandle<AudioClip> purchaseSoundHandle;
    private AsyncOperationHandle<IList<ShopItem>> itemsHandle;

    private void Awake()
    {
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
            itemsInfo = operation.Result.ToArray();
            index = 0;
            maxIndex = itemsInfo.Length - 1;
            MakeItemsList().Forget();
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
    private async UniTaskVoid MakeItemsList()
    {
        shopItems = new(itemsInfo.Length);
        for (int i = 0; i < itemsInfo.Length; i++)
        {
            ShopItemData data;
            data.info = itemsInfo[i];
            data.quantity = 0;
            foreach (var item in ownedItems)
            {
                if (item.name == data.info.itemName)
                {
                    data.quantity = item.owned;
                    break;
                }
            }
            shopItems.Add(data);
            await UniTask.Yield();
        }
    }
    public void NextItem()
    {
        audioSource.clip = clickSound;
        audioSource.Play();
        if (index == maxIndex)
            return;

        index++;
        UpdateUI();
    }
    public void PreviousItem()
    {
        audioSource.clip = clickSound;
        audioSource.Play();
        if (index == 0)
            return;

        index--;
        UpdateUI();
    }
    public void UpdateUI()
    {
        itemIcon.sprite = shopItems[index].info.itemIcon;
        priceText.SetText($"{shopItems[index].info.price}$");
        quantityDisplay.SetText($"Owned: {shopItems[index].quantity}");
        coinsDisplay.SetText($"Coins: {gameManager.Coins}");
    }
    public void BuyItem()
    {
        audioSource.clip = purchaseSound;
        audioSource.Play();
        ShopItemData shopItem = shopItems[index];
        if (gameManager.Coins >= shopItem.info.price)
        {
            gameManager.Coins -= shopItem.info.price;
            coinsDisplay.SetText($"Coins: {gameManager.Coins}");
            shopItem.quantity++;
            GameData.OwnedItemPair itemPair;
            itemPair.name = shopItem.info.itemName;
            itemPair.owned = shopItem.quantity;
            if (itemPair.owned == 1)
            {
                ownedItems.Add(itemPair);
            } 
            else
            {
                for (int i = 0; i < ownedItems.Count; i++)
                {
                    if (ownedItems[i].name == itemPair.name)
                    {
                        ownedItems[i] = itemPair;
                        break;
                    }
                }
            }
            shopItems[index] = shopItem;
            UpdateUI();
            SaveManager.Save();
        }
    }
    public void FetchData(GameData data)
    {
        data.ownedItems = ownedItems;
    }

    public void LoadData(GameData data)
    {
        ownedItems = data.ownedItems;
        coinsDisplay.SetText($"Coins: {data.coins}");
    }
    private void OnDestroy()
    {
        if (clickSoundHandle.IsValid())
            clickSoundHandle.Release();
        if (purchaseSoundHandle.IsValid())
            purchaseSoundHandle.Release();
    }
}
