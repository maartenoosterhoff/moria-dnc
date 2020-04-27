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
            Console.SetCursorPosition(y, x);
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
            y = Console.CursorLeft;
            x = Console.CursorTop;
        }

        public int WindowHeight => Console.WindowHeight;

        public int WindowWidth => Console.WindowWidth;

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
