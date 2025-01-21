using UnityEngine;

public class Coin : MonoBehaviour
{
    public float rotationSpeed = 1f;
    void Awake()
    {
        transform.Rotate(Vector3.right, transform.position.z);
    }
    void Update()
    {
        transform.Rotate(Vector3.right, Time.deltaTime * rotationSpeed);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out PlayerController player))
        {
            player.AddCoin();
            Destroy(gameObject);
        }
    }
}
