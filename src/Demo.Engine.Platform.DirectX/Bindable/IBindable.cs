namespace Demo.Engine.Platform.DirectX.Bindable
{
    /// <summary>
    /// An object that can be bound to the rendering pipeline
    /// </summary>
    internal interface IBindable
    {
        /// <summary>
        /// Method used to bind to the rendering pipeline
        /// </summary>
        void Bind();
    }
}