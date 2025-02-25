using Shears.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    public class CoroutineChain
    {
        private IEnumerator start;
        private Func<Coroutine> coroutineStart;
        private bool isRunning = false;

        private readonly Queue<ChainElement> chainQueue = new();

        private struct ChainElement
        {
            private readonly Action action;
            private readonly IEnumerator enumeratorAction;
            private readonly Func<Coroutine> coroutineAction;

            public Coroutine Coroutine { get; private set; }

            public ChainElement(Action action)
            {
                this.action = action;
                enumeratorAction = null;
                coroutineAction = null;

                Coroutine = null;
            }

            public ChainElement(IEnumerator action)
            {
                enumeratorAction = action;
                this.action = null;
                coroutineAction = null;

                Coroutine = null;
            }

            public ChainElement(Func<Coroutine> action)
            {
                coroutineAction = action;
                this.action = null;
                enumeratorAction = null;

                Coroutine = null;
            }

            public void Run()
            {
                if (action != null)
                    action();
                else if (enumeratorAction != null)
                    Coroutine = CoroutineRunner.Start(enumeratorAction);
                else
                    Coroutine = coroutineAction();
            }
        }

        public static CoroutineChain Create()
        {
            return new();
        }

        public CoroutineChain SetStart(IEnumerator action)
        {
            start = action;

            return this;
        }

        public CoroutineChain SetStart(Func<Coroutine> action)
        {
            coroutineStart = action;

            return this;
        }

        public CoroutineChain Then(Action action)
        {
            chainQueue.Enqueue(new(action));

            return this;
        }

        public CoroutineChain Then(IEnumerator action)
        {
            chainQueue.Enqueue(new(action));

            return this;
        }

        public CoroutineChain Then(Func<Coroutine> action)
        {
            chainQueue.Enqueue(new(action));

            return this;
        }

        public Coroutine Run()
        {
            if (isRunning)
                return null;

            return CoroutineRunner.Start(IERun());
        }

        private IEnumerator IERun()
        {
            isRunning = true;

            if (start != null)
                yield return CoroutineRunner.Start(start);
            else
                yield return coroutineStart();

            foreach (var element in chainQueue)
            {
                element.Run();

                if (element.Coroutine != null)
                    yield return element.Coroutine;
            }

            isRunning = false;
        }
    }
}
