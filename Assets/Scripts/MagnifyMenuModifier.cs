using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Menu Modifiers/Magnify")]
public class MagnifyMenuModifier : ScriptableObject, IMenuAppearModifier
{
    public AnimationClip clip;
    private Animation animation;
    public void RunModifier()
    {
        animation.clip = clip;
        animation.Play();
    }

    public void Setup(GameObject menuPanel)
    {
        animation = menuPanel.AddComponent<Animation>();
        animation.clip = clip;
    }
}
