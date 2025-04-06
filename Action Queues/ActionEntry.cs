using Shears.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.ActionQueues
{
    [Serializable]
    public class ActionEntry : IActionEntryStatusHandle
    {
        [SerializeField] private string name;

        private readonly List<Func<bool>> conditionals = new();
        private readonly Action action;
        private readonly IEnumerator enumeratorAction;
        private readonly Func<Coroutine> coroutineAction;

        public bool Success { get; private set; } = false;
        public Coroutine Coroutine { get; private set; }

        #region Constructors
        public ActionEntry(Action action)
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

            name = $"{className}.{methodName}";

            this.action = action;
        }

        public ActionEntry(IEnumerator action)
        {
            name = action.ToString();

            enumeratorAction = action;
        }

        public ActionEntry(Func<Coroutine> action)
        {
            name = action.ToString();

            coroutineAction = action;
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
                    yield return coroutineAction();

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
