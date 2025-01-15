using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public UnityEvent onGameOver;
    public float swipeDeltaThreshold;
    public LayerMask raycastLayerMask;

    private Vector2 swipeDelta = Vector2.zero;
    private CapsuleCollider playerCollider;
    private bool onRightWall = true;
    private bool isMidair = false;
    private bool isControlLocked = false;

    private void Awake()
    {
        playerCollider = GetComponent<CapsuleCollider>();
    }
    void Start()
    {
        TouchInput controls = new();
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
    public void TriggerGameOver()
    {
        onGameOver.Invoke();
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
            progress += 3.5f * Time.deltaTime;
            Vector3 temp = Vector3.Lerp(startPoint, destination, progress);
            temp.y = -0.05f * (transform.position.x + startPoint.x) * (transform.position.x + destination.x);
            transform.position = temp;
            yield return null;
        }
        transform.position = destination;
        isMidair = false;
    }
}
