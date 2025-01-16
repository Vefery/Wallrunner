using UnityEngine;
using UnityEngine.AddressableAssets;

public class StartupLoader : MonoBehaviour
{
    void Start()
    {
        Addressables.LoadSceneAsync("Assets/Scenes/Menu.unity", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
