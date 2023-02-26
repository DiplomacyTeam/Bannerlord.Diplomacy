using Diplomacy.CivilWar.Actions;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar.Factions
{
    internal class SecessionFaction : RebelFaction
    {
        public SecessionFaction(Clan sponsorClan) : base(sponsorClan) { }

        public override RebelDemandType RebelDemandType => RebelDemandType.Secession;

        public override float LeaderInfluenceOnSuccess => 500f;

        public override float MemberInfluenceOnSuccess => 100f;

        public override float LeaderInfluenceOnFailure => -500f;

        public override float MemberInfluenceOnFailure => -100f;

        public override bool ConsolidateOnSuccess => false;

        protected override void ApplyDemand()
        {
            var kingdomName = FactionNameGenerator.GenerateKingdomName(this);
            var newKingdom = RebelKingdom!;
            newKingdom.ChangeKingdomName(kingdomName, kingdomName);

            var strVars = new Dictionary<string, object>
            {
                {"PARENT_KINGDOM", ParentKingdom.Name},
                {"KINGDOM", newKingdom.Name},
                {"PLAYER_PARTICIPATION", GetPlayerParticipationText(true)}
            };

            ChangeKingdomBannerAction.Apply(newKingdom);
            RebelFactionManager.DestroyRebelFaction(this, true);

            InformationManager.ShowInquiry(
                new InquiryData(
                    new TextObject("{=x5ThZR5A}A New Kingdom Emerges").ToString(),
                    new TextObject("{=nRxyMIEE}{PARENT_KINGDOM} has lost their civil war and failed to keep their kingdom united. The {KINGDOM} has emerged. {PLAYER_PARTICIPATION}", strVars).ToString(),
                    true,
                    false,
                    GameTexts.FindText("str_ok").ToString(),
                    null,
                    null,
                    null), true);
        }
    }
}