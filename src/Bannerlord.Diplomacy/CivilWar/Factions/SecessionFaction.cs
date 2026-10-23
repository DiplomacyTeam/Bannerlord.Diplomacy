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
            var newKingdom = RebelKingdom!;
            RenameKingdom(newKingdom, FactionNameGenerator.GenerateKingdomName(this));

            var playerParticipation = GetPlayerParticipationText(true);

            ChangeKingdomBannerAction.Apply(newKingdom);
            RebelFactionManager.DestroyRebelFaction(this, true);

            if (newKingdom.RulingClan == Clan.PlayerClan)
            {
                FactionNameInquiry.Show(
                    new TextObject("{=FNkdmTtl}Name Your Kingdom"),
                    new TextObject("{=FNkdmTxt}Your lands have broken away from {PARENT_KINGDOM}. Choose a name for your new kingdom.")
                        .SetTextVariable("PARENT_KINGDOM", ParentKingdom.Name),
                    newKingdom.Name,
                    name =>
                    {
                        RenameKingdom(newKingdom, name);
                        ShowNewKingdomInquiry(newKingdom, playerParticipation);
                    },
                    canCancel: false);
            }
            else
            {
                ShowNewKingdomInquiry(newKingdom, playerParticipation);
            }
        }

        private static void RenameKingdom(Kingdom kingdom, TextObject name)
        {
#if BL15
            kingdom.ChangeKingdomName(name, name, name);
#else
            kingdom.ChangeKingdomName(name, name);
#endif
        }

        private void ShowNewKingdomInquiry(Kingdom newKingdom, TextObject playerParticipation)
        {
            var strVars = new Dictionary<string, object>
            {
                {"PARENT_KINGDOM", ParentKingdom.Name},
                {"KINGDOM", newKingdom.Name},
                {"PLAYER_PARTICIPATION", playerParticipation}
            };

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