namespace WarnerEngine.Lib.SceneGraph
{
    public class Scene : IScene
    {
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

        private INode _root;
        public INode Root
        {
            get
            {
                return _root;
            }
        }

        public Scene(string Name, INode Root)
        {
            _name = Name;
            _root = Root;
        }
    }
}
