using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using System.Xml;

namespace Diplomacy.ViewModelMixins
{
    [PrefabExtension("EncyclopediaHeroPage", "descendant::RichTextWidget[@Text='@InformationText']")]
    internal sealed class EncyclopediaHeroPagePrefabExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Append;

        private readonly XmlDocument _document;

        public EncyclopediaHeroPagePrefabExtension()
        {
            _document = new XmlDocument();
            _document.LoadXml(@"<EncyclopediaHeroPageInject />");
        }

        [PrefabExtensionXmlDocument]
        public XmlDocument GetPrefabExtension() => _document;
    }
}
