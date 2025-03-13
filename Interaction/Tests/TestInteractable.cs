using UnityEngine;

namespace Shears.Interaction
{
    public class TestInteractable : MonoBehaviour, IInteractable
    {
        void IInteractable.Accept(IInteractor interactor)
        {
            interactor.Interact(this);
        }
    }
}
