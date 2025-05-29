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
            private Coroutine coroutine;
            private readonly YieldInstruction yieldInstruction;

            public readonly Coroutine Coroutine => coroutine;
            public readonly YieldInstruction YieldInstruction => yieldInstruction;

            public ChainElement(Action action)
            {
                this.action = action;
                enumeratorAction = null;
                coroutineAction = null;
                coroutine = null;
                yieldInstruction = null;
            }

            public ChainElement(IEnumerator action)
            {
                enumeratorAction = action;
                this.action = null;
                coroutineAction = null;
                coroutine = null;
                yieldInstruction = null;
            }

            public ChainElement(Func<Coroutine> action)
            {
                coroutineAction = action;
                this.action = null;
                enumeratorAction = null;
                coroutine = null;
                yieldInstruction = null;
            }

            public ChainElement(YieldInstruction yieldInstruction)
            {
                this.yieldInstruction = yieldInstruction;
                action = null;
                enumeratorAction = null;
                coroutineAction = null;
                coroutine = null;
            }

            public void Run()
            {
                if (action != null)
                    action?.Invoke();
                else if (enumeratorAction != null)
                    coroutine = CoroutineRunner.Start(enumeratorAction);
                else if (coroutineAction != null)
                    coroutine = coroutineAction();

                // yield instruction doesn't run anything
            }
        }

        public static CoroutineChain Create()
        {
            return new();
        }

        public void Enqueue(Action action) => chainQueue.Enqueue(new(action));
        public void Enqueue(IEnumerator action) => chainQueue.Enqueue(new(action));
        public void Enqueue(Func<Coroutine> action) => chainQueue.Enqueue(new(action));
        public void Enqueue(YieldInstruction yieldInstruction) => chainQueue.Enqueue(new(yieldInstruction));

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

        public CoroutineChain WaitForSeconds(float seconds)
        {
            if (seconds <= 0)
                return this;

            chainQueue.Enqueue(new(CoroutineUtil.WaitForSeconds(seconds)));

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
                else if (element.YieldInstruction != null)
                    yield return element.YieldInstruction;
            }

            isRunning = false;
        }
    }
}
