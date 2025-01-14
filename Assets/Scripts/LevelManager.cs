using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Transform playerStartingPosition;
    public float levelPartDistanceLimit;
    public float levelSpeed;
    public List<GameObject> levelPartPrefabs;

    private float firstPartHalfLength;
    private List<LevelPart> activeLevelParts = new();
    private GameObject RandomPartPrefab { get => levelPartPrefabs[Random.Range(0, levelPartPrefabs.Count)]; }

    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
            activeLevelParts.Add(transform.GetChild(i).GetComponent<LevelPart>());
        firstPartHalfLength = activeLevelParts[0].halfLength;
    }
    void Update()
    {
        foreach (LevelPart part in activeLevelParts)
        {
            part.transform.Translate(levelSpeed * Time.deltaTime * -Vector3.forward, Space.Self);
        }
        if (activeLevelParts[0].transform.localPosition.z < -(firstPartHalfLength + levelPartDistanceLimit))
            UpdateLevelParts();
    }
    private void UpdateLevelParts()
    {
        LevelPart lastPart = activeLevelParts.Last();
        float lastHalfLength = lastPart.halfLength;
        Destroy(activeLevelParts[0].gameObject);
        activeLevelParts.RemoveAt(0);
        LevelPart newPiece = Instantiate(RandomPartPrefab, lastPart.transform.position, Quaternion.identity, transform).GetComponent<LevelPart>();
        newPiece.transform.Translate(Vector3.forward * (lastHalfLength + newPiece.halfLength), Space.Self);
        activeLevelParts.Add(newPiece);
        firstPartHalfLength = activeLevelParts[0].halfLength;
    }
}
