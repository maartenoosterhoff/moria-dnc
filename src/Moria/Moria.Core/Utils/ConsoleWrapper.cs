using System;

namespace Moria.Core.Utils
{
    public class ConsoleWrapper : IConsoleWrapper
    {
        public void beep()
        {
            Console.Beep();
        }

        public void clear() => Console.Clear();

        public void move(int y, int x)
        {
            Console.SetCursorPosition(x, y);
        }

        public void addch(char c)
        {
            Console.Write(c);
        }

        public void addstr(string s)
        {
            Console.Write(s);
        }

        public void getyx(out int y, out int x)
        {
            y = Console.CursorTop;
            x = Console.CursorLeft;
        }

        public int WindowHeight => 24; //Console.WindowHeight;

        public int WindowWidth => 80; //Console.WindowWidth - 1;

        public void saveTerminal()
        {
            throw new NotImplementedException();
        }

        public void restoreTerminal()
        {
            throw new NotImplementedException();
        }
    }
}
