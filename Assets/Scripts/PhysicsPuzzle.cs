using UnityEngine;

public class PhysicsPuzzle : MonoBehaviour
{
    public void Win()
    {
        Debug.Log("WINNNNNNNNNNNNNN");
        GameManager.Instance.OpenExitDoor();
    }
}
