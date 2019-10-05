using System;
using Demo.Engine.Windows.Platform.Netstandard.Win32;

namespace Demo.Engine.Windows
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static int Main(string[] args)
        {
            //Application.SetHighDpiMode(HighDpiMode.SystemAware);
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            using var engine = new Engine(new RenderingForm());
            engine.Run();

            return 0;
        }
    }
}