using UnityEngine;

public class MenuItem : MonoBehaviour
{
    public string menuName;
    public Menulabel Label { get => _label; }
    public bool isOpened;
    public ScriptableObject appearModifier;

    [SerializeField]
    private Menulabel _label;
    private IMenuAppearModifier _menuAppearModifier;
    private void Awake()
    {
        if (!isOpened)
            gameObject.SetActive(false);

        if (appearModifier != null)
        {
            if (appearModifier is IMenuAppearModifier)
            {
                _menuAppearModifier = appearModifier as IMenuAppearModifier;
                _menuAppearModifier.Setup(gameObject);
            }
            else
            {
                Debug.LogError($"{appearModifier.name} is not a menu modifier!");
            }
        }
    }
    public void Open()
    {
        if (isOpened)
            return;
        isOpened = true;
        gameObject.SetActive(true);
        if (_menuAppearModifier != null)
            _menuAppearModifier.RunModifier();
    }
    public void Close()
    {
        if (!isOpened)
            return;
        isOpened = false;
        gameObject.SetActive(false);
    }
}
