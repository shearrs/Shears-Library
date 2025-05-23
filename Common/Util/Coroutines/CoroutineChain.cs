using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    public class CoroutineChain
    {
        private readonly Queue<ChainElement> chainQueue = new();

        private bool isRunning = false;

        public int Count => chainQueue.Count;
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

        public void Enqueue(Action action) => chainQueue.Enqueue(new(action));
        public void Enqueue(IEnumerator action) => chainQueue.Enqueue(new(action));
        public void Enqueue(Func<Coroutine> action) => chainQueue.Enqueue(new(action));

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

        public CoroutineChain IfThen(bool condition, Action action)
        {
            if (condition)
                chainQueue.Enqueue(new(action));

            return this;
        }
        public CoroutineChain IfThen(bool condition, IEnumerator action)
        {
            if (condition)
                chainQueue.Enqueue(new(action));

            return this;
        }
        public CoroutineChain IfThen(bool condition, Func<Coroutine> action)
        {
            if (condition)
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

            while (chainQueue.Count > 0)
            {
                var element = chainQueue.Dequeue();

                element.Run();

                if (element.Coroutine != null)
                    yield return element.Coroutine;
            }

            isRunning = false;
        }
    }
}
