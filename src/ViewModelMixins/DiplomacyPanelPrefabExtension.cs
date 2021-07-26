using Bannerlord.BUTR.Shared.Helpers;
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

        private readonly XmlDocument _document;

        public DiplomacyPanelPrefabExtension()
        {
            _document = new XmlDocument();

            if (ApplicationVersionHelper.GameVersion() is { } gameVersion)
            {
                if (gameVersion.Major <= 1 && gameVersion.Minor <= 5 && gameVersion.Revision <= 9)
                    _document.LoadXml(@"<DiplomacyPanel_159 />");
                else
                    _document.LoadXml(@"<DiplomacyPanel_1510 />");
            }
        }

        [PrefabExtensionXmlDocument]
        public XmlDocument GetPrefabExtension() => _document;

    }
}
