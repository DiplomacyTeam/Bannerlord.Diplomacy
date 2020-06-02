using DiplomacyFixes.Alliance;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace DiplomacyFixes.CampaignEventBehaviors
{
    class AllianceBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, this.DailyTickClan);
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, this.WarDeclared);
            Events.AllianceFormed.AddNonSerializedListener(this, this.AllianceFormed);
        }

        private void AllianceFormed(AllianceFormedEvent allianceFormedEvent)
        {
            TextObject textObject = new TextObject("{=PdN5g5ub}{KINGDOM} has formed an alliance with {OTHER_KINGDOM}!");
            textObject.SetTextVariable("KINGDOM", allianceFormedEvent.Kingdom.Name);
            textObject.SetTextVariable("OTHER_KINGDOM", allianceFormedEvent.OtherKingdom.Name);
            if (allianceFormedEvent.Kingdom == Clan.PlayerClan.Kingdom || allianceFormedEvent.OtherKingdom == Clan.PlayerClan.Kingdom)
            {

                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=qIa19an4}Alliance Formed").ToString(), textObject.ToString(), true, false, GameTexts.FindText("str_ok", null).ToString(), null, null, null, ""));
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString()));
            }
        }

        private void WarDeclared(IFaction faction1, IFaction faction2)
        {
            Kingdom attackingKingdom = faction1 as Kingdom;
            Kingdom defendingKingdom = faction2 as Kingdom;

            if (attackingKingdom == null || defendingKingdom == null)
            {
                return;
            }

            AlliedKingdomDeclareWar(attackingKingdom, defendingKingdom);
            AlliedKingdomDeclareWar(defendingKingdom, attackingKingdom);
        }

        private void AlliedKingdomDeclareWar(Kingdom kingdom, Kingdom kingdomToDeclareWarOn)
        {
            IEnumerable<Kingdom> alliedKingdoms = Kingdom.All.Where(curKingdom => kingdom != curKingdom && FactionManager.IsAlliedWithFaction(kingdom, curKingdom));
            IEnumerable<Kingdom> kingdomsToDeclareWarOn = Kingdom.All.Where(curKingdom => FactionManager.IsAlliedWithFaction(kingdomToDeclareWarOn, curKingdom));
            foreach (Kingdom alliedKingdom in alliedKingdoms)
            {
                foreach (Kingdom enemyKingdom in kingdomsToDeclareWarOn)
                {
                    if (FactionManager.IsAlliedWithFaction(alliedKingdom, enemyKingdom) || FactionManager.IsAtWarAgainstFaction(alliedKingdom, enemyKingdom))
                    {
                        continue;
                    }

                    DeclareWarAction.Apply(alliedKingdom, enemyKingdom);
                    TextObject textObject = new TextObject("{ALLIED_KINGDOM} is joining their ally, {KINGDOM}, in the war against {ENEMY_KINGDOM}.");
                    textObject.SetTextVariable("ALLIED_KINGDOM", alliedKingdom.Name);
                    textObject.SetTextVariable("KINGDOM", kingdom.Name);
                    textObject.SetTextVariable("ENEMY_KINGDOM", kingdomToDeclareWarOn.Name);

                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString()));
                }
            }
        }

        private void DailyTickClan(Clan clan)
        {
            if (!Settings.Instance.EnableAlliances)
            {
                BreakAllAlliances(clan);
                return;
            }

            if (clan.Leader == clan.Kingdom?.Leader && clan.Leader != Hero.MainHero)
            {
                Kingdom kingdom = clan.Kingdom;
                ConsiderBreakingAlliances(kingdom);
                ConsiderFormingAlliances(kingdom);
            }
        }

        private static void ConsiderFormingAlliances(Kingdom kingdom)
        {
            List<Kingdom> potentialAllies = Kingdom.All.Where(otherKingdom => otherKingdom != kingdom).Where(otherKingdom => AllianceConditions.CanFormAlliance(kingdom, otherKingdom)).ToList();

            foreach (Kingdom potentialAlly in potentialAllies)
            {
                if (MBRandom.RandomFloat < 0.05f && AllianceScoringModel.ShouldFormAlliance(kingdom, potentialAlly))
                {
                    DeclareAllianceAction.Apply(kingdom, potentialAlly);
                }
            }
        }

        private static void ConsiderBreakingAlliances(Kingdom kingdom)
        {
            List<Kingdom> alliedKingdoms = Kingdom.All.Where(otherKingdom => otherKingdom != kingdom).Where(otherKingdom => FactionManager.IsAlliedWithFaction(kingdom, otherKingdom)).ToList();

            foreach (Kingdom alliedKingdom in alliedKingdoms)
            {
                if (MBRandom.RandomFloat < 0.05f && AllianceConditions.CanBreakAlliance(kingdom, alliedKingdom) && !AllianceScoringModel.ShouldFormAlliance(kingdom, alliedKingdom))
                {
                    BreakAllianceAction.Apply(kingdom, alliedKingdom);
                }
            }
        }

        private static void BreakAllAlliances(Clan clan)
        {
            Kingdom kingdom = clan.Kingdom;
            List<Kingdom> alliedKingdoms = Kingdom.All.Where(otherKingdom => otherKingdom != kingdom).Where(otherKingdom => FactionManager.IsAlliedWithFaction(kingdom, otherKingdom)).ToList();

            foreach (Kingdom alliedKingdom in alliedKingdoms)
            {
                BreakAllianceAction.Apply(kingdom, alliedKingdom);
            }
            return;
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}
