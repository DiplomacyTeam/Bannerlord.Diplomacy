using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

using JetBrains.Annotations;

using System.Xml;

namespace Diplomacy.ViewModelMixin
{
    [PrefabExtension("EncyclopediaFactionPage", "descendant::NavigatableGridWidget[@Id='EnemiesGrid']")]
    [UsedImplicitly]
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
        [UsedImplicitly]
        public XmlDocument GetPrefabExtension() => _document;
    }

    [PrefabExtension("EncyclopediaFactionPage", "descendant::MaskedTextureWidget[@Brush='Encyclopedia.Faction.Banner']")]
    [UsedImplicitly]
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
        [UsedImplicitly]
        public XmlDocument GetPrefabExtension() => _document;
    }
}