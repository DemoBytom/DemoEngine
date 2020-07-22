namespace Demo.Engine.Platform.DirectX.Interfaces
{
    internal interface IUpdatable : IBindable
    {
    }

    internal interface IUpdatable<T> : IUpdatable
    {
        public void Update(in T data);
    }
}