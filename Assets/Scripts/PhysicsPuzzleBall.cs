using UnityEngine;

public class PhysicsPuzzleBall : MonoBehaviour
{
    public PhysicsPuzzle PuzzleObject { get; set; }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("collided with " + collision.collider.gameObject.name);
        if (collision.collider.CompareTag("PhysicsPuzzleGoal"))
        {
            //Debug.Log("TODO WIN");
            PuzzleObject.Win();
        }
    }
}
