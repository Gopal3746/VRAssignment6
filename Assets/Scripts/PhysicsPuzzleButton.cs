using UnityEngine;

public class PhysicsPuzzleButton : RayTappableObject
{
    [SerializeField] private PhysicsPuzzleBall ballPrefab;
    [SerializeField] private Vector3 ballPosition;
    [SerializeField] private PhysicsPuzzle physicsPuzzle;

    protected override void TriggerAction()
    {
        //Debug.Log("summoned ball");
        PhysicsPuzzleBall created = Instantiate(ballPrefab, ballPosition + physicsPuzzle.transform.position, Quaternion.identity, physicsPuzzle.transform);
        created.PuzzleObject = physicsPuzzle;
    }
}
