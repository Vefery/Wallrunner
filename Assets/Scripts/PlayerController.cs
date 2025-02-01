using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerController : MonoBehaviour, IDataLoader
{
    public float jumpSpeed;
    public float swipeDeltaThreshold;
    public LayerMask raycastLayerMask;
    public Action onJump;
    public Action onLanded;
    public Action onAliveStateChanged;
    public Action<bool> onAnimationPause;

    private PlayerModelController playerModelController;
    private Vector2 swipeDelta = Vector2.zero;
    private CapsuleCollider playerCollider;
    private TouchInput controls;
    private AsyncOperationHandle<IngameChannel> ingameChannelHandle;
    private AsyncOperationHandle<GameObject>  playerModelHandle;
    private IngameChannel ingameChannel;
    private Rigidbody rb;
    private bool onRightWall = true;
    private bool isMidair = false;
    private bool isDead = false;
    private bool isPaused = false;
    private float playerYlevel;

    private void Awake()
    {
        playerCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        ingameChannelHandle = Addressables.LoadAssetAsync<IngameChannel>("Assets/EventChannels/Ingame Channel.asset");
        ingameChannelHandle.Completed += OnLoadGameOverChannel_Completed;
        playerYlevel = transform.position.y;
    }
    private void OnLoadGameOverChannel_Completed(AsyncOperationHandle<IngameChannel> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            ingameChannel = operation.Result;
            ingameChannel.OnResurrect.AddListener(OnRessurect);
            ingameChannel.OnPause.AddListener(OnPause);
        }
        else
            Debug.LogError("Failed to load base parts of the level!");
    }
    void Start()
    {
        controls = new();
        controls.Enable();
        controls.Player.Touch.canceled += OnTouchEnd;
        controls.Player.Swipe.performed += OnTouchMove;
        controls.UI.Back.performed += OnBackButton;
    }
    private void OnTouchEnd(InputAction.CallbackContext context)
    {
        if (isMidair || isDead)
            return;

        RaycastHit hit;
        if (swipeDelta.x > swipeDeltaThreshold)
        {
            if (!onRightWall)
            {
                if (Physics.Raycast(transform.position, transform.right, out hit, 15, raycastLayerMask))
                {
                    onRightWall = true;
                    StartCoroutine(JumpCoroutine(hit.point - transform.right * playerCollider.radius));
                }
                else
                {
                    onRightWall = true;
                    StartCoroutine(JumpCoroutine(transform.position + transform.right * (14 - 2 * playerCollider.radius)));
                }
            }
        }
        else if (swipeDelta.x < -swipeDeltaThreshold)
        {
            if (onRightWall)
            {
                if (Physics.Raycast(transform.position, -transform.right, out hit, 15, raycastLayerMask))
                {
                    onRightWall = false;
                    StartCoroutine(JumpCoroutine(hit.point + transform.right * playerCollider.radius));
                }
                else
                {
                    onRightWall = false;
                    StartCoroutine(JumpCoroutine(transform.position + transform.right * (-14 + 2 * playerCollider.radius)));
                }
            }
        }
    }
    public void TriggerRevertableGameOver()
    {
        ingameChannel.TriggerGameOver();
        controls.Player.Disable();
        isDead = true;
        onAliveStateChanged?.Invoke();
    }
    public void AddCoin()
    {
        ingameChannel.TriggerCollectedCoin(1);
    }
    private void OnBackButton(InputAction.CallbackContext context)
    {
        if (isDead)
            return;

        ingameChannel.TriggerPause(!isPaused);
    }
    private void OnPause(bool isPaused)
    {
        if (isPaused)
        {
            controls.Player.Disable();
        }
        else
        {
            controls.Player.Enable();
        }
        this.isPaused = isPaused;
        onAnimationPause?.Invoke(isPaused);
    }
    private void OnRessurect(int keysLeft)
    {
        controls.Player.Enable();
        isDead = false;
        onAliveStateChanged?.Invoke();
    }
    private void OnTouchMove(InputAction.CallbackContext context)
    {
        swipeDelta = context.ReadValue<Vector2>();
    }
    private IEnumerator JumpCoroutine(Vector3 destination)
    {
        onJump?.Invoke();
        Vector3 startPoint = transform.position;
        float progress = 0f;
        isMidair = true;
        while (progress < 1f)
        {
            progress += jumpSpeed * Time.deltaTime;
            Vector3 temp = Vector3.Lerp(startPoint, destination, progress);
            temp.y = -0.05f * (transform.position.x + startPoint.x) * (transform.position.x + destination.x) + playerYlevel;
            rb.MovePosition(temp);
            yield return null;
        }
        rb.MovePosition(destination);
        isMidair = false;
        onLanded?.Invoke();
    }
    private void OnDestroy()
    {
        if (ingameChannelHandle.IsValid())
            ingameChannelHandle.Release();
        if (playerModelHandle.IsValid())
            playerModelHandle.Release();
    }

    public void LoadData(GameData data)
    {
        if (playerModelController != null)
            return;

        playerModelHandle = Addressables.LoadAssetAsync<GameObject>($"Assets/Prefabs/Skins/{data.primarySkinName}.prefab");
        playerModelHandle.Completed += OnLoadPlayerModel_Completed;
    }
    private void OnLoadPlayerModel_Completed(AsyncOperationHandle<GameObject> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            playerModelController = Instantiate(operation.Result, transform.position, Quaternion.identity, transform).GetComponent<PlayerModelController>();
        }
        else
        {
            Debug.LogError("Failed to load player model! Fallback to default");
            var playerModelHandle = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Skins/PurpleRobot.prefab");
            playerModelHandle.Completed += OnLoadPlayerModel_Completed;
        }
    }
    
}
