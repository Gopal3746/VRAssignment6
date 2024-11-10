using UnityEngine;

public class GrabbableVase : RayTappableObject
{
    [SerializeField] private bool isCorrectVase;
    [SerializeField] private GameObject hintObject;

    protected override void TriggerAction()
    {
        // nothing, we only care about it being placed on the correct pedestal
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isCorrectVase)
        {
            if (collision.collider.CompareTag("PlacingPuzzlePedestal"))
            {
                Debug.Log("CORRECT FOR PLACING PUZZLE");
                hintObject.SetActive(true);
            }
        }
    }
}
