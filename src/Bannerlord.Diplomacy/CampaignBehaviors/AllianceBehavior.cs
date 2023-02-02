using Diplomacy.DiplomaticAction.Alliance;
using Diplomacy.DiplomaticAction.WarPeace;
using Diplomacy.Events;
using Diplomacy.Extensions;

using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.CampaignBehaviors
{
    internal sealed class AllianceBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, DailyTickClan);
#if v100 || v101 || v102 || v103
            DiplomacyEvents.WarDeclared.AddNonSerializedListener(this, WarDeclared);
#else
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, WarDeclared);
#endif
            DiplomacyEvents.AllianceFormed.AddNonSerializedListener(this, AllianceFormed);
        }

        public override void SyncData(IDataStore dataStore) { }

        private void AllianceFormed(AllianceEvent allianceFormedEvent)
        {
            var txt = new TextObject("{=PdN5g5ub}{KINGDOM} has formed an alliance with {OTHER_KINGDOM}!");
            txt.SetTextVariable("KINGDOM", allianceFormedEvent.Kingdom.Name);
            txt.SetTextVariable("OTHER_KINGDOM", allianceFormedEvent.OtherKingdom.Name);
            var txtRendered = txt.ToString();

            if (allianceFormedEvent.Kingdom == Clan.PlayerClan.Kingdom
                || allianceFormedEvent.OtherKingdom == Clan.PlayerClan.Kingdom)
            {
                InformationManager.ShowInquiry(
                    new InquiryData(new TextObject("{=qIa19an4}Alliance Formed").ToString(),
                                    txtRendered,
                                    true,
                                    false,
                                    GameTexts.FindText("str_ok").ToString(),
                                    null,
                                    null,
                                    null));
            }
            else
                InformationManager.DisplayMessage(new InformationMessage(txtRendered, SubModule.StdTextColor));
        }

#if v100 || v101 || v102 || v103
        private void WarDeclared(WarDeclaredEvent warDeclaredEvent)
        {
            if (!warDeclaredEvent.IsProvoked
                && warDeclaredEvent.Faction is Kingdom attacker
                && warDeclaredEvent.ProvocatorFaction is Kingdom defender)
            {
                SupportAlliedKingdom(defender, attacker);
            }
        }
#else
        private void WarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail declareWarDetail)
        {
            if (declareWarDetail != DeclareWarAction.DeclareWarDetail.CausedByPlayerHostility
                && faction1 is Kingdom attacker
                && faction2 is Kingdom defender)
            {
                SupportAlliedKingdom(defender, attacker);
            }
        }
#endif
        private void SupportAlliedKingdom(Kingdom kingdom, Kingdom kingdomToDeclareWarOn)
        {
            var allies = KingdomExtensions.AllActiveKingdoms.Where(k => kingdom != k && FactionManager.IsAlliedWithFaction(kingdom, k));

            foreach (var ally in allies)
            {
                if (!DeclareWarConditions.Instance.CanApply(ally, kingdomToDeclareWarOn, bypassCosts: true))
                    continue;

#if v100 || v101 || v102 || v103
                DeclareWarAction.ApplyDeclareWarOverProvocation(ally, kingdomToDeclareWarOn);
#else
                DeclareWarAction.ApplyByKingdomDecision(ally, kingdomToDeclareWarOn);
#endif
                var txt = new TextObject("{=UDC8eW7s}{ALLIED_KINGDOM} is joining their ally, {KINGDOM}, in the war against {ENEMY_KINGDOM}.");
                txt.SetTextVariable("ALLIED_KINGDOM", ally.Name);
                txt.SetTextVariable("KINGDOM", kingdom.Name);
                txt.SetTextVariable("ENEMY_KINGDOM", kingdomToDeclareWarOn.Name);

                InformationManager.DisplayMessage(new InformationMessage(txt.ToString(), SubModule.StdTextColor));
            }
        }

        private void DailyTickClan(Clan clan)
        {
            if (!Settings.Instance!.EnableAlliances)
            {
                BreakAllAlliances(clan);
                return;
            }

            if (clan.Leader == clan.Kingdom?.Leader && clan.Leader != Hero.MainHero && clan.MapFaction.IsKingdomFaction)
            {
                var kingdom = clan.Kingdom;
                ConsiderBreakingAlliances(kingdom);
                ConsiderFormingAlliances(kingdom);
            }
        }

        private static void ConsiderFormingAlliances(Kingdom kingdom)
        {
            var potentialAllies = KingdomExtensions.AllActiveKingdoms
                .Where(k => k != kingdom && FormAllianceConditions.Instance.CanApply(kingdom, k))
                .ToList();

            foreach (var potentialAlly in potentialAllies)
                if (MBRandom.RandomFloat < 0.05f && AllianceScoringModel.Instance.ShouldFormBidirectional(kingdom, potentialAlly))
                    DeclareAllianceAction.Apply(kingdom, potentialAlly);
        }

        private static void ConsiderBreakingAlliances(Kingdom kingdom)
        {
            var alliedKingdoms = KingdomExtensions.AllActiveKingdoms
                .Where(k => k != kingdom && FactionManager.IsAlliedWithFaction(kingdom, k))
                .ToList();

            foreach (var alliedKingdom in alliedKingdoms)
            {
                if (MBRandom.RandomFloat < 0.05f
                    && BreakAllianceConditions.Instance.CanApply(kingdom, alliedKingdom)
                    && !AllianceScoringModel.Instance.ShouldForm(kingdom, alliedKingdom))
                {
                    BreakAllianceAction.Apply(kingdom, alliedKingdom);
                }
            }
        }

        private static void BreakAllAlliances(Clan clan)
        {
            var kingdom = clan.Kingdom;

            if (kingdom is null)
                return;

            var alliedKingdoms = KingdomExtensions.AllActiveKingdoms
                .Where(k => k != kingdom && FactionManager.IsAlliedWithFaction(kingdom, k))
                .ToList();

            foreach (var alliedKingdom in alliedKingdoms)
                BreakAllianceAction.Apply(kingdom, alliedKingdom);
        }
    }
}