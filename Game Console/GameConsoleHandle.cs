using System;
using UnityEngine;

namespace Shears.GameConsole
{
    public readonly struct GameConsoleHandle
    {
        private readonly Action<string> message;
        private readonly Action<string> error;

        public GameConsoleHandle(Action<string> message, Action<string> error)
        {
            this.message = message;
            this.error = error;
        }

        public void Message(string text) => message(text);

        [HideInCallstack]
        public void Error(string text) => error(text);
    }
}
