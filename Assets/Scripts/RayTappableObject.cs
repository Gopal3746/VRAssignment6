using Oculus.Haptics;
using Oculus.Interaction;
using UnityEngine;

public abstract class RayTappableObject : MonoBehaviour
{
    [SerializeField, Interface(typeof(IInteractableView))] private Object interactableView;
    [SerializeField] private GameObject hoverObject;
    [SerializeField] private HapticClip hapticClip;

    private IInteractableView _interactableView;
    private InteractableState previousState;
    private HapticClipPlayer hapticPlayer;

    protected void Start()
    {
        _interactableView = interactableView as IInteractableView;
        previousState = InteractableState.Normal;
        hapticPlayer = new HapticClipPlayer(hapticClip);
    }

    protected void Update()
    {
        //Debug.Log(_interactableView.State.ToString());
        InteractableState currentState = _interactableView.State;
        if (previousState != InteractableState.Select && currentState == InteractableState.Select && GameManager.Instance.gameActive)
        {
            TriggerAction();
        }
        if (currentState == InteractableState.Normal)
        {
            hoverObject.SetActive(false);
        } else if (currentState == InteractableState.Select || currentState == InteractableState.Hover)
        {
            hoverObject.SetActive(true);
        }
        previousState = currentState;
    }

    protected virtual void TriggerAction()
    {

        bool rightPressed = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
        bool leftPressed = OVRInput.Get(OVRInput.RawButton.LIndexTrigger);
        if (rightPressed && leftPressed)
        {
            hapticPlayer.Play(Controller.Both);
        } else if (leftPressed)
        {
            hapticPlayer.Play(Controller.Left);
        } else
        {
            hapticPlayer.Play(Controller.Right);
        }
    }
}
