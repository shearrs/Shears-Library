using UnityEngine;

namespace Shears.Input
{
    public class ManagedInputGroup
    {
        private readonly ManagedInputBindingInstance[] bindings;

        internal ManagedInputGroup(ManagedInputBindingInstance[] bindings)
        {
            this.bindings = bindings;
        }

        public void Bind()
        {
            foreach (var binding in bindings)
                binding.Bind();
        }

        public void Unbind()
        {
            foreach (var binding in bindings)
                binding.Unbind();
        }
    }
}
