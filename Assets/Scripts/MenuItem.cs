using UnityEngine;

public class MenuItem : MonoBehaviour
{
    public string menuName;
    public Menulabel Label { get => _label; }
    public bool isOpened;

    [SerializeField]
    private Menulabel _label;
    private void Awake()
    {
        if (!isOpened)
            gameObject.SetActive(false);
    }
    public void Open()
    {
        if (isOpened)
            return;
        isOpened = true;
        gameObject.SetActive(true);
    }
    public void Close()
    {
        if (!isOpened)
            return;
        isOpened = false;
        gameObject.SetActive(false);
    }
}
