using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEngine.Rendering.HDROutputUtils;

public enum Menulabel
{
    Ingame,
    MainMenu
}
public class MenuManager : MonoBehaviour
{
    public AudioSource audioSource;
    [SerializeField]
    private Menulabel primaryLabel;
    private MenuItem[] menus;
    private AudioClip clickSound;
    private AsyncOperationHandle<AudioClip> clickSoundOperation;

    private void Awake()
    {
        menus = FindObjectsByType<MenuItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList().Where(x => x.Label == primaryLabel).ToArray();

        AsyncOperationHandle<AudioClip> clickSoundHandle = Addressables.LoadAssetAsync<AudioClip>("Assets/Sounds/Click.wav");
        clickSoundHandle.Completed += OnClickSoundHandle_Completed;
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

    public void OpenMenu(string name)
    {
        audioSource.clip = clickSound;
        audioSource.Play();

        bool flag = false;
        foreach (MenuItem menu in menus)
        {
            if (menu.menuName == name)
            {
                flag = true;
                menu.Open();
            }
            else
                menu.Close();
        }
        if (!flag)
            Debug.LogError($"Couldn't open {name}");
    }
    public void OpenMenu(MenuItem menu)
    {
        audioSource.clip = clickSound;
        audioSource.Play();

        if (menu.Label != primaryLabel)
        {
            Debug.LogWarning($"Menu's label {menu.Label} and primary label {primaryLabel} don't match");
            return;
        }
        foreach (MenuItem menuItem in menus)
            menuItem.Close();

        menu.Open();
    }
    public void CloseAll()
    {
        audioSource.clip = clickSound;
        audioSource.Play();

        foreach (MenuItem menu in menus)
            menu.Close();
    }
    private void OnDestroy()
    {
        if (clickSoundOperation.IsValid())
            clickSoundOperation.Release();
    }
}
