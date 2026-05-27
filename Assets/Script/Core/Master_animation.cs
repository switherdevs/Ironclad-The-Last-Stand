using UnityEngine;

public abstract class BaseUnitAnimation : MonoBehaviour
{
    protected Animator animator;
    private string currentAnimationState;

    protected string animIdle = "Unit_Idle";
    protected string animWalk = "Unit_Walk";
    protected string animAttack = "Unit_Attack";
    protected string animHit = "Unit_TakeDamage";
    protected string animDie = "Unit_Die";

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ChangeAnimationState(string newState, float transitionDuration = 0.1f)
    {
        if (currentAnimationState == newState) return;
        if (currentAnimationState == animDie) return; // Đã chết thì không chuyển state khác

        animator.CrossFade(newState, transitionDuration);
        currentAnimationState = newState;
    }

    public void PlayIdle() => ChangeAnimationState(animIdle);
    public void PlayWalk() => ChangeAnimationState(animWalk);
    public void PlayAttack(float speed = 0.05f) => ChangeAnimationState(animAttack, speed);
    public void PlayTakeDamage() => ChangeAnimationState(animHit, 0.05f);
    public void PlayDie() => ChangeAnimationState(animDie, 0.1f);
}