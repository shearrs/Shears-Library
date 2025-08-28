using Shears.Logging;
using System;
using System.Collections.Generic;

namespace Shears.StateMachineGraphs
{
    public abstract class State<T> : State, IStateInjectable
    {
        IReadOnlyCollection<Type> IStateInjectable.GetInjectableTypes()
        {
            return new[] { typeof(T) };
        }

        void IStateInjectable.InjectType(object dependency)
        {
            if (dependency is T tDependency)
                Inject(tDependency);
            else
                this.Log($"Failed to inject dependency of type {dependency.GetType()} into state {GetType()}. Expected type: {typeof(T)}.", SHLogLevels.Error);
        }

        bool IStateInjectable.CanInjectType(Type type)
        {
            return type == typeof(T);
        }

        protected abstract void Inject(T dependency);
    }

    public abstract class State<T1, T2> : State, IStateInjectable
    {
        private T1 dependency1;
        private T2 dependency2;

        IReadOnlyCollection<Type> IStateInjectable.GetInjectableTypes()
        {
            return new[] { typeof(T1), typeof(T2) };
        }

        bool IStateInjectable.CanInjectType(Type type)
        {
            return type == typeof(T1) || type == typeof(T2);
        }

        void IStateInjectable.InjectType(object dependency)
        {
            if (dependency is T1 dependency1)
            {
                this.dependency1 = dependency1;
                
                if (dependency2 != null)
                    Inject(this.dependency1, dependency2);
            }
            else if (dependency is T2 dependency2)
            {
                this.dependency2 = dependency2;
                
                if (this.dependency1 != null)
                    Inject(this.dependency1, this.dependency2);
            }
            else
                Log($"Failed to inject dependency of type {dependency.GetType()} into state {GetType()}. Expected type: {typeof(T1)} or {typeof(T2)}.", SHLogLevels.Error);
        }

        protected abstract void Inject(T1 dependency1, T2 dependency2);
    }

    public abstract class State<T1, T2, T3> : State, IStateInjectable
    {
        private T1 dependency1;
        private T2 dependency2;
        private T3 dependency3;

        IReadOnlyCollection<Type> IStateInjectable.GetInjectableTypes()
        {
            return new[] { typeof(T1), typeof(T2), typeof(T3) };
        }

        bool IStateInjectable.CanInjectType(Type type)
        {
            return type == typeof(T1) || type == typeof(T2) || type == typeof(T3);
        }

        void IStateInjectable.InjectType(object dependency)
        {
            if (dependency is T1 dependency1)
            {
                this.dependency1 = dependency1;

                if (dependency2 != null && dependency3 != null)
                    Inject(this.dependency1, dependency2, dependency3);
            }
            else if (dependency is T2 dependency2)
            {
                this.dependency2 = dependency2;

                if (this.dependency1 != null && dependency3 != null)
                    Inject(this.dependency1, this.dependency2, dependency3);
            }
            else if (dependency is T3 dependency3)
            {
                this.dependency3 = dependency3;

                if (this.dependency1 != null && this.dependency2 != null)
                    Inject(this.dependency1, this.dependency2, this.dependency3);
            }
            else
                Log($"Failed to inject dependency of type {dependency.GetType()} into state {GetType()}. Expected type: {typeof(T1)}, {typeof(T2)}, or {typeof(T3)}.", SHLogLevels.Error);
        }

        protected abstract void Inject(T1 dependency1, T2 dependency2, T3 dependency3);
    }
}
