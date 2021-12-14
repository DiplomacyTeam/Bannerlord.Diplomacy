﻿using System.Collections.Generic;

using Diplomacy.CivilWar.Actions;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar.Factions
{
    internal class AbdicationFaction : RebelFaction
    {
        private static readonly TextObject _TAbdicateTitle = new("{=aWWzFITw}A Monarch Abdicates");
        public AbdicationFaction(Clan sponsorClan) : base(sponsorClan) { }

        public override RebelDemandType RebelDemandType => RebelDemandType.Abdication;

        public override float LeaderInfluenceOnSuccess => 500f;

        public override float MemberInfluenceOnSuccess => 100f;

        public override float LeaderInfluenceOnFailure => -500f;

        public override float MemberInfluenceOnFailure => -100f;

        protected override void ApplyDemand()
        {
            var strVars = new Dictionary<string, object>
            {
                {"PARENT_KINGDOM", this.ParentKingdom.Name},
                {"REBELS", this.Name},
                {"LEADER", this.ParentKingdom.Leader.Name},
                {"PLAYER_PARTICIPATION", GetPlayerParticipationText(true)}
            };

            ConsolidateKingdomsAction.Apply(this);
            RebelFactionManager.DestroyRebelFaction(this);
            this.ParentKingdom.AddDecision(new KingSelectionKingdomDecision(this.SponsorClan, this.ParentKingdom.Leader.Clan), true);

            InformationManager.ShowInquiry(
                new InquiryData(
                    _TAbdicateTitle.ToString(),
                    new TextObject("{=2Pv3DsVm}{PARENT_KINGDOM} has lost its civil war to the {REBELS}. {LEADER} will abdicate the throne. {PLAYER_PARTICIPATION}", strVars).ToString(),
                    true,
                    false,
                    GameTexts.FindText("str_ok").ToString(),
                    null,
                    null,
                    null), true);
        }

        public void DestroyFactionBecauseDemandSatisfied()
        {
            RebelFactionManager.DestroyRebelFaction(this);

            var strVars = new Dictionary<string, object> { { "PARENT_KINGDOM", this.ParentKingdom.Name }, { "REBELS", this.Name }};
            InformationManager.ShowInquiry(
                new InquiryData(
                    _TAbdicateTitle.ToString(),
                    new TextObject("{=GKSaZESx}With the election of a new monarch and their objective complete, the {REBELS} of {PARENT_KINGDOM} disbands.", strVars).ToString(),
                    true,
                    false,
                    GameTexts.FindText("str_ok").ToString(),
                    null,
                    null,
                    null), true);
        }
    }
}
