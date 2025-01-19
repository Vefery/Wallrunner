using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SkinSelector : MonoBehaviour
{
    public Transform initialCameraPoint;
    public Transform skinSelectCameraPoint;
    public GameObject skins;

    private Transform mainCamera;
    private Coroutine cameraTurnIEnumerator;
    private void Awake()
    {
        mainCamera = Camera.main.transform;
    }
    public void SkinSelection(bool activate)
    {
        if (cameraTurnIEnumerator != null)
            StopCoroutine(cameraTurnIEnumerator);

        cameraTurnIEnumerator = StartCoroutine(TurnCamera(activate ? skinSelectCameraPoint : initialCameraPoint));
    }
    private IEnumerator TurnCamera(Transform destination)
    {
        Quaternion startRot = mainCamera.transform.rotation;
        Vector3 startPos = mainCamera.transform.position;
        float progress = 0f;
        while (progress < 1f)
        {
            mainCamera.transform.rotation = Quaternion.Lerp(startRot, destination.rotation, progress);
            mainCamera.transform.position = Vector3.Lerp(startPos, destination.position, progress);
            progress += Time.deltaTime * 5f;
            yield return null;
        }
        cameraTurnIEnumerator = null;
    }
}
