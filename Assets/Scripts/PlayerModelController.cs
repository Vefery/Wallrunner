using UnityEngine;

public class PlayerModelController : MonoBehaviour
{
    [SerializeField]
    private string _skinName;
    public string SkinName { get { return _skinName; } }
}
