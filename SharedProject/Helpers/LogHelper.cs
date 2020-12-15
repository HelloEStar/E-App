using System;
using System.Collections.Generic;
using System.Text;

namespace SharedProject
{
    public class LogHelper
    {
        public static void System(string content, bool showTag = true)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine((showTag ? "【系统】" : "") + content);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void SystemError(string content, bool showTag = true)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine((showTag ? "【系统】" : "") + content);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void Message(string content)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(content);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void Message(string sender, string content)
        {
            string str = string.Format("【{0}】{1}", sender, content);
            Message(str);
        }
    }
}
