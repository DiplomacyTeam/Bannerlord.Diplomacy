using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

using JetBrains.Annotations;

using System.Collections.Generic;
using System.Xml;

namespace Diplomacy.ViewModelMixin
{
    [PrefabExtension("KingdomManagement", "descendant::ButtonWidget[@Id='FiefsTabButton']")]
    [UsedImplicitly]
    internal sealed class KingdomManagementPrefabExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Append;

        private readonly XmlDocument _document;

        public KingdomManagementPrefabExtension()
        {
            _document = new XmlDocument();
            _document.LoadXml(@"
                <!--Fiefs Tab-->
                <ButtonWidget DoNotPassEventsToChildren='true' WidthSizePolicy='Fixed' HeightSizePolicy='Fixed' SuggestedWidth='!Header.Tab.Center.Width.Scaled' SuggestedHeight='!Header.Tab.Center.Height.Scaled' VerticalAlignment='Center' PositionYOffset='2' Brush='Header.Tab.Center' Command.Click='ExecuteShowFactions' UpdateChildrenStates='true'>
                  <Children>
                    <TextWidget DataSource='{..}' WidthSizePolicy='CoverChildren' HeightSizePolicy='CoverChildren' HorizontalAlignment='Center' VerticalAlignment='Center' Brush='Clan.TabControl.Text' Text='@FactionsLabel' />
                  </Children>
                </ButtonWidget>");
        }

        [PrefabExtensionXmlDocument]
        [UsedImplicitly]
        public XmlDocument GetPrefabExtension() => _document;
    }

    [PrefabExtension("KingdomManagement", "descendant::Constant[@Name='Header.Tab.Center.Width.Scaled']")]
    [UsedImplicitly]
    internal sealed class KingdomManagementScalingPatch : PrefabExtensionSetAttributePatch
    {
        public override List<Attribute> Attributes => new()
        {
            new Attribute("MultiplyResult", "0.60")
        };
    }
}