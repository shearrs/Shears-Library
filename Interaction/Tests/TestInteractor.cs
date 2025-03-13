using UnityEngine;

namespace Shears.Interaction
{
    public class TestInteractor : Interactor<IInteractable>
    {
        public override void TypeInteract(IInteractable interactable)
        {
            Debug.Log($"Interacted with {interactable}!");
        }
    }
}
