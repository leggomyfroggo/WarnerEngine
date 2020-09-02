using System.Xml.Serialization;

namespace WarnerEngine.Lib.Content
{
    [XmlRoot("ContentItem")]
    public struct ContentItem
    {
        public enum ContentType { Animation, Animation4, Texture, Sound, SpriteFont, Dialog, Effect, WorldGroupDefinition }

        [XmlAttribute("Type")]
        public ContentType Type;

        [XmlAttribute("Key")]
        public string Key;

        [XmlAttribute("Path")]
        public string Path;
    }
}
