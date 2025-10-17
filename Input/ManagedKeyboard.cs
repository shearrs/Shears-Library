using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shears.Input
{
    public static class ManagedKeyboard
    {
        public static event Action<char> OnTextInput { add => Keyboard.current.onTextInput += value; remove => Keyboard.current.onTextInput -= value; }

        public static bool IsKeyPressed(string displayName)
        {
            var key = Keyboard.current.FindKeyOnCurrentKeyboardLayout(displayName);
            
            return key.isPressed;
        }

        public static void GetKeysPressedThisFrame(List<ManagedKey> pressedKeys)
        {
            pressedKeys.Clear();
            
            if (Keyboard.current == null)
                return;
            
            foreach (var control in Keyboard.current.allKeys)
            {
                if (control == null)
                    continue;

                if (control.wasPressedThisFrame)
                    pressedKeys.Add(KeyTranslation.TranslateKey(control.keyCode));
            }
        }
    }
}
