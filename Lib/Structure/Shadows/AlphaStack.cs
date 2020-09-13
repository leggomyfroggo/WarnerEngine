using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WarnerEngine.Lib.Structure.Shadows
{
    public class AlphaStack
    {
        protected Stack<AlphaFragment> fragments;
        public int TotalFragments 
        { 
            get 
            {
                return fragments.Count;
            } 
        }

        public AlphaStack()
        {
            fragments = new Stack<AlphaFragment>(500);
        }

        public void ClearStack()
        {
            fragments.Clear();
        }

        public void PushFragment(Texture2D Texture, Rectangle DestinationRectangle, Rectangle SourceRectangle, float Opacity, float Depth, float Rotation = 0f, Vector2? Origin = null, Color? Tint = null, bool IsTiling = false)
        {
            fragments.Push(new AlphaFragment() {
                Texture = Texture,
                DestinationRectangle = DestinationRectangle,
                SourceRectangle = SourceRectangle,
                Opacity = Opacity,
                Depth = Depth,
                Rotation = Rotation,
                Origin = Origin.HasValue ? Origin.Value : Vector2.Zero,
                Tint = Tint.HasValue ? Tint.Value : Color.White,
                IsTiling = IsTiling,
            });
        }

        public void Draw(Color? Tint = null)
        {
            foreach (AlphaFragment fragment in fragments)
            {
                fragment.Draw(Tint);
            }
        }
    }
}
