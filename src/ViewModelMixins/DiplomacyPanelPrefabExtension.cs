using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using System.Xml;

namespace Diplomacy.ViewModelMixins
{
    [PrefabExtension("DiplomacyPanel", "descendant::Widget[@IsVisible='@Show']")]
    internal sealed class DiplomacyPanelPrefabExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Replace;

        // The file should have an extension of type .xml, and be located inside of the GUI folder of your module.
        // You can include or omit the extension type. I.e. both of the following would work:
        //   ExampleFileInjectedPatch
        //   ExampleFileInjectedPatch.xml

        private XmlDocument document;

        public DiplomacyPanelPrefabExtension()
        {
            document = new XmlDocument();
#if e159
            document.LoadXml(@"<DiplomacyPanel_159 />");
#elif e1510
            document.LoadXml(@"<DiplomacyPanel_1510 />");
#endif
        }

        [PrefabExtensionXmlDocument]
        public XmlDocument GetPrefabExtension() => document;

    }
}
