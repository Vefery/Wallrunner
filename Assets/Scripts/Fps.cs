using UnityEngine;
using System.Collections;

public class Fps : MonoBehaviour
{
    private float count;

    private IEnumerator Start()
    {
        GUI.depth = 2;
        while (true)
        {
            count = 1f / Time.unscaledDeltaTime;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnGUI()
    {
        var style = new GUIStyle();
        style.fontSize = 50;
        GUI.Label(new Rect(150, 40, 1000, 250), "FPS: " + Mathf.Round(count), style);
    }
}