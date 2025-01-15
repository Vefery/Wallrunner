using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class PlayerController : MonoBehaviour
{
    public float swipeDeltaThreshold;

    InputAction jumpAction;
    Vector2 swipeDelta = Vector2.zero;
    CapsuleCollider playerCollider;
    bool onRightWall = true;
    bool isMidair = false;

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
        if (isMidair)
            return;

        RaycastHit hit;
        if (swipeDelta.x > swipeDeltaThreshold)
        {
            if (!onRightWall)
            {
                if (Physics.Raycast(transform.position, Vector3.right, out hit, 15))
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
                if (Physics.Raycast(transform.position, -Vector3.right, out hit, 15))
                {
                    onRightWall = false;
                    StartCoroutine(JumpCoroutine(hit.point + Vector3.right * playerCollider.radius));
                }
            }
        }
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
