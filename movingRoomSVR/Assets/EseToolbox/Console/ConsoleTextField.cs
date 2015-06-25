using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ese.Events;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Ese
{
    /// <summary>
    /// The textfield in the console
    /// </summary>
    public class ConsoleTextField
    {
        private readonly Console console;
        private readonly float textFieldHeight;
        private const string ConsoleTextFieldName = "ConsoleTextField";
        private const string CommandContent = "> {0}";
        private readonly List<string> commands = new List<string>();
        private int currentCommand = 0;
        private string consoleText = string.Empty;
        private bool shouldUpdateCaret = false;

        private string currentAutoCompleteString = string.Empty;
        private int currentAutoCompleteIndex = 0;

        /// <summary>
        /// Default constructor for the Console Text field.
        /// </summary>
        /// <param name="console">The console</param>
        /// <param name="textFieldHeight">The height the textfield should take</param>
        public ConsoleTextField(Console console, float textFieldHeight)
        {
            this.console = console;
            this.textFieldHeight = textFieldHeight;
        }

        private void AutoComplete()
        {
            Dictionary<ConsoleCommand, string> consoleCommands = FindAllConsoleCommands();

            if (!consoleCommands.IsNullOrEmpty())
            {
                FindConsoleCommand(consoleCommands.Values.ToList());
            }
        }

        private Dictionary<ConsoleCommand, string> FindAllConsoleCommands()
        {
            Dictionary<ConsoleCommand, string> consoleCommands = new Dictionary<ConsoleCommand, string>();
            foreach (ConsoleCommand command in console.ConsoleCommands.Values)
            {
                string commandText = command.GetCommands().FirstOrDefault(text => text.ToLower().StartsWith(currentAutoCompleteString.ToLower()));
                if (!commandText.IsNullOrEmpty() && !consoleCommands.ContainsKey(command))
                {
                    consoleCommands.Add(command, commandText);
                }
            }
            return consoleCommands;
        }

        private void FindConsoleCommand(List<string> consoleCommands)
        {
            string possibleConsoleText = consoleCommands[currentAutoCompleteIndex];

            if (!possibleConsoleText.ToLower().Equals(consoleText.ToLower()))
            {
                consoleText = possibleConsoleText;
                shouldUpdateCaret = true;
            }
            else
            {
                IncrementCurrentAutoCompleteIndex(consoleCommands);
            }
        }

        private void IncrementCurrentAutoCompleteIndex(List<string> consoleCommands)
        {
            currentAutoCompleteIndex++;
            if (currentAutoCompleteIndex >= consoleCommands.Count)
            {
                currentAutoCompleteIndex = 0;
            }
        }

        private void ProcessConsoleCommand()
        {
            ConsoleContext context = new ConsoleContext(consoleText);
            if (console.ConsoleCommands.ContainsKey(context.Command.ToLower()))
            {
                CommandExist(context);
            }
            else
            {
                CommandFailed();
            }
            commands.Add(consoleText);
            currentCommand = commands.Count;
            ClearText();
        }

        private void CommandFailed()
        {
            console.AddLogEventToShow(new LogEvent(consoleText, null));
        }

        private void CommandExist(ConsoleContext context)
        {
            console.AddLogEventToShow(new SuccessEvent(consoleText, null));
            console.ConsoleCommands[context.Command].OnCommand(context);
        }

        private void MoveConsoleCommandUp()
        {
            if (commands.Count > 0)
            {
                currentCommand--;
                if (currentCommand < 0)
                {
                    currentCommand = (commands.Count - 1);
                }
                SetText(commands[currentCommand]);
            }
        }

        private void MoveConsoleCommandDown()
        {
            if (commands.Count > 0)
            {
                currentCommand++;
                if (currentCommand >= commands.Count)
                {
                    currentCommand = 0;
                }

                SetText(commands[currentCommand]);
            }
        }

        /// <summary>
        /// This is run from the OnGUI in the console script.
        /// </summary>
        public void OnGUI()
        {
            Rect consoleTextRectangle = new Rect(0, console.BottomPosition - textFieldHeight, Screen.width, textFieldHeight);
            GUI.SetNextControlName(ConsoleTextFieldName);
            SetText(GUI.TextField(consoleTextRectangle, consoleText));
            GUI.FocusControl(ConsoleTextFieldName);

            Event currentEvent = Event.current;
            if (currentEvent.isKey && currentEvent.type == UnityEngine.EventType.KeyUp)
            {
                KeyCode key = currentEvent.keyCode;

                
                if (key == KeyCode.Return || key == KeyCode.KeypadEnter || currentEvent.character == '\r' || currentEvent.character == '\n')
                {
                    ProcessConsoleCommand();
                }
                else if (key == KeyCode.Tab)
                {
                    AutoComplete();
                }
                else if (key == KeyCode.UpArrow || key == KeyCode.PageUp)
                {
                    MoveConsoleCommandUp();
                }
                else if (key == KeyCode.DownArrow || key == KeyCode.PageDown)
                {
                    MoveConsoleCommandDown();
                }
                
            }
            //We dont want the player to move.
            currentEvent.Use();
            Input.Reset();

        }

        /// <summary>
        /// Sets the console text
        /// </summary>
        /// <param name="text">The text</param>
        private void SetText(string text)
        {
            bool update = text != consoleText && !consoleText.IsNullOrEmpty();

            consoleText = text;

            if(update)
            {
                UpdateTextField(text);
            }

            if (shouldUpdateCaret)
            {
                SetCaretAtPosition(new Vector2(5555, 5555));
            }
        }

        private void UpdateTextField(string text)
        {
            currentAutoCompleteIndex = 0;
            currentAutoCompleteString = text;
            
        }

        private void SetCaretAtPosition(Vector2 position)
        {
            object something = GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            if (something != null)
            {
                TextEditor textField = something as TextEditor;
                if (textField != null)
                {
                    textField.MoveCursorToPosition(position);
                }
            }
            shouldUpdateCaret = false;
        }

        /// <summary>
        /// Clears the console text
        /// </summary>
        public void ClearText()
        {
            consoleText = string.Empty;
        }
    }
}
