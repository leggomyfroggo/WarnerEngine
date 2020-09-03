using System.Xml.Serialization;

namespace WarnerEngine.Lib.Content
{
    [XmlRoot("ContentItem")]
    public struct ContentItem
    {
        [XmlAttribute("Type")]
        public string Type;

        [XmlAttribute("Key")]
        public string Key;

        [XmlAttribute("Path")]
        public string Path;
    }
}
