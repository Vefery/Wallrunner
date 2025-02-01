using UnityEngine;

public class PlayerModelController : MonoBehaviour
{
    public SkinInfo skinInfo;
    public Animator animator;
    private PlayerController playerController;
    private Rigidbody[] ragdoll;
    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
        if (playerController != null)
        {
            playerController.onJump += TriggerJumpAnimation;
            playerController.onLanded += ResumeRunningAnimation;
            playerController.onAliveStateChanged += ToggleRagdoll;
            playerController.onAnimationPause += PauseAnimation;
        }
        animator.keepAnimatorStateOnDisable = true;
        ragdoll = GetComponentsInChildren<Rigidbody>();
    }
    private void Start()
    {
        if (playerController == null)
            animator.SetBool("Idle", true);
    }
    private void TriggerJumpAnimation()
    {
        animator.SetTrigger("Jump");
    }
    private void ResumeRunningAnimation()
    {
        animator.SetTrigger("Landed");
    }
    private void PauseAnimation(bool isPaused)
    {
        animator.speed = isPaused ? 0f : 1f;
    }
    private void ToggleRagdoll()
    {
        animator.enabled = !animator.enabled;
        foreach (Rigidbody body in ragdoll)
        {
            body.isKinematic = !body.isKinematic;
            body.AddForce(playerController.GetComponent<Rigidbody>().linearVelocity * 100f, ForceMode.Force);
        }
    }
}