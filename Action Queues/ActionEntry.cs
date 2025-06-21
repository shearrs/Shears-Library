using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.ActionQueues
{
    [Serializable]
    public struct ActionEntry : IActionEntryStatusHandle
    {
#if UNITY_EDITOR
        [SerializeField, ReadOnly] private string name;
#endif

        private Coroutine coroutine;
        private bool success;
        private readonly List<Func<bool>> conditionals;
        private readonly Action action;
        private readonly IEnumerator enumeratorAction;
        private readonly Func<Coroutine> funcAction;
        private readonly YieldInstruction yieldAction;

        public readonly bool Success => success;
        public readonly Coroutine Coroutine => coroutine;

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
            success = false;
            conditionals = new();
            enumeratorAction = null;
            funcAction = null;
            coroutine = null;
            yieldAction = null;


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
            success = false;
            conditionals = new();
            this.action = null;
            funcAction = null;
            coroutine = null;
            yieldAction = null;

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
            success = false;
            conditionals = new();
            this.action = null;
            enumeratorAction = null;
            coroutine = null;
            yieldAction = null;

            funcAction = action;
        }
        #endregion

        public ActionEntry(YieldInstruction action, string name = "")
        {
#if UNITY_EDITOR
            if (name == "")
                this.name = action.ToString();
            else
                this.name = name;
#endif

            success = false;
            conditionals = new();
            this.action = null;
            enumeratorAction = null;
            funcAction = null;
            coroutine = null;

            yieldAction = action;
        }

        public readonly ActionEntry AddConditional(Func<bool> conditionalFunc)
        {
            conditionals.Add(conditionalFunc);

            return this;
        }

        public readonly ActionEntry AddConditional(IActionEntryStatusHandle statusHandle)
        {
            conditionals.Add(() => statusHandle.Success);

            return this;
        }

        public void Run()
        {
            if (action != null && ConditionalsAreTrue())
            {
                action();
                success = true;
            }
            else
                coroutine = CoroutineRunner.Start(IERun());
        }

        private IEnumerator IERun()
        {
            if (ConditionalsAreTrue())
            {
                if (enumeratorAction != null)
                    yield return CoroutineRunner.Start(enumeratorAction);
                else if (yieldAction != null)
                    yield return yieldAction;
                else
                    yield return funcAction();

                success = true;
            }
        }

        private readonly bool ConditionalsAreTrue()
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
