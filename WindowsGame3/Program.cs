using System;

namespace WindowsGame3
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (MainProgram game = new MainProgram())
            {
                game.Run();
            }
        }
    }
#endif
}

