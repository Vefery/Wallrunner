using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Channels/GameOver Channel", fileName = "OnGameOver Channel")]
public class IngameChannel : ScriptableObject
{
    public UnityEvent OnRestartGame;
    public UnityEvent OnGameOver;
    public UnityEvent<int> OnResurrect;
    public UnityEvent<bool> OnPause;
    public UnityEvent<int> OnCollectedCoin;
    public void TriggerPause(bool isPaused)
    {
        OnPause.Invoke(isPaused);
    }
    public void TriggerCollectedCoin(int amount)
    {
        OnCollectedCoin.Invoke(amount);
    }
    public void TriggerGameOver()
    {
        OnGameOver.Invoke();
    }
    public void TriggerRestartGame()
    {
        OnRestartGame.Invoke();
    }
    public void TriggerResurrect(int keysLeft)
    {
        OnResurrect.Invoke(keysLeft);
    }
}
