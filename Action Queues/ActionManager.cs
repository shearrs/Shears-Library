using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shears.ActionQueues
{
    public class ActionManager : ProtectedSingleton<ActionManager>
    {
        [SerializeField] private List<ActionQueue> actionQueues;
        private readonly Dictionary<Component, ActionQueue> componentQueueDictionary = new();
        private readonly Dictionary<GameObject, ActionQueue> gameObjectQueueDictionary = new();

        protected override void Awake()
        {
            base.Awake();

            actionQueues = FindObjectsByType<ActionQueue>(FindObjectsSortMode.None).ToList();
        }

        internal static ActionQueue GetQueueFor(Component component) => Instance.InstGetQueueFor(component);
        internal static ActionQueue GetQueueFor(GameObject gameObject) => Instance.InstGetQueueFor(gameObject);

        private ActionQueue InstGetQueueFor(Component component)
        {
            if (componentQueueDictionary.TryGetValue(component, out ActionQueue queue))
                return queue;

            return InstGetQueueFor(component.gameObject);
        }
        private ActionQueue InstGetQueueFor(GameObject gameObject)
        {
            if (gameObjectQueueDictionary.TryGetValue(gameObject, out ActionQueue queue))
                return queue;

            return FindActionQueue(gameObject);
        }

        internal static ActionQueue GetQueueWithTag(string name) => Instance.InstGetQueueWithName(name);
        private ActionQueue InstGetQueueWithName(string tag)
        {
            foreach (var queue in actionQueues)
            {
                if (queue.CompareTag(tag))
                    return queue;
            }

            Debug.LogWarning($"Could not find action queue with tag {tag}!");
            return null;
        }

        private ActionQueue FindActionQueue(GameObject gameObject)
        {
            foreach (var queue in actionQueues)
            {
                if (queue.HasSubject(gameObject))
                {
                    gameObjectQueueDictionary.Add(gameObject, queue);

                    return queue;
                }
            }

            Debug.LogWarning($"Could not find action queue for {gameObject.name}!");
            return null;
        }
    }
}
