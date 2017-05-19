using System;
using console = System.Console;

namespace Samples.Console
{
    public class Output
    {
        public static object ColorLock = new Object();

        public static void Message(string line)
        {
            lock (ColorLock)
            {
                console.ForegroundColor = ConsoleColor.White;
                console.WriteLine(line);
                console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public static string Question(string quesiton)
        {
            lock (ColorLock)
            {
                console.ForegroundColor = ConsoleColor.Green;
                console.WriteLine(quesiton);
                console.ForegroundColor = ConsoleColor.Gray;
            }
            return console.ReadLine();
        }

        public static void Log(string line)
        {
            lock (ColorLock)
            {
                console.ForegroundColor = ConsoleColor.DarkGray;
                console.WriteLine("\t\t\t\t" + line);
                console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public static void Error(string line)
        {
            lock (ColorLock)
            {
                console.ForegroundColor = ConsoleColor.Red;
                console.WriteLine(line);
                console.ForegroundColor = ConsoleColor.Gray;
            }
        }   
    }
}
