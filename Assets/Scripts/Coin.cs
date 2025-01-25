using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEngine.Rendering.HDROutputUtils;

public class Coin : MonoBehaviour
{
    public float rotationSpeed = 1f;
    private AudioClip collectSound;
    private AsyncOperationHandle<AudioClip> collectSoundOperation;
    private AudioSource soundsSource;

    void Awake()
    {
        transform.Rotate(Vector3.right, transform.position.z);
        AsyncOperationHandle<AudioClip> collectSoundHandle = Addressables.LoadAssetAsync<AudioClip>("Assets/Sounds/Collect.wav");
        collectSoundHandle.Completed += OnCollectSoundHandle_Completed;
        soundsSource = GameObject.FindGameObjectWithTag("SoundSource").GetComponent<AudioSource>();
    }
    private void OnCollectSoundHandle_Completed(AsyncOperationHandle<AudioClip> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            collectSound = operation.Result;
        }
        else
            Debug.LogError("Failed to load collect sound!");
        collectSoundOperation = operation;
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
            soundsSource.clip = collectSound;
            soundsSource.Play();
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        if (collectSoundOperation.IsValid())
            collectSoundOperation.Release();
    }
}
