using UnityEngine;

public class CodeButton : RayTappableObject
{
    [SerializeField] private LockCodePuzzle lockCodePuzzle;
    [SerializeField] private int myNumber;

    protected override void TriggerAction()
    {
        lockCodePuzzle.PressNumber(myNumber);
    }
}
