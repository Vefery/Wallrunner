using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public enum Menulabel
{
    Ingame,
    MainMenu
}
public class MenuManager : MonoBehaviour
{
    public UnityEvent onLevelRestart;

    [SerializeField]
    private Menulabel primarylabel;
    private AsyncOperationHandle<SceneInstance> sceneHandle;
    [SerializeField]
    private MenuItem[] menus;

    private void Awake()
    {
        menus = FindObjectsByType<MenuItem>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList().Where(x => x.Label == primarylabel).ToArray();
    }
    public void OpenMenu(string name)
    {
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
        if (menu.Label != primarylabel)
        {
            Debug.LogWarning($"Menu's label {menu.Label} and primary label {primarylabel} don't match");
            return;
        }
        foreach (MenuItem menuItem in menus)
            menuItem.Close();

        menu.Open();
    }
    public void CloseAll()
    {
        foreach (MenuItem menu in menus)
            menu.Close();
    }
}
