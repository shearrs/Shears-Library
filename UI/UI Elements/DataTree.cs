using System.Collections.Generic;
using Shears.Logging;
using UnityEngine;

namespace Shears.UI
{
    public class DataTree<T>
        where T : class
    {
        private readonly Dictionary<T, TreeNode> nodeMap = new();
        private TreeNode root;

        public T Root => root?.Value;

        private class TreeNode
        {
            private readonly List<TreeNode> children = new();

            public T Value { get; }
            public int Depth { get; set; } = 0;
            public int Order { get; set; } = 0;
            public TreeNode Parent { get; private set; }
            public IReadOnlyList<TreeNode> Children => children;

            public TreeNode() { }

            public TreeNode(T element)
            {
                Value = element;
            }

            public void AddChild(TreeNode node)
            {
                node.Parent = this;
                node.Depth = Depth + 1;
                children.Add(node);
            }

            public void InsertChild(TreeNode node, int index)
            {
                if (index > children.Count)
                {
                    SHLogger.Log($"Index {index} is out of range.", SHLogLevels.Error);
                    return;
                }

                node.Parent = this;
                node.Depth = Depth + 1;
                children.Insert(index, node);
            }

            public bool RemoveChild(TreeNode node)
            {
                node.Parent = null;
                node.Depth = 0;

                if (children.Remove(node))
                {
                    return true;
                }
                else
                    return false;
            }

            public void RemoveAt(int index)
            {
                if (index > children.Count)
                {
                    SHLogger.Log($"Index {index} is out of range.", SHLogLevels.Error);
                    return;
                }

                var node = children[index];
                node.Parent = null;
                node.Depth = 0;

                children.RemoveAt(index);
            }

            public void Clear()
            {
                children.Clear();
            }
        }

        public void Add(T element, T parent = null)
        {
            TreeNode parentNode = null;

            if (parent != null)
            {
                if (!nodeMap.TryGetValue(parent, out parentNode))
                {
                    SHLogger.Log(
                        $"Data Tree does not contain parent: {parent}.",
                        SHLogLevels.Error
                    );
                    return;
                }

                var elementNode = new TreeNode(element);

                nodeMap[element] = elementNode;
                parentNode.AddChild(elementNode);

                RecalculateChildOrder(elementNode);

                return;
            }

            var newNode = new TreeNode(element);

            nodeMap[element] = newNode;

            if (parentNode != null)
            {
                parentNode.AddChild(newNode);
                RecalculateChildOrder(parentNode);
            }
            else if (root != null)
            {
                root.AddChild(newNode);
                RecalculateChildOrder(root);
            }
            else
                root = newNode;
        }

        public void InsertTree(DataTree<T> tree, T otherTreeValue, T parent, int index)
        {
            if (
                otherTreeValue == null
                || !tree.nodeMap.TryGetValue(otherTreeValue, out var otherNode)
            )
                return;

            if (!nodeMap.TryGetValue(parent, out var parentNode))
            {
                SHLogger.Log($"Data Tree does not contain parent: {parent}.", SHLogLevels.Error);
                return;
            }

            InsertTreeRecursive(otherNode, parentNode, index);
            RecalculateChildOrder(parentNode);
        }

        public void Insert(T element, T parent, int index)
        {
            if (!nodeMap.TryGetValue(parent, out var parentNode))
            {
                SHLogger.Log($"Data Tree does not contain parent: {parent}.", SHLogLevels.Error);
                return;
            }

            var newNode = new TreeNode(element);
            nodeMap[element] = newNode;
            parentNode.InsertChild(newNode, index);

            RecalculateChildOrder(parentNode);
        }

        public bool Remove(T element)
        {
            if (root != null && root.Value == element)
                return false;

            if (!nodeMap.TryGetValue(element, out var node))
                return false;

            var parent = node.Parent;

            if (parent == null)
            {
                SHLogger.Log($"Tried to remove node with null parent!", SHLogLevels.Error);
                return false;
            }

            if (parent.RemoveChild(node))
            {
                nodeMap.Remove(element);

                RecalculateChildOrder(parent);
                return true;
            }
            else
                return false;
        }

        public void Swap(T element0, T element1)
        {
            if (!nodeMap.TryGetValue(element0, out var node0))
            {
                SHLogger.Log($"Could not find node for element: {element0}.", SHLogLevels.Error);
                return;
            }

            if (!nodeMap.TryGetValue(element1, out var node1))
            {
                SHLogger.Log($"Could not find node for element: {element1}.", SHLogLevels.Error);
                return;
            }

            var node0Parent = node0.Parent;
            var node1Parent = node1.Parent;

            if (node0Parent == null || node1Parent == null)
            {
                SHLogger.Log($"Cannot swap as a node has a null parent.", SHLogLevels.Error);
                return;
            }

            int node0Index = node0Parent.Children.IndexOf(node0);
            int node1Index = node1Parent.Children.IndexOf(node1);

            node0Parent.RemoveAt(node0Index);
            node1Parent.RemoveAt(node1Index);

            node0Parent.InsertChild(node1, node0Index);
            node1Parent.InsertChild(node0, node1Index);

            if (node0Parent.Order < node1Parent.Order)
                RecalculateChildOrder(node0Parent);
            else
                RecalculateChildOrder(node1Parent);
        }

        public bool Contains(T element) => nodeMap.ContainsKey(element);

        public void Clear()
        {
            nodeMap.Clear();
            root.Clear();
        }

        public int GetDepth(T element)
        {
            if (!nodeMap.TryGetValue(element, out var node))
            {
                SHLogger.Log($"Data Tree does not contain element: {element}.", SHLogLevels.Error);
                return -1;
            }

            return node.Depth;
        }

        public int GetOrder(T element)
        {
            if (!nodeMap.TryGetValue(element, out var node))
            {
                SHLogger.Log($"Data Tree does not contain element: {element}.", SHLogLevels.Error);
                return -1;
            }

            return node.Order;
        }

        public T GetParent(T element)
        {
            if (!nodeMap.TryGetValue(element, out var node))
            {
                SHLogger.Log($"Data Tree does not contain element: {element}.", SHLogLevels.Error);
                return null;
            }
            else if (node == root)
                return null;

            return node.Parent.Value;
        }

        public void GetChildren(T element, List<T> children)
        {
            children.Clear();

            if (!nodeMap.TryGetValue(element, out var node))
            {
                SHLogger.Log($"Data Tree does not contain element: {element}.", SHLogLevels.Error);
                return;
            }

            GetChildrenRecursive(node, children);
        }

        public void GetDirectChildren(T element, List<T> children)
        {
            children.Clear();

            if (!nodeMap.TryGetValue(element, out var node))
            {
                SHLogger.Log($"Data Tree does not contain element: {element}.", SHLogLevels.Error);
                return;
            }

            foreach (var child in node.Children)
                children.Add(child.Value);
        }

        private void InsertTreeRecursive(TreeNode node, TreeNode parent, int index)
        {
            var nodeCopy = new TreeNode(node.Value);
            nodeMap[node.Value] = nodeCopy;
            parent.InsertChild(nodeCopy, index);

            foreach (var child in node.Children)
                InsertTreeRecursive(child, nodeCopy, index);
        }

        private void GetChildrenRecursive(TreeNode node, List<T> children)
        {
            foreach (var child in node.Children)
            {
                children.Add(child.Value);
                GetChildrenRecursive(child, children);
            }
        }

        private void RecalculateChildOrder(TreeNode startNode)
        {
            if (startNode == root)
            {
                RecalculateOrderRecursive(root, 0);
                return;
            }

            RecalculateOrderRecursive(startNode, startNode.Order);
        }

        private void RecalculateOrderRecursive(TreeNode node, int order)
        {
            node.Order = order;

            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];

                RecalculateOrderRecursive(child, order + 1 + i);
            }
        }
    }
}
