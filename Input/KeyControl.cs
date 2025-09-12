using UnityEngine;
using UnityEngine.InputSystem;

using OldControl = UnityEngine.InputSystem.Controls.KeyControl;
using OldKey = UnityEngine.InputSystem.Key;

namespace Shears.Input
{
    public enum Key
    {
        None,
        Q,
        W,
        E,
        R,
        T,
        Y,
        U,
        I,
        O,
        P,
        A,
        S,
        D,
        F,
        G,
        H,
        J,
        K,
        L,
        Z,
        X,
        C,
        V,
        B,
        N,
        M
    }

    public class KeyControl
    {
        private OldControl oldControl;

        public Key Key => 

        private Key TranslateKey(OldKey oldKey)
        {
            return oldKey switch
            {

            }
        }
    }
}
