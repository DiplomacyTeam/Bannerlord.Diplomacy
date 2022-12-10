using Bannerlord.BUTR.Shared.Helpers;

using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

using JetBrains.Annotations;

using System.Xml;

namespace Diplomacy.ViewModelMixin
{
    [PrefabExtension("DiplomacyPanel", "descendant::Widget[@IsVisible='@Show']")]
    [UsedImplicitly]
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
                _document.LoadXml(@"<DiplomacyPanelCustom />");
            }
        }

        [PrefabExtensionXmlDocument]
        [UsedImplicitly]
        public XmlDocument GetPrefabExtension() => _document;

    }
}