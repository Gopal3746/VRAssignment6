using UnityEngine;

public class StickMovement : MonoBehaviour
{
    [SerializeField] private Transform centerEyeAnchor;
    [SerializeField] private float movementSpeed = 5f;

    // Update is called once per frame
    void Update()
    {
        Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector3 inputDirection = new Vector2(stick.x, stick.y).normalized;
        Vector3 moveDirection = movementSpeed * Time.deltaTime * (centerEyeAnchor.forward * inputDirection.x + centerEyeAnchor.right * inputDirection.y);
        transform.Translate(moveDirection);
    }
}
