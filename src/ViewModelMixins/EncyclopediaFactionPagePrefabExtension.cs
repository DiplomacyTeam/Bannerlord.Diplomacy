using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using System.Xml;

namespace Diplomacy.ViewModelMixins
{
    [PrefabExtension("EncyclopediaFactionPage", "descendant::GridWidget[@Id='EnemiesGrid']")]
    public class EncyclopediaFactionPagePrefabExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Append;

        private XmlDocument document;

        public EncyclopediaFactionPagePrefabExtension()
        {
            document = new XmlDocument();
            document.LoadXml(@"<EncyclopediaFactionPageInject />");
        }

        [PrefabExtensionXmlDocument]
        public XmlDocument GetPrefabExtension() => document;
    }

    [PrefabExtension("EncyclopediaFactionPage", "descendant::MaskedTextureWidget[@Brush='Encyclopedia.Faction.Banner']")]
    public class FactionsButtonExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Append;

        private XmlDocument document;

        public FactionsButtonExtension()
        {
            document = new XmlDocument();
            document.LoadXml(@"<FactionButtonInject />");
        }

        [PrefabExtensionXmlDocument]
        public XmlDocument GetPrefabExtension() => document;
    }
}
