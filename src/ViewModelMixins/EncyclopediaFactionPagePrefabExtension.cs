using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using System.Xml;

namespace Diplomacy.ViewModelMixins
{
    [PrefabExtension("EncyclopediaFactionPage", "descendant::GridWidget[@Id='EnemiesGrid']")]
    internal sealed class EncyclopediaFactionPagePrefabExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Append;

        private readonly XmlDocument _document;

        public EncyclopediaFactionPagePrefabExtension()
        {
            _document = new XmlDocument();
            _document.LoadXml(@"<EncyclopediaFactionPageInject />");
        }

        [PrefabExtensionXmlDocument]
        public XmlDocument GetPrefabExtension() => _document;
    }

    [PrefabExtension("EncyclopediaFactionPage", "descendant::MaskedTextureWidget[@Brush='Encyclopedia.Faction.Banner']")]
    internal sealed class FactionsButtonExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Append;

        private readonly XmlDocument _document;

        public FactionsButtonExtension()
        {
            _document = new XmlDocument();
            _document.LoadXml(@"<FactionButtonInject />");
        }

        [PrefabExtensionXmlDocument]
        public XmlDocument GetPrefabExtension() => _document;
    }
}
