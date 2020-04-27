namespace Moria.App
{
    class Program
    {
        static void Main(string[] args)
        {
            Core.Methods.Main_m.main(args);
            /*
            var input = '\0';
            Console.TreatControlCAsInput = true;
            var waiting = 0;
            var buffer = string.Empty;
            while (input != 's')
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    input = key.KeyChar;
                    buffer += input;
                    Console.SetCursorPosition(0, 0);
                    Console.Write("Pressed key: {0}", input);
                    Console.SetCursorPosition(0, 1);
                    Console.Write("Pressed so far: {0}", buffer);
                    waiting = 0;
                }
                else
                {
                    Console.SetCursorPosition(0, 2);
                    Console.Write("Waiting for input{0}", new string('.', waiting));
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    waiting++;
                }

                Console.SetCursorPosition(0, 0);
            }
            */
        }
    }
}
