using Oculus.Interaction;
using UnityEngine;

public class SwappyThing : RayTappableObject
{
    [SerializeField] private Animator animator;

    protected override void TriggerAction()
    {
        animator.SetTrigger("Swap");
    }
}
