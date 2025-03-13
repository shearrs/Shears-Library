using System;
using UnityEngine;

namespace Shears.Interaction
{
    public interface IInteractor
    {
        public void Interact(IInteractable interactable);
    }

    public interface IInteractor<T> : IInteractor where T : IInteractable
    {
        public void TypeInteract(T interactable);
    }
}
