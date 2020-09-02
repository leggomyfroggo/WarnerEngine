using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectWarnerShared.Services;

namespace ProjectWarnerShared.Lib.Structure.Shadows
{
    public class AlphaStack
    {
        protected AlphaFragment[] fragments;
        public int TotalFragments { get; private set; }

        public AlphaStack(int StackSize)
        {
            fragments = new AlphaFragment[StackSize];
            TotalFragments = 0;
        }

        public void ClearStack()
        {
            TotalFragments = 0;
        }

        public void PushFragment(Texture2D Texture, Rectangle DestinationRectangle, Rectangle SourceRectangle, float Opacity, float Depth, float Rotation = 0f, Vector2? Origin = null, Color? Tint = null, bool IsTiling = false)
        {
            if (fragments.Length == TotalFragments)
            {
                return;
            }
            fragments[TotalFragments].Texture = Texture;
            fragments[TotalFragments].DestinationRectangle = DestinationRectangle;
            fragments[TotalFragments].SourceRectangle = SourceRectangle;
            fragments[TotalFragments].Opacity = Opacity;
            fragments[TotalFragments].Depth = Depth;
            fragments[TotalFragments].Rotation = Rotation;
            fragments[TotalFragments].Origin = Origin.HasValue ? Origin.Value : Vector2.Zero;
            fragments[TotalFragments].Tint = Tint.HasValue ? Tint.Value : Color.White;
            fragments[TotalFragments].IsTiling = IsTiling;
            TotalFragments++;
        }

        public void Draw(Color? Tint = null)
        {
            for (int i = 0; i < TotalFragments; i++)
            {
                fragments[i].Draw(Tint);
            }
        }
    }
}
