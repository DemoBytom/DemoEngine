namespace Demo.Engine.Windows.Models.Options
{
    public class FormSettings
    {
        /// <summary>
        /// Screen width
        /// </summary>
        public int Width { get; set; } = 1024;

        /// <summary>
        /// Screen height
        /// </summary>
        public int Height { get; set; } = 768;

        /// <summary>
        /// Should the application run in Fullscreen mode
        /// </summary>
        public bool Fullscreen { get; set; }

        /// <summary>
        /// Screen that should be used to display the application in fullscreen mode
        /// <para>0 is the primary screen</para>
        /// </summary>
        public byte Screen { get; set; } = 0;

        /// <summary>
        /// Can you resize the non-fullscreen window?
        /// </summary>
        public bool AllowResizing { get; set; }
    }
}