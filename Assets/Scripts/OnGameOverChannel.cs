using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "GameOver Channel")]
public class OnGameOverChannel : ScriptableObject
{
    public UnityEvent OnRestartGame;
    public UnityEvent OnGameOver;
    public void TriggerGameOver()
    {
        OnGameOver.Invoke();
    }
    public void TriggerRestartGame()
    {
        OnRestartGame.Invoke();
    }
}
