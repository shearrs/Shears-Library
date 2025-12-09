using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Shears.Input
{
    public static class ManagedKeyboard
    {
        private static Keyboard current;
        private static readonly Dictionary<ManagedKey, KeyControl> keyTranslationCache = new();

        public static event Action<char> TextInput { add => Keyboard.current.onTextInput += value; remove => Keyboard.current.onTextInput -= value; }

        public static bool IsKeyPressed(ManagedKey key)
        {
            if (current != Keyboard.current)
                InitializeKeyboard();

            return keyTranslationCache[key].isPressed;
        }

        public static bool WasKeyPressedThisFrame(ManagedKey key)
        {
            if (current != Keyboard.current)
                InitializeKeyboard();

            return keyTranslationCache[key].wasPressedThisFrame;
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

        private static void InitializeKeyboard()
        {
            current = Keyboard.current;
            keyTranslationCache.Clear();

            foreach (var control in Keyboard.current.allKeys)
                keyTranslationCache[KeyTranslation.TranslateKey(control.keyCode)] = control;
        }
    }
}
