using System;
using UnityEditor;
using UnityEngine;

namespace Shears.GraphViews.Editor
{
    public abstract class GraphViewEditorWindow : EditorWindow
    {
        public event Action NonGraphWindowFocused;

        private void OnFocus()
        {
            if (!EditorApplication.isFocused)
                return;

            windowFocusChanged -= OnWindowFocusChanged;
            windowFocusChanged += OnWindowFocusChanged;
        }

        public void OnDestroy()
        {
            windowFocusChanged -= OnWindowFocusChanged;
        }

        private void OnWindowFocusChanged()
        {
            if (focusedWindow != this && focusedWindow.titleContent.text != "Inspector" && focusedWindow.titleContent.text != "Debug")
            {
                windowFocusChanged -= OnWindowFocusChanged;
                NonGraphWindowFocused?.Invoke();
            }
        }
    }
}
