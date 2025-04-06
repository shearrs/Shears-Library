using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.ActionQueues
{
    public class ActionQueue : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private List<ActionEntry> entryDisplay;
#endif
        [SerializeField] private List<GameObject> subjects;
        [SerializeField] private List<ActionQueue> subQueues;

        private HashSet<GameObject> subjectSet;
        private readonly Queue<ActionEntry> queue = new();

        private HashSet<GameObject> SubjectSet
        {
            get
            {
                if (subjectSet == null)
                {
                    subjectSet = new();

                    foreach (var subject in subjects)
                        subjectSet.Add(subject);
                }

                return subjectSet;
            }
        }

        public bool IsRunning { get; private set; }

        private void Awake()
        {
            StartCoroutine(IEExecuteActions());
        }

        #region Get Functions
        public static ActionQueue For(Component component) => For(component.gameObject);
        public static ActionQueue For(GameObject gameObject) => ActionManager.GetQueueFor(gameObject);
        public static ActionQueue WithTag(string name) => ActionManager.GetQueueWithTag(name);

        public bool HasSubject(Component component) => SubjectSet.Contains(component.gameObject);
        public bool HasSubject(GameObject subject) => SubjectSet.Contains(subject);
        #endregion

        #region Enqueues
        public ActionEntry Enqueue(Action action) => EnqueueAction(new(action));
        public ActionEntry Enqueue(IEnumerator action) => EnqueueAction(new(action));
        public ActionEntry Enqueue(Func<Coroutine> action) => EnqueueAction(new(action));

        public ActionEntry EnqueueAction(ActionEntry entry)
        {
            queue.Enqueue(entry);

#if UNITY_EDITOR
            entryDisplay.Add(entry);
#endif

            return entry;
        }
        #endregion

        private IEnumerator IEExecuteActions()
        {
            while (true)
            {
                while (queue.Count == 0)
                {
                    yield return null;
                }

                var entry = queue.Dequeue();

#if UNITY_EDITOR
                entryDisplay.Remove(entry);
#endif
                IsRunning = true;

                entry.Run();

                if (entry.Coroutine != null)
                    yield return entry.Coroutine;

                while (IsSubQueueRunning())
                    yield return null;

                IsRunning = false;
            }
        }

        private bool IsSubQueueRunning()
        {
            foreach (var subQueue in subQueues)
                if (subQueue.IsRunning)
                    return true;

            return false;
        }
    }
}
