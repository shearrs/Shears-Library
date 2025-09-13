using UnityEngine;
using UnityEngine.InputSystem;

namespace Shears.Input
{
    public enum ManagedKey
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
        M,
        Backspace,
        Enter
    }

    public static class KeyTranslation
    {
        internal static ManagedKey TranslateKey(Key oldKey)
        {
            return oldKey switch
            {
                Key.Q => ManagedKey.Q,
                Key.W => ManagedKey.W,
                Key.E => ManagedKey.E,
                Key.R => ManagedKey.R,
                Key.T => ManagedKey.T,
                Key.Y => ManagedKey.Y,
                Key.U => ManagedKey.U,
                Key.I => ManagedKey.I,
                Key.O => ManagedKey.O,
                Key.P => ManagedKey.P,
                Key.A => ManagedKey.A,
                Key.S => ManagedKey.S,
                Key.D => ManagedKey.D,
                Key.F => ManagedKey.F,
                Key.G => ManagedKey.G,
                Key.H => ManagedKey.H,
                Key.J => ManagedKey.J,
                Key.K => ManagedKey.K,
                Key.L => ManagedKey.L,
                Key.Z => ManagedKey.Z,
                Key.X => ManagedKey.X,
                Key.C => ManagedKey.C,
                Key.V => ManagedKey.V,
                Key.B => ManagedKey.B,
                Key.N => ManagedKey.N,
                Key.M => ManagedKey.M,
                Key.Backspace => ManagedKey.Backspace,
                Key.Enter => ManagedKey.Enter,
                _ => ManagedKey.None
            };
        }
    
        public static ManagedKey GetKey(string displayName)
        {
            if (displayName.Length > 1)
                displayName = $"{char.ToUpper(displayName[0])}{displayName[1..].ToLower()}";
            else
                displayName = displayName.ToUpper();

            Debug.Log("name: " + displayName);

            return displayName switch
            {
                "Q" => ManagedKey.Q,
                "W" => ManagedKey.W,
                "E" => ManagedKey.E,
                "R" => ManagedKey.R,
                "T" => ManagedKey.T,
                "Y" => ManagedKey.Y,
                "U" => ManagedKey.U,
                "I" => ManagedKey.I,
                "O" => ManagedKey.O,
                "P" => ManagedKey.P,
                "A" => ManagedKey.A,
                "S" => ManagedKey.S,
                "D" => ManagedKey.D,
                "F" => ManagedKey.F,
                "G" => ManagedKey.G,
                "H" => ManagedKey.H,
                "J" => ManagedKey.J,
                "K" => ManagedKey.K,
                "L" => ManagedKey.L,
                "Z" => ManagedKey.Z,
                "X" => ManagedKey.X,
                "C" => ManagedKey.C,
                "V" => ManagedKey.V,
                "B" => ManagedKey.B,
                "N" => ManagedKey.N,
                "M" => ManagedKey.M,
                "Backspace" => ManagedKey.Backspace,
                "Enter" => ManagedKey.Enter,
                _ => ManagedKey.None
            };
        }

        public static string GetDisplayName(this ManagedKey key)
        {
            return key switch
            {
                ManagedKey.Q => "Q",
                ManagedKey.W => "W",
                ManagedKey.E => "E",
                ManagedKey.R => "R",
                ManagedKey.T => "T",
                ManagedKey.Y => "Y",
                ManagedKey.U => "U",
                ManagedKey.I => "I",
                ManagedKey.O => "O",
                ManagedKey.P => "P",
                ManagedKey.A => "A",
                ManagedKey.S => "S",
                ManagedKey.D => "D",
                ManagedKey.F => "F",
                ManagedKey.G => "G",
                ManagedKey.H => "H",
                ManagedKey.J => "J",
                ManagedKey.K => "K",
                ManagedKey.L => "L",
                ManagedKey.Z => "Z",
                ManagedKey.X => "X",
                ManagedKey.C => "C",
                ManagedKey.V => "V",
                ManagedKey.B => "B",
                ManagedKey.N => "N",
                ManagedKey.M => "M",
                ManagedKey.Backspace => "Backspace",
                ManagedKey.Enter => "Enter",
                _ => ""
            };
        }
    
        public static bool IsLetter(this ManagedKey key)
        {
            return key switch
            {
                ManagedKey.Q => true,
                ManagedKey.W => true,
                ManagedKey.E => true,
                ManagedKey.R => true,
                ManagedKey.T => true,
                ManagedKey.Y => true,
                ManagedKey.U => true,
                ManagedKey.I => true,
                ManagedKey.O => true,
                ManagedKey.P => true,
                ManagedKey.A => true,
                ManagedKey.S => true,
                ManagedKey.D => true,
                ManagedKey.F => true,
                ManagedKey.G => true,
                ManagedKey.H => true,
                ManagedKey.J => true,
                ManagedKey.K => true,
                ManagedKey.L => true,
                ManagedKey.Z => true,
                ManagedKey.X => true,
                ManagedKey.C => true,
                ManagedKey.V => true,
                ManagedKey.B => true,
                ManagedKey.N => true,
                ManagedKey.M => true,
                _ => false
            };
        }
    }
}
