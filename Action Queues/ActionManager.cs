using Shears.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shears.ActionQueues
{
    public class ActionManager : ProtectedSingleton<ActionManager>
    {
        [SerializeField] private List<ActionQueue> actionQueues;

        protected override void Awake()
        {
            base.Awake();

            actionQueues = FindObjectsByType<ActionQueue>(FindObjectsSortMode.None).ToList();
        }

        public static ActionQueue GetQueueFor(Component component) => GetQueueFor(component.gameObject);
        public static ActionQueue GetQueueFor(GameObject subject)
        {
            foreach (var queue in Instance.actionQueues)
            {
                if (queue.HasSubject(subject))
                    return queue;
            }

            Debug.LogWarning($"Could not find action queue for {subject.name}!");
            return null;
        }
    }
}
