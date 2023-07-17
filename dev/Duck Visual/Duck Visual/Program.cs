using System;

namespace Duck_Visual
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Console.WriteLine("Start");
            using (var game = new Game1())
                game.Run();

            Console.WriteLine("End");
            Console.Read();
        }
    }
#endif
}
