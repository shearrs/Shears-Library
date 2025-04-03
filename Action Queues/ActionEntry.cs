using Shears.Common;
using System;
using System.Collections;
using UnityEngine;

namespace Shears.ActionQueues
{
    public class ActionEntry
    {
        private Func<bool> conditionalFunc = () => true;

        private readonly Action action;
        private readonly IEnumerator enumeratorAction;
        private readonly Func<Coroutine> coroutineAction;

        public bool Success { get; private set; } = false;

        #region Constructors
        public ActionEntry(Action action)
        {
            this.action = action;
        }

        public ActionEntry(IEnumerator action)
        {
            enumeratorAction = action;
        }

        public ActionEntry(Func<Coroutine> action)
        {
            coroutineAction = action;
        }

        public ActionEntry AddConditionalFunc(Func<bool> conditionalFunc)
        {
            this.conditionalFunc = conditionalFunc;

            return this;
        }
        #endregion

        public Coroutine Run() => CoroutineRunner.Start(IERun());

        private IEnumerator IERun()
        {
            if (conditionalFunc())
            {
                if (action != null)
                    action();
                else if (enumeratorAction != null)
                    yield return CoroutineRunner.Start(enumeratorAction);
                else
                    yield return coroutineAction();

                Success = true;
            }
        }
    }
}
