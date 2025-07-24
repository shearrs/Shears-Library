using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.GraphViews.Editor
{
    public static class GraphViewClipboard
    {
        const string CLIPBOARD_KEY = "Graph View - ";

        public static event Action<IReadOnlyList<GraphElementClipboardData>> OnPaste;

        public static void CopyToClipboard(JsonList<GraphElementClipboardData> data)
        {
            var json = CLIPBOARD_KEY + JsonUtility.ToJson(data);
            Debug.Log("copy to clipboard: " + json);

            GUIUtility.systemCopyBuffer = json;
        }

        public static void PasteFromClipboard()
        {
            Debug.Log("paste");
            var buffer = GUIUtility.systemCopyBuffer;

            if (!buffer.StartsWith(CLIPBOARD_KEY))
                return;

            Debug.Log("valid buffer");

            buffer = buffer[CLIPBOARD_KEY.Length..];

            var data = JsonUtility.FromJson<JsonList<GraphElementClipboardData>>(buffer);

            if (data != null)
                OnPaste?.Invoke(data);
        }
    }
}
