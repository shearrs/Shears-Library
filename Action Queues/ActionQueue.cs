using Shears;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.ActionQueues
{
    public class ActionQueue : MonoBehaviour
    {
        [SerializeField] private List<GameObject> subjects;

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

        private void Awake()
        {
            StartCoroutine(IEExecuteActions());
        }

        public bool HasSubject(Component component) => SubjectSet.Contains(component.gameObject);
        public bool HasSubject(GameObject subject) => SubjectSet.Contains(subject);

        public ActionEntry Enqueue(Action action)
        {
            var entry = new ActionEntry(action);

            return entry;
        }
        
        public ActionEntry Enqueue(IEnumerator action)
        {
            var entry = new ActionEntry(action);

            return entry;
        }

        public ActionEntry Enqueue(Func<Coroutine> action)
        {
            var entry = new ActionEntry(action);

            return entry;
        }

        public void EnqueueAction(ActionEntry entry) => queue.Enqueue(entry);

        private IEnumerator IEExecuteActions()
        {
            while (true)
            {
                while (queue.Count == 0)
                    yield return null;

                var entry = queue.Dequeue();

                yield return entry.Run();
            }
        }
    }
}
