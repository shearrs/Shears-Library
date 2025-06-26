using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    public class CoroutineChain
    {
        private UnityEngine.Object owner;
        private Coroutine coroutine;
        private bool hasOwner = false;
        private bool isRunning = false;
        private readonly Queue<ChainAction> chainQueue = new();

        public int Count => chainQueue.Count;
        public bool IsRunning => isRunning;

        public struct ChainAction
        {
            private readonly Action action;
            private readonly IEnumerator enumeratorAction;
            private readonly Func<Coroutine> coroutineAction;
            private Coroutine coroutine;
            private readonly YieldInstruction yieldInstruction;

            public readonly Coroutine Coroutine => coroutine;
            public readonly YieldInstruction YieldInstruction => yieldInstruction;

            public ChainAction(Action action)
            {
                this.action = action;
                enumeratorAction = null;
                coroutineAction = null;
                coroutine = null;
                yieldInstruction = null;
            }

            public ChainAction(IEnumerator action)
            {
                enumeratorAction = action;
                this.action = null;
                coroutineAction = null;
                coroutine = null;
                yieldInstruction = null;
            }

            public ChainAction(Func<Coroutine> action)
            {
                coroutineAction = action;
                this.action = null;
                enumeratorAction = null;
                coroutine = null;
                yieldInstruction = null;
            }

            public ChainAction(YieldInstruction yieldInstruction)
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

        public CoroutineChain WithLifetime(UnityEngine.Object owner)
        {
            hasOwner = true;
            this.owner = owner;

            return this;
        }

        public void Enqueue(ChainAction action) => chainQueue.Enqueue(action);
        public void Enqueue(Action action) => Enqueue(new ChainAction(action));
        public void Enqueue(IEnumerator action) => Enqueue(new ChainAction(action));
        public void Enqueue(Func<Coroutine> action) => Enqueue(new ChainAction(action));
        public void Enqueue(YieldInstruction yieldInstruction) => Enqueue(new ChainAction(yieldInstruction));

        public CoroutineChain Then(ChainAction action)
        {
            Enqueue(action);

            return this;
        }
        public CoroutineChain Then(Action action) => Then(new ChainAction(action));
        public CoroutineChain Then(IEnumerator action) => Then(new ChainAction(action));
        public CoroutineChain Then(Func<Coroutine> action) => Then(new ChainAction(action));

        public CoroutineChain IfThen(bool condition, ChainAction action)
        {
            if (condition)
                Enqueue(action);

            return this;
        }
        public CoroutineChain IfThen(bool condition, Action action) => IfThen(condition, new ChainAction(action));
        public CoroutineChain IfThen(bool condition, IEnumerator action) => IfThen(condition, new ChainAction(action));
        public CoroutineChain IfThen(bool condition, Func<Coroutine> action) => IfThen(condition, new ChainAction(action));
        public CoroutineChain IfThen(Func<bool> condition, ChainAction action)
        {
            if (condition())
                Enqueue(action);

            return this;
        }
        public CoroutineChain IfThen(Func<bool> condition, Action action) => IfThen(condition, new ChainAction(action));
        public CoroutineChain IfThen(Func<bool> condition, IEnumerator action) => IfThen(condition, new ChainAction(action));
        public CoroutineChain IfThen(Func<bool> condition, Func<Coroutine> action) => IfThen(condition, new ChainAction(action));

        public CoroutineChain IfThenElse(bool condition, ChainAction ifAction, ChainAction elseAction)
        {
            if (condition)
                Enqueue(ifAction);
            else
                Enqueue(elseAction);

            return this;
        }
        public CoroutineChain IfThenElse(bool condition, Action ifAction, Action elseAction) => IfThenElse(condition, new ChainAction(ifAction), new ChainAction(elseAction));
        public CoroutineChain IfThenElse(bool condition, IEnumerator ifAction, IEnumerator elseAction) => IfThenElse(condition, new ChainAction(ifAction), new ChainAction(elseAction));
        public CoroutineChain IfThenElse(bool condition, Func<Coroutine> ifAction, Func<Coroutine> elseAction) => IfThenElse(condition, new ChainAction(ifAction), new ChainAction(elseAction));
        public CoroutineChain IfThenElse(Func<bool> condition, ChainAction ifAction, ChainAction elseAction)
        {
            if (condition())
                Enqueue(ifAction);
            else
                Enqueue(elseAction);

            return this;
        }
        public CoroutineChain IfThenElse(Func<bool> condition, Action ifAction, Action elseAction) => IfThenElse(condition, new ChainAction(ifAction), new ChainAction(elseAction));
        public CoroutineChain IfThenElse(Func<bool> condition, IEnumerator ifAction, IEnumerator elseAction) => IfThenElse(condition, new ChainAction(ifAction), new ChainAction(elseAction));
        public CoroutineChain IfThenElse(Func<bool> condition, Func<Coroutine> ifAction, Func<Coroutine> elseAction) => IfThenElse(condition, new ChainAction(ifAction), new ChainAction(elseAction));

        public CoroutineChain DoForDuration(Action action, float duration)
        {
            if (duration > 0)
                chainQueue.Enqueue(new(IEDoForDuration(action, duration)));

            return this;
        }

        public CoroutineChain DoOnInterval(Action action, float duration, float interval)
        {
            if (duration > 0)
                chainQueue.Enqueue(new(IEDoOnInterval(action, duration, interval)));

            return this;
        }

        public CoroutineChain Tween(Action<float> update, float duration)
        {
            if (duration > 0)
                chainQueue.Enqueue(new(IETween(update, duration)));

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

            coroutine = CoroutineRunner.Start(IERun());

            return coroutine;
        }

        public void Stop()
        {
            if (coroutine == null)
                return;

            CoroutineRunner.Stop(coroutine);
        }

        public void Clear()
        {
            chainQueue.Clear();
        }

        private IEnumerator IEDoForDuration(Action action, float duration)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                if (OwnerIsDestroyed())
                    break;

                action?.Invoke();

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator IEDoOnInterval(Action action, float duration, float interval)
        {
            float elapsedTime = 0f;
            var wait = CoroutineUtil.WaitForSeconds(interval);

            while (elapsedTime < duration)
            {
                if (OwnerIsDestroyed())
                    break;

                action?.Invoke();

                elapsedTime += Time.deltaTime;
                yield return wait;
            }
        }

        private IEnumerator IETween(Action<float> update, float duration)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                if (OwnerIsDestroyed())
                    break;

                update?.Invoke(elapsedTime / duration);

                elapsedTime += Time.deltaTime;

                yield return null;
            }
        }

        private IEnumerator IERun()
        {
            isRunning = true;

            while (chainQueue.Count > 0)
            {
                if (OwnerIsDestroyed())
                    break;

                var element = chainQueue.Dequeue();

                element.Run();

                if (element.Coroutine != null)
                    yield return element.Coroutine;
                else if (element.YieldInstruction != null)
                    yield return element.YieldInstruction;
            }

            isRunning = false;
        }

        private bool OwnerIsDestroyed() => hasOwner && owner == null;
    }
}
