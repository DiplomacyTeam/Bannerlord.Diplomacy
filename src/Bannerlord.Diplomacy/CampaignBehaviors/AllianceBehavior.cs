using Diplomacy.DiplomaticAction;
using Diplomacy.DiplomaticAction.Alliance;
using Diplomacy.DiplomaticAction.WarPeace;
using Diplomacy.Event;

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
            Events.WarDeclared.AddNonSerializedListener(this, WarDeclared);
            Events.AllianceFormed.AddNonSerializedListener(this, AllianceFormed);
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

            Kingdom.All.Where(k => k != allianceFormedEvent.Kingdom && k != allianceFormedEvent.OtherKingdom
                                   && (FactionManager.IsAtWarAgainstFaction(k, allianceFormedEvent.Kingdom) || FactionManager.IsAtWarAgainstFaction(k, allianceFormedEvent.OtherKingdom)))
                       .ToList().ForEach(k => AllianceEnemiesSync(allianceFormedEvent, k));
        }

        private void AllianceEnemiesSync(AllianceEvent allianceFormedEvent, Kingdom kingdomToDeclareWarOn)
        {
            bool kingdomStanceIsWar = FactionManager.IsAtWarAgainstFaction(allianceFormedEvent.Kingdom, kingdomToDeclareWarOn);
            bool otherKingdomStanceIsWar = FactionManager.IsAtWarAgainstFaction(allianceFormedEvent.OtherKingdom, kingdomToDeclareWarOn);

            if (!kingdomStanceIsWar || !otherKingdomStanceIsWar)
            {
                Kingdom kingdomDeclaringWar = kingdomStanceIsWar ? allianceFormedEvent.OtherKingdom : allianceFormedEvent.Kingdom;
                if (FactionManager.IsAlliedWithFaction(kingdomDeclaringWar, kingdomToDeclareWarOn))
                {
                    if (BreakAllianceAction.CanApply(kingdomDeclaringWar, kingdomToDeclareWarOn, DiplomacyConditionType.OnlyBasics))
                    {
                        BreakAllianceAction.Apply(kingdomDeclaringWar, kingdomToDeclareWarOn, bypassCosts: true);
                    }
                    if (DeclareWarConditions.Instance.CanApply(kingdomDeclaringWar, kingdomToDeclareWarOn, DiplomacyConditionType.OnlyBasics))
                    {
                        DoDeclareWar(kingdomDeclaringWar, kingdomToDeclareWarOn, kingdomStanceIsWar ? allianceFormedEvent.Kingdom : allianceFormedEvent.OtherKingdom);
                    }
                }
            }
        }

        private void WarDeclared(WarDeclaredEvent warDeclaredEvent)
        {
            if (!warDeclaredEvent.IsProvoked && warDeclaredEvent.Faction is Kingdom attacker && warDeclaredEvent.ProvocatorFaction is Kingdom defender)
            {
                SupportAlliedKingdom(defender, attacker);
            }
        }

        private void SupportAlliedKingdom(Kingdom kingdom, Kingdom kingdomToDeclareWarOn)
        {
            var allies = Kingdom.All.Where(k => kingdom != k && FactionManager.IsAlliedWithFaction(kingdom, k));

            foreach (var ally in allies)
            {
                if (!DeclareWarConditions.Instance.CanApply(ally, kingdomToDeclareWarOn, accountedConditions: DiplomacyConditionType.BypassCosts))
                    continue;
                DoDeclareWar(ally, kingdomToDeclareWarOn, kingdom);
            }
        }

        private static void DoDeclareWar(Kingdom kingdomDeclaringWar, Kingdom kingdomToDeclareWarOn, Kingdom allyKingdom)
        {
            DeclareWarAction.ApplyDeclareWarOverProvocation(kingdomDeclaringWar, kingdomToDeclareWarOn);
            var txt = new TextObject("{=UDC8eW7s}{ALLIED_KINGDOM} is joining their ally, {KINGDOM}, in the war against {ENEMY_KINGDOM}.");
            txt.SetTextVariable("ALLIED_KINGDOM", kingdomDeclaringWar.Name);
            txt.SetTextVariable("KINGDOM", allyKingdom.Name);
            txt.SetTextVariable("ENEMY_KINGDOM", kingdomToDeclareWarOn.Name);

            InformationManager.DisplayMessage(new InformationMessage(txt.ToString(), SubModule.StdTextColor));
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
            var potentialAllies = Kingdom.All.Where(k => k != kingdom
                                                         && FormAllianceConditions.Instance.CanApply(kingdom, k) //we need to doublecheck the score coz condition has auto-rejection threshold now. 
                                                         && DeclareAllianceAction.GetActionScore(kingdom, k, DiplomaticPartyType.Proposer) >= AllianceScoringModel.Instance.ScoreThreshold).ToList();

            foreach (var potentialAlly in potentialAllies)
            {
                if (MBRandom.RandomFloat < 0.05f && DeclareAllianceAction.CanApply(kingdom, potentialAlly))
                {
                    DeclareAllianceAction.Apply(kingdom, potentialAlly);
                }
            }
        }

        private static void ConsiderBreakingAlliances(Kingdom kingdom)
        {
            var alliedKingdoms = Kingdom.All.Where(k => k != kingdom && FactionManager.IsAlliedWithFaction(kingdom, k)).ToList();

            foreach (var alliedKingdom in alliedKingdoms)
            {
                if (MBRandom.RandomFloat < 0.05f
                    && !AllianceScoringModel.Instance.ShouldForm(kingdom, alliedKingdom)
                    && BreakAllianceAction.CanApply(kingdom, alliedKingdom))
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

            var alliedKingdoms = Kingdom.All.Where(k => k != kingdom && FactionManager.IsAlliedWithFaction(kingdom, k)).ToList();

            foreach (var alliedKingdom in alliedKingdoms)
                BreakAllianceAction.Apply(kingdom, alliedKingdom);
        }
    }
}