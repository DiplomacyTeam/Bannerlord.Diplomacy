using Bannerlord.ButterLib.Common.Helpers;

using Diplomacy.Costs;
using Diplomacy.Events;
using Diplomacy.Extensions;
using Diplomacy.Helpers;
using Diplomacy.WarExhaustion;
using Diplomacy.WarExhaustion.EventRecords;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.WarPeace
{
    internal sealed class KingdomPeaceAction
    {
        private static readonly ILogger _logger = LogFactory.Get<KingdomPeaceAction>();

        private static readonly TextObject _TDefeatTitle = new("{=BXluvRnJ}Bitter Defeat");
        private static readonly TextObject _TTieTitle = new("{=0aZCy0at}Forced Peace");

        private const string _leaderDefeatedWithFiefs = "{=vLfbqXjq}Your armies and people are exhausted from the conflict with {ENEMY_KINGDOM} and have given up the fight. You must accept the terms of defeat and pay war reparations in the amount of {REPARATIONS}{GOLD_ICON}. These expenses will be distributed among all the clans of the kingdom.{?ADD_TRIBUTE}You also pledge to pay a daily tribute of {TRIBUTE}{GOLD_ICON}.{?}{\\?}{?FIEFS_TO_RETURN_LIST.ANY}{NEW_LINE} {NEW_LINE}On top of that, you have to return {FIEFS_TO_RETURN_LIST.START}{?FIEFS_TO_RETURN_LIST.IS_PLURAL} and {?}{\\?}{FIEFS_TO_RETURN_LIST.END} back to {ENEMY_KINGDOM}.{NEW_LINE} {NEW_LINE}{?} {\\?}The shame of defeat will cost you {INFLUENCE} influence.";
        private const string _leaderDefeatedWithNoFiefs = "{=ghZCj7hb}With your final stronghold falling to your enemies, you can no longer continue the fight with {ENEMY_KINGDOM}. You must accept the terms of defeat{?TO_BE_DESTROYED}. Without land or livelihood, your kingdom is no more.{?} and pay war reparations in the amount of {REPARATIONS}{GOLD_ICON}. These expenses will be distributed among all the clans of the kingdom. {?ADD_TRIBUTE}You also pledge to pay a daily tribute of {TRIBUTE}{GOLD_ICON}. {?}{\\?}The shame of defeat will cost you {INFLUENCE} influence.{\\?}";

        private const string _vassalDefeatedWithFiefs = "{=cKV5Jded}The armies and people of {KINGDOM.NAME} are exhausted from the conflict with {ENEMY_KINGDOM} and have given up the fight. {KINGDOM_LEADER.NAME} must accept the terms of defeat and pay war reparations in the amount of {REPARATIONS}{GOLD_ICON} (these expenses will be distributed among all the clans of the kingdom).{?ADD_TRIBUTE}{?KINGDOM_LEADER.GENDER}She{?}He{\\?} also pledges to pay a daily tribute of {TRIBUTE}{GOLD_ICON}.{?}{\\?}{?FIEFS_TO_RETURN_LIST.ANY}{NEW_LINE} {NEW_LINE}On top of that, {KINGDOM_LEADER.NAME} has to return {FIEFS_TO_RETURN_LIST.START}{?FIEFS_TO_RETURN_LIST.IS_PLURAL} and {?}{\\?}{FIEFS_TO_RETURN_LIST.END} back to {ENEMY_KINGDOM}.{NEW_LINE} {NEW_LINE}{?} {\\?}The shame of defeat will cost {?KINGDOM_LEADER.GENDER}her{?}him{\\?} {INFLUENCE} influence.";
        private const string _vassalDefeatedWithNoFiefs = "{=idAcjzXB}With your final stronghold falling to your enemies, you can no longer continue the fight with {ENEMY_KINGDOM}. {KINGDOM_LEADER.NAME} must accept the terms of defeat{?TO_BE_DESTROYED}Without land or livelihood, your kingdom is no more.{?} and pay war reparations in the amount of {REPARATIONS}{GOLD_ICON} (these expenses will be distributed among all the clans of the kingdom). {?ADD_TRIBUTE}{?KINGDOM_LEADER.GENDER}She{?}He{\\?} also pledges to pay a daily tribute of {TRIBUTE}{GOLD_ICON}. {?}{\\?}The shame of defeat will cost {?KINGDOM_LEADER.GENDER}her{?}him{\\?} {INFLUENCE} influence.{\\?}";

        private const string _tieMessage = "{=a3W5zKQr}The armies and people of both {KINGDOM.NAME} and {ENEMY_KINGDOM} are exhausted from the conflict and have given up the fight. There is no other choice but to make peace{?TO_BE_DESTROYED}, but without land or livelihood, {?IS_REBELLION}your rebellion is over{?}your kingdom is no more{\\?}{?}{\\?}.";

        private const string _peaceProposalWithFiefs = "{=HWiDa4R1}The armies and people of {KINGDOM.LINK} are exhausted from the conflict with {PLAYER_KINGDOM} and have given up the fight.{NEW_LINE} {NEW_LINE}{KINGDOM_LEADER.LINK} is proposing a peace treaty and is willing to pay war reparations in the amount of {REPARATIONS}{GOLD_ICON}.{?ADD_TRIBUTE}{?KINGDOM_LEADER.GENDER}She{?}He{\\?} also pledges to pay a daily tribute of {TRIBUTE}{GOLD_ICON}.{?}{\\?}{?FIEFS_TO_RETURN_LIST.ANY}{NEW_LINE} {NEW_LINE}On top of that, {KINGDOM_LEADER.NAME} has to return {FIEFS_TO_RETURN_LIST.START}{?FIEFS_TO_RETURN_LIST.IS_PLURAL} and {?}{\\?}{FIEFS_TO_RETURN_LIST.END} back to {PLAYER_KINGDOM}.{?}{\\?}";
        private const string _peaceProposalWithNoFiefs = "{=t0ZS9maD}With their final stronghold falling, {KINGDOM.LINK} can no longer continue the fight with {PLAYER_KINGDOM}.{NEW_LINE} {NEW_LINE}{KINGDOM_LEADER.LINK} proposes a peace treaty{?TO_BE_DESTROYED}, despite the fact that without land or livelihood, their kingdom would cease to exist.{?} and is willing to pay war reparations in the amount of {REPARATIONS}{GOLD_ICON}.{?ADD_TRIBUTE} {?KINGDOM_LEADER.GENDER}She{?}He{\\?} also pledges to pay a daily tribute of {TRIBUTE}{GOLD_ICON}.{?}{\\?}{\\?}";
        private const string _peaceProposalWhenTie = "{=ZrwszZww}The armies and people of both {KINGDOM.NAME} and {PLAYER_KINGDOM} are exhausted from the conflict and have given up the fight. {NEW_LINE} {NEW_LINE}{KINGDOM_LEADER.NAME} is proposing a peace treaty{?TO_BE_DESTROYED}, even though without land or livelihood, {?IS_REBELLION}their rebellion is practically over{?}their kingdom would be no more{\\?}{?} without putting forward any conditions{\\?}.";

        private const string _noChoice = "{=13G0c8RE}Given how badly your kingdom has been ravaged by the war, you have no choice but to accept the peace.";

        /*
        private const string _rebellionDefeated = "{=!}In the light of recent events, it is clear that your rebellion has failed. It's time to admit defeat and give the divided, war-weary {ORIGINAL_KINGDOM} a chance to reunite.";
        private const string _peaceProposalFromRebels = "{=!}In light of recent events, it is clear that the rebellion called {REBEL_KINGDOM} has failed. Is it time to accept their pleas for peace and forgiveness, thus giving your divided, war-weary kingdom a chance to be reunited?";
        */

        private static void ApplyPeaceInternal(Kingdom kingdomMakingPeace, Kingdom otherKingdom, bool isATie, bool skipPlayerPrompts, HybridCost diplomacyCost, int dailyPeaceTributeToPay, List<Town> fiefsToBeReturned, bool hasFiefsRemaining, bool shouldBeDestroyed)
        {
            if (kingdomMakingPeace == Clan.PlayerClan.Kingdom && !skipPlayerPrompts)
            {
                NotifyPlayerOfPeace(kingdomMakingPeace, otherKingdom, isATie, diplomacyCost, dailyPeaceTributeToPay, fiefsToBeReturned, hasFiefsRemaining, shouldBeDestroyed);
            }
            else if (!otherKingdom.Leader.IsHumanPlayerCharacter && !(otherKingdom == Clan.PlayerClan.Kingdom && Settings.Instance!.PlayerDiplomacyControl))
            {
                AcceptPeace(kingdomMakingPeace, otherKingdom, diplomacyCost, dailyPeaceTributeToPay, fiefsToBeReturned, shouldBeDestroyed);
            }
            else if (!CooldownManager.HasPeaceProposalCooldownWithPlayerKingdom(kingdomMakingPeace))
            {
                CreatePeaceInquiry(kingdomMakingPeace, otherKingdom, isATie, diplomacyCost, dailyPeaceTributeToPay, fiefsToBeReturned, hasFiefsRemaining, shouldBeDestroyed);
            }
        }

        private static void NotifyPlayerOfPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom, bool isATie, HybridCost diplomacyCost, int dailyPeaceTributeToPay, List<Town> fiefsToBeReturned, bool hasFiefsRemaining, bool shouldBeDestroyed)
        {
            GetNotificationInquiryTitleAndBody(kingdomMakingPeace, otherKingdom, isATie, diplomacyCost, dailyPeaceTributeToPay, fiefsToBeReturned, hasFiefsRemaining, shouldBeDestroyed, out var inquiryBody, out var inquiryTitle);

            InformationManager.ShowInquiry(new InquiryData(
                inquiryTitle.ToString(),
                inquiryBody.ToString(),
                true,
                false,
                GameTexts.FindText("str_ok").ToString(),
                null,
                () => AcceptPeace(kingdomMakingPeace, otherKingdom, diplomacyCost, dailyPeaceTributeToPay, fiefsToBeReturned, shouldBeDestroyed),
                null), true);
        }

        private static void GetNotificationInquiryTitleAndBody(Kingdom kingdomMakingPeace, Kingdom otherKingdom, bool isATie, HybridCost diplomacyCost, int dailyPeaceTributeToPay, List<Town> fiefsToBeReturned, bool hasFiefsRemaining, bool shouldBeDestroyed, out TextObject inquiryBody, out TextObject inquiryTitle)
        {
            var strArgs = new Dictionary<string, object>
            {
                {"ORGANIZATIONAL_EXPENSES", diplomacyCost.GoldCost.Value},
                {"REPARATIONS", diplomacyCost.KingdomWalletCosts.Where(c => c.PayingKingdom == kingdomMakingPeace).Sum(c => c.Value)},
                {"TRIBUTE", dailyPeaceTributeToPay},
                {"GOLD_ICON", StringConstants.GoldIcon},
                {"INFLUENCE", diplomacyCost.InfluenceCost.Value},
                {"ENEMY_KINGDOM", otherKingdom.Name},
                {"ADD_TRIBUTE", dailyPeaceTributeToPay != 0 ? 1 : 0},
                {"TO_BE_DESTROYED", shouldBeDestroyed ? 1 : 0},
                {"IS_REBELLION", otherKingdom.IsRebelKingdomOf(kingdomMakingPeace)},
                {"NEW_LINE", Environment.NewLine}
            };

            if (isATie)
            {
                inquiryTitle = _TTieTitle;
                inquiryBody = new(_tieMessage, strArgs);
                LocalizationHelper.SetEntityProperties(inquiryBody, "KINGDOM", kingdomMakingPeace, addLeaderInfo: false);
            }
            else
            {
                inquiryTitle = _TDefeatTitle;
                if (kingdomMakingPeace.Leader.IsHumanPlayerCharacter)
                {
                    inquiryBody = new(hasFiefsRemaining ? _leaderDefeatedWithFiefs : _leaderDefeatedWithNoFiefs, strArgs);
                }
                else
                {
                    inquiryBody = new(hasFiefsRemaining ? _vassalDefeatedWithFiefs : _vassalDefeatedWithNoFiefs, strArgs);
                    LocalizationHelper.SetEntityProperties(inquiryBody, "KINGDOM", kingdomMakingPeace, addLeaderInfo: true);
                }
                LocalizationHelper.SetListVariable(inquiryBody, "FIEFS_TO_RETURN_LIST", fiefsToBeReturned.Select(f => f.Name.ToString()).ToList());
            }
        }

        private static void AcceptPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom, HybridCost diplomacyCost, int dailyPeaceTributeToPay, List<Town> fiefsToBeReturned, bool shouldBeDestroyed)
        {
            diplomacyCost.ApplyCost();
            DoReturnFiefs(kingdomMakingPeace, otherKingdom, fiefsToBeReturned);
            MakePeaceAction.Apply(kingdomMakingPeace, otherKingdom, dailyPeaceTributeToPay);
            if (shouldBeDestroyed) DoDestroyKingdom(kingdomMakingPeace);

            DoLogging(kingdomMakingPeace, otherKingdom, diplomacyCost, dailyPeaceTributeToPay);
        }

        private static void DoLogging(Kingdom kingdomMakingPeace, Kingdom otherKingdom, HybridCost diplomacyCost, int dailyPeaceTributeToPay)
        {
            var goldCost = diplomacyCost.GoldCost.Value;
            var influenceCost = diplomacyCost.InfluenceCost.Value;
            var reparationsPaying = diplomacyCost.KingdomWalletCosts.Where(c => c.PayingKingdom == kingdomMakingPeace).Sum(c => c.Value);
            var reparationsReceiving = diplomacyCost.KingdomWalletCosts.Where(c => c.ReceivingKingdom == kingdomMakingPeace).Sum(c => c.Value);
            var reparationsPayingReport = reparationsPaying > 0 ? $" War reparations payed: {reparationsPaying} gold." : string.Empty;
            var reparationsReceivingReport = reparationsReceiving > 0 ? $" War reparations received: {reparationsReceiving} gold." : string.Empty;
            var trubuteReport = dailyPeaceTributeToPay > 0 ? $" They pledged to pay daily tribute of {dailyPeaceTributeToPay} gold." : dailyPeaceTributeToPay < 0 ? $" They are to get daily tribute of {-dailyPeaceTributeToPay} gold." : string.Empty;

            var fullReport = $"{kingdomMakingPeace.Name} secured peace with {otherKingdom.Name} (cost: {goldCost} gold and {influenceCost} influence).{reparationsPayingReport}{reparationsReceivingReport}{trubuteReport}";
            _logger.LogTrace($"[{CampaignTime.Now}] {fullReport}");
            if (Settings.Instance!.EnableWarExhaustionDebugMessages)
                InformationManager.DisplayMessage(new InformationMessage(fullReport, Color.FromUint(4282569842U)));
        }

        private static void DoReturnFiefs(Kingdom kingdomMakingPeace, Kingdom otherKingdom, List<Town> fiefsToBeReturned)
        {
            foreach (var fief in fiefsToBeReturned)
            {
                ChangeOwnerOfSettlementAction.ApplyByLeaveFaction(otherKingdom.Leader, fief.Settlement);
            }
        }

        private static void DoDestroyKingdom(Kingdom kingdomToDestroy)
        {
            foreach (Clan clan in kingdomToDestroy.Clans.ToList())
            {
                if (!clan.IsEliminated)
                {
                    if (clan.IsUnderMercenaryService)
                    {
                        ChangeKingdomAction.ApplyByLeaveKingdomAsMercenary(clan);
                    }
                    else
                    {
                        ChangeKingdomAction.ApplyByLeaveKingdom(clan);
                    }
                }
            }
            //Cold be already eliminated via other mods
            if (!kingdomToDestroy.IsEliminated) DestroyKingdomAction.Apply(kingdomToDestroy);
        }

        private static void CreatePeaceInquiry(Kingdom kingdomMakingPeace, Kingdom otherKingdom, bool isATie, HybridCost diplomacyCost, int dailyPeaceTributeToPay, List<Town> fiefsToBeReturned, bool hasFiefsRemaining, bool shouldBeDestroyed)
        {
#if v100 || v101 || v102 || v103
            InformationManager.ShowInquiry(new InquiryData(
                new TextObject("{=BkGSVccZ}Peace Proposal").ToString(),
                GetPeaceInquiryText(kingdomMakingPeace, otherKingdom, isATie, diplomacyCost, dailyPeaceTributeToPay, fiefsToBeReturned, hasFiefsRemaining, shouldBeDestroyed),
                true,
                IsDeclineAvailable(kingdomMakingPeace, otherKingdom),
                new TextObject("{=3fTqLwkC}Accept").ToString(),
                new TextObject("{=dRoMejb0}Decline").ToString(),
                () => AcceptPeace(kingdomMakingPeace, otherKingdom, diplomacyCost, dailyPeaceTributeToPay, fiefsToBeReturned, shouldBeDestroyed),
                () => DiplomacyEvents.Instance.OnPeaceProposalSent(kingdomMakingPeace)), true);
#else
            InformationManager.ShowInquiry(new InquiryData(
                new TextObject("{=BkGSVccZ}Peace Proposal").ToString(),
                GetPeaceInquiryText(kingdomMakingPeace, otherKingdom, isATie, diplomacyCost, dailyPeaceTributeToPay, fiefsToBeReturned, hasFiefsRemaining, shouldBeDestroyed),
                true,
                true,
                new TextObject("{=3fTqLwkC}Accept").ToString(),
                new TextObject("{=dRoMejb0}Decline").ToString(),
                () => AcceptPeace(kingdomMakingPeace, otherKingdom, diplomacyCost, dailyPeaceTributeToPay, fiefsToBeReturned, shouldBeDestroyed),
                () => DiplomacyEvents.Instance.OnPeaceProposalSent(kingdomMakingPeace),
                isNegativeOptionEnabled: () => IsDeclineAvailable(kingdomMakingPeace, otherKingdom)), true);
#endif
        }

        private static string GetPeaceInquiryText(Kingdom kingdomMakingPeace, Kingdom otherKingdom, bool isATie, HybridCost diplomacyCost, int dailyPeaceTributeToPay, List<Town> fiefsToBeReturned, bool hasFiefsRemaining, bool shouldBeDestroyed)
        {
            var strArgs = new Dictionary<string, object>
            {
                {"REPARATIONS", diplomacyCost.KingdomWalletCosts.Where(c => c.PayingKingdom == kingdomMakingPeace).Sum(c => c.Value)},
                {"TRIBUTE", dailyPeaceTributeToPay},
                {"GOLD_ICON", StringConstants.GoldIcon},
                {"PLAYER_KINGDOM", otherKingdom.Name},
                {"ADD_TRIBUTE", dailyPeaceTributeToPay != 0 ? 1 : 0},
                {"TO_BE_DESTROYED", shouldBeDestroyed ? 1 : 0},
                {"IS_REBELLION", otherKingdom.IsRebelKingdomOf(kingdomMakingPeace)},
                {"NEW_LINE", Environment.NewLine}
            };
            TextObject inquiryBody = new(isATie ? _peaceProposalWhenTie : hasFiefsRemaining ? _peaceProposalWithFiefs : _peaceProposalWithNoFiefs, strArgs);
            LocalizationHelper.SetEntityProperties(inquiryBody, "KINGDOM", kingdomMakingPeace, addLeaderInfo: true);
            if (!isATie && hasFiefsRemaining)
                LocalizationHelper.SetListVariable(inquiryBody, "FIEFS_TO_RETURN_LIST", fiefsToBeReturned.Select(f => f.Name.ToString()).ToList());

#if v100 || v101 || v102 || v103
            var peaceInquiryText = inquiryBody.ToString() + (!IsDeclineAvailable(kingdomMakingPeace, otherKingdom) ? $"\n \n{new TextObject(_noChoice)}" : string.Empty);
            return peaceInquiryText;
#else
            return inquiryBody.ToString();
#endif
        }

