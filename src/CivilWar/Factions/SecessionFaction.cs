using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar.Factions
{
    public class SecessionFaction : RebelFaction
    {
        public SecessionFaction(Clan sponsorClan) : base(sponsorClan) { }

        public override RebelDemandType RebelDemandType => RebelDemandType.Secession;

        public override void EnforceDemand()
        {
            var kingdomName = KingdomNameGenerator.GenerateKingdomName(this);
            Kingdom newKingdom = this.RebelKingdom!;
            newKingdom.ChangeKingdomName(kingdomName, kingdomName);
            ChangeKingdomBannerAction.Apply(newKingdom, false);
            RebelFactionManager.DestroyRebelFaction(this, true);
            var strVars = new Dictionary<string, object> { { "PARENT_KINGDOM", this.ParentKingdom.Name }, { "KINGDOM", newKingdom.Name } };

            InformationManager.ShowInquiry(
                new InquiryData(
                    new TextObject("{=x5ThZR5A}A New Kingdom Emerges").ToString(),
                    new TextObject("{=nRxyMIEE}{PARENT_KINGDOM} has lost their civil war and failed to keep their kingdom united. The {KINGDOM} has emerged.", strVars).ToString(),
                    true,
                    false,
                    GameTexts.FindText("str_ok", null).ToString(),
                    null,
                    null,
                    null), true);
        }
    }
}
