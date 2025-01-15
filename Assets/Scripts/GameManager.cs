using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameManager : MonoBehaviour
{
    public void RestartLevel()
    {
        Addressables.LoadSceneAsync("Assets/Scenes/Game.unity", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
