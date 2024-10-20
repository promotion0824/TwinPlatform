using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Willow.AzureDigitalTwins.BackupRestore.Log
{
    public class InteractiveLogger : IInteractiveLogger
    {
        private readonly List<string> _logs;

        public InteractiveLogger()
        {
            _logs = new List<string>();
        }

        public void Log(string message, bool replaceCurrentLine = false, bool includeInOutputFile = true)
        {
            var formattedMessage = message;
            if (replaceCurrentLine)
            {
                if(_logs.Any() && includeInOutputFile)
                    _logs.RemoveAt(_logs.Count - 1);
                formattedMessage = "\r" + message;
            }

            if(includeInOutputFile)
                _logs.Add(message);

            Console.Write(formattedMessage);
        }

        public void LogLine(string message, bool tab = false)
        {
            _logs.Add(message);
            if (tab)
                message = "\t" + message;
            Console.WriteLine(message);           
        }

        public void NewLine()
        {
            Console.WriteLine();
        }

        public void SetSuccessFormat()
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void SetErrorFormat()
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void ResetFormat()
        {
            Console.ResetColor();
        }

        public async Task CreateOutputLogFile(string folder)
        {
            var writer = new StreamWriter(Path.Combine(folder, "output.log"), false, Encoding.UTF8);
            foreach (var log in _logs)
            {
                await writer.WriteLineAsync(log);
            }
            await writer.FlushAsync();
            writer.Close();
        }
    }
}
