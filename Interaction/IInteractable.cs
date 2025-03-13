using UnityEngine;

namespace Shears.Interaction
{
    public interface IInteractable
    {
        public void Accept(IInteractor interactor);
    }
}
