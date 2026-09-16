using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Shears.Grids
{
    [Serializable]
    [DisallowMultipleComponent]
    public class DoorwayNodeData : GridNodeData
    {
        private static int ENTITY_LAYER = -1;

        [SerializeField]
        private bool isExit;

        [SerializeField]
        private Vector3 travelOffset = new(0, 0, 1f);

        [SerializeField]
        private Vector3 exitOffset = new(2, 0, 0);

        private readonly Dictionary<GameObject, int> objectLayers = new();

        protected override Color EditorColor => Color.cyan;
        public GridNode ConnectedNode { get; set; }
        public bool IsExit => isExit;
        public Vector3 TravelOffset => travelOffset;
        public Vector3 ExitOffset => exitOffset;
        public bool IsConnected => ConnectedNode != null;
        public Vector3Int ConnectedGridPosition
        {
            get
            {
                if (IsConnected)
                    return ConnectedNode.GridPosition;
                else
                    return Vector3Int.zero;
            }
        }

        public void SetEntityLayers(GameObject gameObject)
        {
            if (ENTITY_LAYER == -1)
                ENTITY_LAYER = LayerMask.NameToLayer("Doorway Entity");

            StoreLayers(gameObject);

            gameObject.SetLayerOnAllChildren(ENTITY_LAYER);
        }

        public void ClearEntityLayers(GameObject gameObject)
        {
            gameObject.layer = objectLayers[gameObject];
            objectLayers.Remove(gameObject);

            for (int i = 0; i < gameObject.transform.childCount; i++)
                ClearEntityLayers(gameObject.transform.GetChild(i).gameObject);
        }

        public bool IsAffectingEntity(GameObject gameObject)
        {
            return objectLayers.ContainsKey(gameObject);
        }

        private void StoreLayers(GameObject gameObject)
        {
            objectLayers[gameObject] = gameObject.layer;

            for (int i = 0; i < gameObject.transform.childCount; i++)
                StoreLayers(gameObject.transform.GetChild(i).gameObject);
        }

        protected override void OnDrawHandles(GridNodeHandleContext context)
        {
            var nodePosition = context.NodePosition;
            var nodeSize = context.NodeSize;

#if UNITY_EDITOR
            var startPosition = nodePosition + 0.5f * context.Grid.NodeSize * Vector3.down;

            var color = Color.yellow;
            color.a = 0.85f;
            Handles.color = color;
            Handles.DrawWireDisc(startPosition, Vector3.up, 0.15f);

            color = Color.orangeRed;
            color.a = 0.85f;
            Handles.color = color;
            Handles.DrawWireDisc(startPosition + travelOffset, Vector3.up, 0.15f);

            color = Color.red;
            color.a = 0.85f;
            Handles.color = color;
            Handles.DrawWireDisc(startPosition + travelOffset + exitOffset, Vector3.up, 0.15f);

            Gizmos.color = Color.white;
            Handles.DrawLine(startPosition, startPosition + travelOffset);
            Handles.DrawLine(
                startPosition + travelOffset,
                startPosition + travelOffset + exitOffset
            );

            if (IsConnected)
            {
                Handles.color = Color.green;
                var connectedPosition = context.Grid.GetLocalPosition(ConnectedNode);

                Handles.DrawLine(
                    startPosition + travelOffset + exitOffset,
                    connectedPosition + (0.5f * nodeSize * Vector3.down),
                    0.15f
                );
            }
#endif
        }
    }
}
