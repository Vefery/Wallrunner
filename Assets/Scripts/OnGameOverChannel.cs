using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "GameOver Channel")]
public class OnGameOverChannel : ScriptableObject
{
    public UnityAction OnGameOver;
    public UnityAction OnRevertableGameOver;
    public void TriggerRevertableGameOver()
    {
        OnRevertableGameOver?.Invoke();
    }
    public void TriggerGameOver()
    {
        OnGameOver?.Invoke();
    }
}
