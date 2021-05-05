using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar.Factions
{
    public class AbdicationFaction : RebelFaction
    {
        public AbdicationFaction(Clan sponsorClan) : base(sponsorClan) { }

        public override RebelDemandType RebelDemandType => RebelDemandType.Abdication;

        public override void EnforceDemand()
        {
            ConsolidateKingdomsAction.Apply(this);
            this.ParentKingdom.AddDecision(new KingSelectionKingdomDecision(this.SponsorClan, this.ParentKingdom.Leader.Clan), true);
            RebelFactionManager.DestroyRebelFaction(this);

            var strVars = new Dictionary<string, object> { { "PARENT_KINGDOM", this.ParentKingdom.Name }, { "REBELS", this.Name }, { "LEADER", this.ParentKingdom.Leader.Name } };

            InformationManager.ShowInquiry(
                new InquiryData(
                    new TextObject("{=aWWzFITw}A Monarch Abdicates").ToString(),
                    new TextObject("{=2Pv3DsVm}{PARENT_KINGDOM} has lost its civil war to the {REBELS}. {LEADER} will abdicate the throne.", strVars).ToString(),
                    true,
                    false,
                    GameTexts.FindText("str_ok", null).ToString(),
                    null,
                    null,
                    null), true);
        }
    }
}
