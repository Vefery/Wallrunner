using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void RestartLevel()
    {
        Addressables.LoadSceneAsync($"Assets/Scenes/{SceneManager.GetActiveScene().name}.unity", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    public void LoadLevel(string levelName)
    {
        Addressables.LoadSceneAsync($"Assets/Scenes/{levelName}.unity", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    public void ConfirmSettings()
    {
        Debug.LogWarning("Implement confirming settings!");
    }
    public void CancelSettings()
    {
        Debug.LogWarning("Implement canceling settings!");
    }
}
