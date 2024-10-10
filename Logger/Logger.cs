using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koboldcs.Logger
{
    public static class SLogger
    {
        public enum LogType
        {
            Main,
            Info,
            Warn,
            Error,

            LLaMA,
            CFG
        }

        public static void Log(LogType logType, string message)
        {
            switch (logType)
            {
                case LogType.Main:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[Main] ");
                    break;
                case LogType.Info:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("[Info] ");
                    break;
                case LogType.Warn:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write("[Warn] ");
                    break;
                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[Error] ");
                    break;
                case LogType.LLaMA:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("[llama.cpp] ");
                    break;
                case LogType.CFG:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write("[CFG] ");
                    break;
                default:
                    break;
            }

            Console.ResetColor();
            Console.WriteLine(message);
        }
    }
}
