using System.Xml.Serialization;

using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Services;

namespace WarnerEngine.Lib.Dialog
{
    [XmlRoot("DialogLink")]
    public class DialogLink
    {
        [XmlAttribute("Key")]
        public string Key;
        [XmlAttribute("NextKey")]
        public string NextKey;
        [XmlAttribute("CharacterName")]
        public string CharacterName;
        [XmlArray("Segments")]
        public DialogSegment[] Segments;

        public DialogLink NextDialog
        {
            get
            {
                if (NextKey == null)
                {
                    return null;
                }
                return GameService.GetService<IContentService>().GetDialogLink(NextKey);
            }
        }

        private int length;
        public int Length
        {
            get
            {
                if (length == 0)
                {
                    foreach (DialogSegment segment in Segments)
                    {
                        length += segment.Text.Length;
                    }
                }
                return length;
            }
        }

        private string fullText;
        public string FullText
        {
            get
            {
                if (fullText == null)
                {
                    fullText = "";
                    foreach (DialogSegment segment in Segments)
                    {
                        fullText += segment.Text;
                    }
                }
                return fullText;
            }
        }

        public DialogLink() { }

        public DialogLink(string Key, string NextKey, DialogSegment[] Segments)
        {
            this.Key = Key;
            this.NextKey = NextKey;
            this.Segments = Segments;
        }

        public float GetDrawnWidth(string FontKey)
        {
            SpriteFont font = GameService.GetService<IContentService>().GetSpriteFont(FontKey);
            return font.MeasureString(FullText).X;
        }

        public static DialogLink ExportTestChain()
        {
            DialogSegment s1 = new DialogSegment("This is some ", DialogSegment.SegmentColors.White);
            DialogSegment s2 = new DialogSegment("text!", DialogSegment.SegmentColors.Blue);

            DialogSegment s3 = new DialogSegment("I think ", DialogSegment.SegmentColors.White);
            DialogSegment s4 = new DialogSegment("bugs", DialogSegment.SegmentColors.Green);
            DialogSegment s5 = new DialogSegment(" are rad.", DialogSegment.SegmentColors.White);

            DialogLink l1 = new DialogLink("link_1", "link_2", new DialogSegment[] { s1, s2 });
            DialogLink l2 = new DialogLink("link_2", null, new DialogSegment[] { s3, s4, s5 });

            DialogLink[] links = new DialogLink[] { l1, l2 };

            return l2;
        }
    }
}
