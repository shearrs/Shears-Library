using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.ActionQueues
{
    [Serializable]
    public class ActionEntry : IActionEntryStatusHandle
    {
#if UNITY_EDITOR
        [SerializeField, ReadOnly] private string name;
#endif

        private readonly List<Func<bool>> conditionals = new();
        private readonly Action action;
        private readonly IEnumerator enumeratorAction;
        private readonly Func<Coroutine> funcAction;

        public bool Success { get; private set; } = false;
        public Coroutine Coroutine { get; private set; }

        #region Constructors
        public ActionEntry(Action action, string name = "")
        {
#if UNITY_EDITOR
            if (name == "")
            {
                string className = action.Method.DeclaringType.ToString();
                int classEnd = className.IndexOf('+');

                if (classEnd > 0)
                    className = className[..classEnd];

                string methodName = action.Method.Name;
                int methodStart = methodName.IndexOf('<') + 1;
                int methodEnd = methodName.IndexOf('>');

                if (methodEnd - methodStart > 0)
                    methodName = methodName[methodStart..methodEnd];

                this.name = $"{className}.{methodName}";
            }
            else
                this.name = name;
#endif

                this.action = action;
        }

        public ActionEntry(IEnumerator action, string name = "")
        {
#if UNITY_EDITOR
            if (name == "")
                this.name = action.ToString();
            else
                this.name = name;
#endif

                enumeratorAction = action;
        }

        public ActionEntry(Func<Coroutine> action, string name = "")
        {
#if UNITY_EDITOR
            if (name == "")
                this.name = action.ToString();
            else
                this.name = name;
#endif

                funcAction = action;
        }
        #endregion

        public ActionEntry AddConditional(Func<bool> conditionalFunc)
        {
            conditionals.Add(conditionalFunc);

            return this;
        }

        public ActionEntry AddConditional(IActionEntryStatusHandle statusHandle)
        {
            conditionals.Add(() => statusHandle.Success);

            return this;
        }

        public void Run()
        {
            if (action != null && ConditionalsAreTrue())
            {
                action();
                Success = true;
            }
            else
                Coroutine = CoroutineRunner.Start(IERun());
        }

        private IEnumerator IERun()
        {
            if (ConditionalsAreTrue())
            {
                if (enumeratorAction != null)
                    yield return CoroutineRunner.Start(enumeratorAction);
                else
                    yield return funcAction();

                Success = true;
            }
        }

        private bool ConditionalsAreTrue()
        {
            if (conditionals.Count == 0)
                return true;

            foreach (var conditional in conditionals)
            {
                if (!conditional())
                    return false;
            }

            return true;
        }
    }
}
