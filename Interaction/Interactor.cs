using UnityEngine;

namespace Shears.Interaction
{
    public abstract class Interactor : MonoBehaviour, IInteractor
    {
        public abstract void Interact(IInteractable interactable);
    }

    public abstract class Interactor<T> : Interactor, IInteractor<T> where T : IInteractable
    {
        public override void Interact(IInteractable interactable)
        {
            if (interactable is T typedInteractable)
                TypeInteract(typedInteractable);
        }

        public abstract void TypeInteract(T interactable);
    }
}
