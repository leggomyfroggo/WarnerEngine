namespace WarnerEngine.Lib.SceneGraph
{
    public interface IScene
    {
        string Name { get; }
        INode Root { get; }
    }
}
