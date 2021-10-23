namespace WarnerEngine.Lib.Graphics
{
    public class DrawStack
    {
        private DrawCall[] _drawCalls;
        private int _drawCallCount;

        public string ID { get; }
        
        public DrawStack(string ID, int Size = 100)
        {
            this.ID = ID;
            _drawCalls = new DrawCall[Size];
            _drawCallCount = 0;
        }

        public DrawStack PushDrawCall(DrawCall Call)
        {
            _drawCalls[_drawCallCount++] = Call;
            return this;
        }

        public DrawStack Flush()
        {
            Draw();
            _drawCallCount = 0;
            return this;
        }

        public DrawStack Draw()
        {
            for (int i = 0; i < _drawCallCount; i++)
            {
                _drawCalls[i].Draw();
            }
            return this;
        }

        public DrawStack Clear()
        {
            _drawCallCount = 0;
            return this;
        }
    }
}
