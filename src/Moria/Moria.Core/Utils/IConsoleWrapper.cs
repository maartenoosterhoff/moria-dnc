namespace Moria.Core.Utils
{
    public interface IConsoleWrapper
    {
        void beep();
        void clear();
        void move(int y, int x);
        void addch(char c);
        void addstr(string s);
        void getyx(out int y, out int x);

        int WindowHeight { get; }
        int WindowWidth { get; }

        void saveTerminal();
        void restoreTerminal();
    }
}