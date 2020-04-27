using System.Collections.Generic;

namespace Moria.Core.Utils
{
    public class StateSavingDecorator : IConsoleWrapper
    {
        private readonly IConsoleWrapper wrapper;

        public StateSavingDecorator(IConsoleWrapper wrapper)
        {
            this.wrapper = wrapper;
        }

        public void beep()
        {
            this.wrapper.beep();
        }

        public void clear()
        {
            this.wrapper.clear();
            this.GetState().reset();
        }

        public void move(int y, int x)
        {
            this.wrapper.move(y, x);
            var state = this.GetState();
            state.y = y;
            state.x = x;
        }

        public void addch(char c)
        {
            this.wrapper.addch(c);
            this.GetState().addch(c);
        }

        public void addstr(string s)
        {
            this.wrapper.addstr(s);
            this.GetState().addstr(s);
        }

        public void getyx(out int y, out int x)
        {
            this.wrapper.getyx(out y, out x);
        }

        public int WindowHeight => this.wrapper.WindowHeight;

        public int WindowWidth => this.wrapper.WindowWidth;

        private Stack<ConsoleState> savedStates = new Stack<ConsoleState>();

        public void saveTerminal()
        {
            this.savedStates.Push(this.GetState().Clone());
        }

        public void restoreTerminal()
        {
            this.currentState = this.savedStates.Pop();
            for (int i = 0; i < this.currentState.state.Length; i++)
            {
                move(i,0);
                addstr(new string(this.currentState.state[i]));
            }
            move(this.currentState.y, this.currentState.x);
        }

        private ConsoleState currentState = null;
        public ConsoleState GetState()
        {
            return currentState ??
                   (currentState = new ConsoleState(this.wrapper));
        }
    }
}