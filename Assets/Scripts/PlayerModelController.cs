using UnityEngine;

public class PlayerModelController : MonoBehaviour
{
    public SkinInfo skinInfo;
    public Animator animator;
    private PlayerController playerController;
    private Rigidbody[] bodies;
    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
        playerController.onJump += TriggerJumpAnimation;
        playerController.onLanded += ResumeRunningAnimation;
        playerController.onAliveStateChanged += ToggleRagdoll;
        playerController.onAnimationPause += PauseAnimation;

        bodies = GetComponentsInChildren<Rigidbody>();
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
    }
}
