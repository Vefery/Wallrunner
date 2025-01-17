using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerController : MonoBehaviour
{
    public float jumpSpeed;
    public float swipeDeltaThreshold;
    public LayerMask raycastLayerMask;

    private Vector2 swipeDelta = Vector2.zero;
    private CapsuleCollider playerCollider;
    private TouchInput controls;
    private AsyncOperationHandle<OnGameOverChannel> gameOverChannelOperation;
    private OnGameOverChannel gameOverChannel;
    private bool onRightWall = true;
    private bool isMidair = false;
    private bool isControlLocked = false;

    private void Awake()
    {
        playerCollider = GetComponent<CapsuleCollider>();
        var gameOverChannelHandle = Addressables.LoadAssetAsync<OnGameOverChannel>("Assets/EventChannels/GameOver Channel.asset");
        gameOverChannelHandle.Completed += OnLoadGameOverChannel_Completed;
    }
    private void OnLoadGameOverChannel_Completed(AsyncOperationHandle<OnGameOverChannel> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            gameOverChannel = operation.Result;
        }
        else
            Debug.LogError("Failed to load base parts of the level!");
        gameOverChannelOperation = operation;
    }
    void Start()
    {
        controls = new();
        controls.Enable();
        controls.Player.Touch.canceled += OnTouchEnd;
        controls.Player.Swipe.performed += OnTouchMove;
    }
    private void OnTouchEnd(InputAction.CallbackContext context)
    {
        if (isMidair || isControlLocked)
            return;

        RaycastHit hit;
        if (swipeDelta.x > swipeDeltaThreshold)
        {
            if (!onRightWall)
            {
                if (Physics.Raycast(transform.position, Vector3.right, out hit, 15, raycastLayerMask))
                {
                    onRightWall = true;
                    StartCoroutine(JumpCoroutine(hit.point - Vector3.right * playerCollider.radius));
                }
            }
        }
        else if (swipeDelta.x < -swipeDeltaThreshold)
        {
            if (onRightWall)
            {
                if (Physics.Raycast(transform.position, -Vector3.right, out hit, 15, raycastLayerMask))
                {
                    onRightWall = false;
                    StartCoroutine(JumpCoroutine(hit.point + Vector3.right * playerCollider.radius));
                }
            }
        }
    }
    public void TriggerRevertableGameOver()
    {
        gameOverChannel.TriggerGameOver();
        controls.Disable();
        isControlLocked = true;
    }
    private void OnTouchMove(InputAction.CallbackContext context)
    {
        swipeDelta = context.ReadValue<Vector2>();
    }
    private IEnumerator JumpCoroutine(Vector3 destination)
    {
        Vector3 startPoint = transform.position;
        float progress = 0f;
        isMidair = true;
        while (progress < 1f)
        {
            progress += jumpSpeed * Time.deltaTime;
            Vector3 temp = Vector3.Lerp(startPoint, destination, progress);
            temp.y = -0.05f * (transform.position.x + startPoint.x) * (transform.position.x + destination.x);
            transform.position = temp;
            yield return null;
        }
        transform.position = destination;
        isMidair = false;
    }
    private void OnDestroy()
    {
        if (gameOverChannelOperation.IsValid())
            gameOverChannelOperation.Release();
    }
}
