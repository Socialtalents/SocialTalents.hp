using System;
using console = System.Console;

namespace Samples.Console
{
    public class Output
    {
        public static object ColorLock = new Object();

        public static void Message(string line, string sender = null)
        {
            lock (ColorLock)
            {
                console.ForegroundColor = ConsoleColor.White;
                console.Write(sender == null ? string.Empty : sender + ": ");
                console.WriteLine(line);
                console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public static void Success(string quesiton, string sender = null)
        {
            lock (ColorLock)
            {
                console.ForegroundColor = ConsoleColor.Green;
                console.Write(sender == null ? string.Empty : sender + ": ");
                console.WriteLine(quesiton);
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

        public static void Log(string line, string sender = null)
        {
            lock (ColorLock)
            {
                console.ForegroundColor = ConsoleColor.DarkGray;
                console.Write(sender == null ? string.Empty : sender + ": ");
                console.WriteLine("\t\t\t" + line);
                console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public static void Error(string line, string sender = null)
        {
            lock (ColorLock)
            {
                console.ForegroundColor = ConsoleColor.Red;
                console.Write(sender == null ? string.Empty : sender + ": ");
                console.WriteLine(line);
                console.ForegroundColor = ConsoleColor.Gray;
            }
        }   
    }
}
