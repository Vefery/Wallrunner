using UnityEngine;

[CreateAssetMenu(menuName = "Game Shop/New Shop Item", fileName = "NewShopItem")]
public class ShopItem : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public int price;
}
