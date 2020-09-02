using System.Xml.Serialization;

using WarnerEngine.Services;

namespace WarnerEngine.Lib.Dialog
{
    [XmlRoot("DialogChain")]
    public class DialogChain
    {
        [XmlElement("Dialog")]
        public string Dialog;
        [XmlElement("DialogKey")]
        public string DialogKey;
        [XmlElement("NextDialogKey")]
        public string NextDialogKey;
        public DialogChain NextDialog
        {
            get
            {
                if (NextDialogKey == null)
                {
                    return null;
                }
                return GameService.GetService<IContentService>().GetDialog(NextDialogKey);
            }
        }

        public DialogChain() { }

        public DialogChain(string DialogKey, string Dialog, string NextDialogKey)
        {
            this.DialogKey = DialogKey;
            this.Dialog = Dialog;
            this.NextDialogKey = NextDialogKey;
        }
    }
}
