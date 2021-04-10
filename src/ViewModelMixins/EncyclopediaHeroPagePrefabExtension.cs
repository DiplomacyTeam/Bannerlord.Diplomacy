using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using System.Xml;

namespace Diplomacy.ViewModelMixins
{
    [PrefabExtension("EncyclopediaHeroPage", "descendant::RichTextWidget[@Text='@InformationText']")]
    public class EncyclopediaHeroPagePrefabExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Append;

        private XmlDocument document;

        public EncyclopediaHeroPagePrefabExtension()
        {
            document = new XmlDocument();
            document.LoadXml(@"<EncyclopediaHeroPageInject />");
        }

        [PrefabExtensionXmlDocument]
        public XmlDocument GetPrefabExtension() => document;
    }
}
