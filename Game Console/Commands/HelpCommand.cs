using System;

namespace Shears.GameConsole
{
    public readonly struct HelpCommand : IConsoleCommand
    {
        public readonly string Command => "help";

        public readonly string Description => "Displays a list of all commands.";

        public void TryExecuteCommand(string input, Action<string> consoleMessage, Action<string> consoleError)
        {
            int cmdLength = Command.Length;

            if (input.Length > cmdLength)
            {
                consoleError($"Could not parse command '{input}'. Use 'help' to see a list of commands.");
                return;
            }

            string commands = CollectionUtil.ToCollectionString(GameConsole.Commands, (cmd) => $"{cmd.Command} - {cmd.Description}", "\n");
            consoleMessage(commands);
        }
    }
}
