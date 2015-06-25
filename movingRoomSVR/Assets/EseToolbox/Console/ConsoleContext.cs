
namespace Ese
{
    /// <summary>
    /// A console command
    /// </summary>
    public class ConsoleContext
    {
        /// <summary>
        /// The console command
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// The parameter to be passed in with the command.
        /// </summary>
        public string Parameter { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="text">The text to be parsed from the console.</param>
        public ConsoleContext(string text)
        {
            ParseText(text);
        }

        private void ParseText(string text)
        {
            string[] words = text.Split(' ');
            int length = words.Length;
            if(length > 0)
            {
                Command = words[0].ToLower();
                if (length > 1)
                {
                    int commandLenght = Command.Length + 1;
                    Parameter = text.Substring(commandLenght, text.Length - commandLenght);
                }
            }
        }
    }
}
