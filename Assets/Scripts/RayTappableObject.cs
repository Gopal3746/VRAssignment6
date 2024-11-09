using Oculus.Interaction;
using UnityEngine;

public abstract class RayTappableObject : MonoBehaviour
{
    [SerializeField, Interface(typeof(IInteractableView))] private Object interactableView;
    [SerializeField] private GameObject hoverObject;

    private IInteractableView _interactableView;
    private InteractableState previousState;

    protected void Start()
    {
        _interactableView = interactableView as IInteractableView;
        previousState = InteractableState.Normal;
    }

    protected void Update()
    {
        //Debug.Log(_interactableView.State.ToString());
        InteractableState currentState = _interactableView.State;
        if (previousState != InteractableState.Select && currentState == InteractableState.Select)
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

    protected abstract void TriggerAction();
}
