using System.Xml.Serialization;

using Microsoft.Xna.Framework;

namespace ProjectWarnerShared.Lib.Dialog
{
    [XmlRoot("DialogSegment")]
    public class DialogSegment
    {
        public enum SegmentColors { White, Red, Blue, Green, Yellow, Black, Gray }
        [XmlText]
        public string Text;
        [XmlAttribute("Color")]
        public SegmentColors TColor;

        public DialogSegment()
        {
            TColor = SegmentColors.Black;
        }

        public DialogSegment(string Text, SegmentColors TColor)
        {
            this.Text = Text;
            this.TColor = TColor;
        }

        public Color GetRealColor()
        {
            switch (TColor)
            {
                case SegmentColors.White:
                    return Color.White;
                case SegmentColors.Red:
                    return Color.Red;
                case SegmentColors.Blue:
                    return Color.Blue;
                case SegmentColors.Green:
                    return Color.LimeGreen;
                case SegmentColors.Yellow:
                    return Color.Gold;
                case SegmentColors.Black:
                    return Color.Black;
                case SegmentColors.Gray:
                    return Color.Gray;
                default:
                    throw new System.Exception("Unsupported color " + TColor);
            }
        }
    }
}
