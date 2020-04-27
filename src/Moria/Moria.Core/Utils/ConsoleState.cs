using System.Linq;

namespace Moria.Core.Utils
{
    public class ConsoleState
    {
        public int x { get; set; }
        public int y { get; set; }

        public char[][] state { get; private set; }

        private readonly IConsoleWrapper console;

        public ConsoleState(IConsoleWrapper console)
        {
            this.console = console;
            this.reset();
        }

        public ConsoleState Clone()
        {
            return new ConsoleState(console)
            {
                x = this.x,
                y = this.y,
                state = this.state
                    .Select(x => x.ToArray())
                    .ToArray()
            };
        }

        public void reset()
        {
            this.state = Enumerable.Range(0, console.WindowHeight).Select(_ => new string(' ', console.WindowWidth).ToArray()).ToArray();
            console.getyx(out var y, out var x);
            this.x = x;
            this.y = y;
        }

        private void forwardCursor()
        {
            this.x++;
            if (this.x >= this.console.WindowWidth)
            {
                this.x = 0;
                this.y++;
            }
        }

        public void addch(char c)
        {
            this.state[this.y][this.x] = c;
            forwardCursor();
        }

        public void addstr(string s)
        {
            foreach (var c in s)
            {
                addch(c);
            }
        }
    }
}