#if v100 || v101 || v102 || v103
        private static bool IsDeclineAvailable(Kingdom kingdomMakingPeace, Kingdom otherKingdom)
        {
            var warResultForOtherKingdom = WarExhaustionManager.Instance.GetWarResult(otherKingdom, kingdomMakingPeace);
            return warResultForOtherKingdom switch
            {
                > WarExhaustionManager.WarResult.None and <= WarExhaustionManager.WarResult.PyrrhicVictory => false,
                _ => true
            };
        }
#else
        private static (bool, string) IsDeclineAvailable(Kingdom kingdomMakingPeace, Kingdom otherKingdom)
        {
            var warResultForOtherKingdom = WarExhaustionManager.Instance.GetWarResult(otherKingdom, kingdomMakingPeace);
            return warResultForOtherKingdom switch
            {
                > WarExhaustionManager.WarResult.None and <= WarExhaustionManager.WarResult.PyrrhicVictory => (false, new TextObject(_noChoice).ToString()),
                _ => (true, string.Empty)
            };
        }
#endif

        public static void ApplyPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, bool skipPlayerPrompts = false)
        {
            var diplomacyCost = DiplomacyCostCalculator.DetermineCostForMakingPeace(kingdomMakingPeace, otherKingdom, forcePlayerCharacterCosts);
            var dailyPeaceTributeToPay = TributeHelper.GetDailyTribute(kingdomMakingPeace, otherKingdom);
            var isATie = WarExhaustionManager.Instance.GetWarResult(kingdomMakingPeace, otherKingdom) == WarExhaustionManager.WarResult.Tie;
            var fiefsToBeReturned = GetFiefsToBeReturned(kingdomMakingPeace, otherKingdom);
            var hasFiefsRemaining = kingdomMakingPeace.Fiefs.Count > 0;
            var shouldBeDestroyed = !hasFiefsRemaining && !FactionManager.GetEnemyKingdoms(kingdomMakingPeace).Any(k => k != otherKingdom && !k.IsEliminated);

            ApplyPeaceInternal(kingdomMakingPeace, otherKingdom, isATie, skipPlayerPrompts, diplomacyCost, dailyPeaceTributeToPay, fiefsToBeReturned, hasFiefsRemaining, shouldBeDestroyed);
        }

        private static List<Town> GetFiefsToBeReturned(Kingdom kingdomMakingPeace, Kingdom otherKingdom)
        {
            if (!Settings.Instance!.EnableWarExhaustion || !WarExhaustionManager.Instance.HasMaxWarExhaustion(kingdomMakingPeace, otherKingdom))
                return new();

            if (!kingdomMakingPeace.Fiefs.Any())
                return new();

            var warResultForOtherKingdom = WarExhaustionManager.Instance.GetWarResult(otherKingdom, kingdomMakingPeace);
            if (warResultForOtherKingdom <= WarExhaustionManager.WarResult.PyrrhicVictory)
                return new();

            var suitableFiefs = GetFiefsSuitableToBeReturnedInternal(kingdomMakingPeace, otherKingdom);
            if (!kingdomMakingPeace.Fiefs.Except(suitableFiefs).Any())
                suitableFiefs.Remove(suitableFiefs.GetRandomElementInefficiently());

            List<Town> fiefList = new();
            var numberToCheckOn = warResultForOtherKingdom == WarExhaustionManager.WarResult.Victory ? 0.5f : 0.1f;
            foreach (var fief in suitableFiefs)
            {
                if (MBRandom.RandomFloat < numberToCheckOn)
                {
                    fiefList.Add(fief);
                    numberToCheckOn -= 0.1f;
                }
            }
            return fiefList;
        }

        public static List<Town> GetFiefsSuitableToBeReturned(Kingdom kingdomMakingPeace, Kingdom otherKingdom)
        {
            if (!Settings.Instance!.EnableWarExhaustion || !WarExhaustionManager.Instance.HasMaxWarExhaustion(kingdomMakingPeace, otherKingdom))
                return new();

            if (!kingdomMakingPeace.Fiefs.Any())
                return new();

            var warResultForOtherKingdom = WarExhaustionManager.Instance.GetWarResult(otherKingdom, kingdomMakingPeace);
            if (warResultForOtherKingdom <= WarExhaustionManager.WarResult.PyrrhicVictory)
                return new();

            return GetFiefsSuitableToBeReturnedInternal(kingdomMakingPeace, otherKingdom);
        }

        private static List<Town> GetFiefsSuitableToBeReturnedInternal(Kingdom kingdomMakingPeace, Kingdom otherKingdom)
        {
            var eventRecords = WarExhaustionManager.Instance.GetWarExhaustionEventRecords(kingdomMakingPeace, otherKingdom, out var kingdoms);
            if (kingdoms is null || eventRecords.IsEmpty())
                return new();

            var settlementsCaptured = eventRecords.OfType<SiegeRecord>().Where(sr => !sr.YieldsDiminishingReturns && kingdoms.ReversedKeyOrder ? sr.Faction1Effected : sr.Faction2Effected).Select(sr => sr.Settlement).Distinct().ToList();
            return kingdomMakingPeace.Fiefs.Where(f => settlementsCaptured.Contains(f.Settlement)).ToList();
        }
    }
}