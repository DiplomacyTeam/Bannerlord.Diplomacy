using Diplomacy.DiplomaticAction.WarPeace;
using Diplomacy.Events;
using Diplomacy.Extensions;
using Diplomacy.Helpers;
using Diplomacy.WarExhaustion.EventRecords;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace Diplomacy.WarExhaustion
{
    internal sealed partial class WarExhaustionManager
    {
        private void AddDailyWarExhaustion(Kingdoms kingdoms, CampaignTime warStartDate)
        {
            var warExhaustionToAdd = GetDailyWarExhaustionDelta(kingdoms, warStartDate, out var dailyRecord);
            AddWarExhaustion(kingdoms, WarExhaustionType.Daily, warExhaustionToAdd, dailyRecord);
        }

        public void AddCasualtyWarExhaustion(Kingdoms kingdoms, MapEvent mapEvent)
        {
            var warExhaustionToAdd = GetCasualtyWarExhaustionDelta(kingdoms, mapEvent, out var battleCasualtyRecord);
            AddWarExhaustion(kingdoms, WarExhaustionType.Casualty, warExhaustionToAdd, battleCasualtyRecord);
        }

        public void AddCasualtyWarExhaustion(Kingdom kingdom1, Kingdom kingdom2, int kingdom1Casualties, int kingdom2Casualties, TextObject? kingdom1PartyName = null, TextObject? kingdom2PartyName = null)
        {
            CreateKey(kingdom1, kingdom2, out var kingdoms);
            if (kingdoms is null) return;

            var warExhaustionToAdd = GetCasualtyWarExhaustionDelta(kingdoms, kingdom1Casualties, kingdom2Casualties, kingdom1PartyName, kingdom2PartyName, out var battleCasualtyRecord);
            AddWarExhaustion(kingdoms, WarExhaustionType.Casualty, warExhaustionToAdd, battleCasualtyRecord);
        }

#if v100 || v101 || v102 || v103
        public void AddRaidWarExhaustion(Kingdoms kingdoms, MapEvent mapEvent)
        {
            var warExhaustionToAdd = GetRaidWarExhaustionDelta(kingdoms, mapEvent.MapEventSettlement.Village, mapEvent.AttackerSide.LeaderParty?.Name ?? kingdoms.Kingdom1.Name, out var raidRecord);
            AddWarExhaustion(kingdoms, WarExhaustionType.Raid, warExhaustionToAdd, raidRecord);
        }
#else
        public void AddRaidWarExhaustion(Kingdoms kingdoms, RaidEventComponent raidEventComponent)
        {
            var warExhaustionToAdd = GetRaidWarExhaustionDelta(kingdoms, raidEventComponent.MapEventSettlement.Village, raidEventComponent.AttackerSide.LeaderParty?.Name ?? kingdoms.Kingdom1.Name, out var raidRecord);
            AddWarExhaustion(kingdoms, WarExhaustionType.Raid, warExhaustionToAdd, raidRecord);
        }
#endif

        public void AddSiegeWarExhaustion(Kingdoms kingdoms, MapEvent mapEvent)
        {
            var warExhaustionToAdd = GetSiegeWarExhaustionDelta(kingdoms, mapEvent, out var siegeRecord);
            AddWarExhaustion(kingdoms, WarExhaustionType.Siege, warExhaustionToAdd, siegeRecord);
        }

        public void AddHeroImprisonedWarExhaustion(Kingdoms kingdoms, Hero hero, TextObject otherPartyName)
        {
            var warExhaustionToAdd = GetHeroImprisonedWarExhaustionDelta(kingdoms, hero, otherPartyName, out var heroImprisonedRecord);
            AddWarExhaustion(kingdoms, WarExhaustionType.HeroImprisoned, warExhaustionToAdd, heroImprisonedRecord);
        }

        public void AddHeroPerishedWarExhaustion(Kingdoms kingdoms, Hero hero, TextObject otherPartyName, KillCharacterAction.KillCharacterActionDetail deathDetail)
        {
            var warExhaustionToAdd = GetHeroPerishedWarExhaustionDelta(kingdoms, hero, otherPartyName, deathDetail, out var heroPerishedRecord);
            AddWarExhaustion(kingdoms, WarExhaustionType.HeroPerished, warExhaustionToAdd, heroPerishedRecord);
        }

        internal void AddCaravanRaidWarExhaustion(Kingdoms kingdoms, MobileParty cp, MapEventSide winningSide)
        {
            var warExhaustionToAdd = GetCaravanRaidWarExhaustionDelta(kingdoms, cp, winningSide, out var caravanRaidRecord);
            AddWarExhaustion(kingdoms, WarExhaustionType.CaravanRaid, warExhaustionToAdd, caravanRaidRecord);
        }

        public void AddOccupiedWarExhaustion(Kingdoms kingdoms)
        {
            var warExhaustionToAdd = GetOccupiedWarExhaustionDelta(kingdoms, out var occupiedRecord);
            AddWarExhaustion(kingdoms, WarExhaustionType.Occupied, warExhaustionToAdd, occupiedRecord);
        }

        public void AddDivineWarExhaustion(Kingdom kingdom1, Kingdom kingdom2, int targetWarExhaustion)
        {
            CreateKey(kingdom1, kingdom2, out var kingdoms);
            if (kingdoms is null) return;

            var warExhaustionToAdd = GetDivineWarExhaustionDelta(kingdoms, targetWarExhaustion, out var divineInterventionRecord);
            AddWarExhaustion(kingdoms, WarExhaustionType.Divine, warExhaustionToAdd, divineInterventionRecord);
        }

        private void AddWarExhaustion(Kingdoms kingdoms, WarExhaustionType warExhaustionType, WarExhaustionRecord warExhaustionToAdd, WarExhaustionEventRecord? eventRecord = null)
        {
            var key = kingdoms.Key;
            if (key is null)
                return;

            if (_warExhaustionScores.TryGetValue(key, out var currentValue))
                _warExhaustionScores[key] = currentValue + warExhaustionToAdd;
            else
                _warExhaustionScores[key] = warExhaustionToAdd;

            //Divine intervention can override the victor
            var newValue = _warExhaustionScores[key];
            if (warExhaustionType == WarExhaustionType.Divine && Math.Max(newValue.Faction1Value, newValue.Faction2Value) < MaxWarExhaustion && newValue.VictoriousFaction != WarExhaustionRecord.VictoriousFactionType.None)
            {
                _warExhaustionScores[key] = new(newValue.Faction1Value, newValue.Faction2Value, WarExhaustionRecord.VictoriousFactionType.None, newValue.QuestState);
            }

            if (eventRecord is not null)
            {
                if (_warExhaustionEventRecords.TryGetValue(key, out var currentRecords))
                    HandleEventRecord(eventRecord, currentRecords);
                else
                    _warExhaustionEventRecords[key] = new List<WarExhaustionEventRecord> { eventRecord };
            }

            if (Settings.Instance!.EnableWarExhaustionDebugMessages && (kingdoms.Kingdom1 == Hero.MainHero.MapFaction || kingdoms.Kingdom2 == Hero.MainHero.MapFaction))
            {
                var information =
                    $"Added {warExhaustionToAdd} {Enum.GetName(typeof(WarExhaustionType), warExhaustionType)} war exhaustion to {kingdoms.Kingdom1.Name}'s war with {kingdoms.Kingdom2.Name}";
                InformationManager.DisplayMessage(new InformationMessage(information, Color.FromUint(4282569842U)));
            }

            InvokeEnevtsIfApplicable(kingdoms, warExhaustionToAdd, warExhaustionType);
        }

        private static void HandleEventRecord(WarExhaustionEventRecord eventRecord, List<WarExhaustionEventRecord> currentRecords)
        {
            if (eventRecord is DailyRecord dailyRecord)
            {
                var lastRecord = currentRecords.OfType<DailyRecord>().OrderByDescending(r => r.EventDate).FirstOrDefault();
                if (lastRecord != null && lastRecord.CanBeCompoundedWith(dailyRecord))
                    currentRecords[currentRecords.IndexOf(lastRecord)] = lastRecord.CompoundWith(dailyRecord);
                else
                    currentRecords.Add(eventRecord);
            }
            else
            {
                currentRecords.Add(eventRecord);
            }
        }

        private static void InvokeEnevtsIfApplicable(Kingdoms kingdoms, WarExhaustionRecord warExhaustionToAdd, WarExhaustionType warExhaustionType)
        {
            if (kingdoms.ReversedKeyOrder)
            {
                InvokeIfApplicableForValue(kingdoms.Kingdom1, kingdoms.Kingdom2, warExhaustionType, warExhaustionToAdd.Faction2Value);
                InvokeIfApplicableForValue(kingdoms.Kingdom2, kingdoms.Kingdom1, warExhaustionType, warExhaustionToAdd.Faction1Value);
            }
            else
            {
                InvokeIfApplicableForValue(kingdoms.Kingdom1, kingdoms.Kingdom2, warExhaustionType, warExhaustionToAdd.Faction1Value);
                InvokeIfApplicableForValue(kingdoms.Kingdom2, kingdoms.Kingdom1, warExhaustionType, warExhaustionToAdd.Faction2Value);
            }

            static void InvokeIfApplicableForValue(Kingdom kingdom1, Kingdom kingdom2, WarExhaustionType warExhaustionType, float warExhaustionToAdd)
            {
                if (warExhaustionToAdd != 0f)
                {
                    DiplomacyEvents.Instance.OnWarExhaustionAdded(new WarExhaustionAddedEvent(kingdom1, kingdom2, warExhaustionType, warExhaustionToAdd));
                }
            }
        }

        private WarExhaustionRecord GetDailyWarExhaustionDelta(Kingdoms kingdoms, CampaignTime warStartDate, out DailyRecord dailyRecord)
        {
            if (!_warExhaustionEventRecords.TryGetValue(kingdoms.Key!, out var currentRecords))
                currentRecords = new();

            CampaignTime eventDate;
            var lastDailyRecord = currentRecords.OfType<DailyRecord>().OrderByDescending(r => r.EventDate).FirstOrDefault();
            if (lastDailyRecord != null)
                eventDate = lastDailyRecord.EventDate + CampaignTime.Days(Math.Max(lastDailyRecord.Faction1Days, lastDailyRecord.Faction2Days));
            else
                eventDate = warStartDate;

            GetWarExhaustionPerDay(out var warExhaustionPerDay, out var warExhaustionPerDayOccupied);
            var faction1 = kingdoms.ReversedKeyOrder ? kingdoms.Kingdom2 : kingdoms.Kingdom1;
            var faction2 = kingdoms.ReversedKeyOrder ? kingdoms.Kingdom1 : kingdoms.Kingdom2;
            var faction1IsOccupied = !faction1.Fiefs.Any();
            var faction2IsOccupied = !faction2.Fiefs.Any();
            var faction1WarExhaustion = faction1IsOccupied ? warExhaustionPerDayOccupied : warExhaustionPerDay;
            var faction2WarExhaustion = faction2IsOccupied ? warExhaustionPerDayOccupied : warExhaustionPerDay;
            var hasActiveQuest = !IsValidQuestState(kingdoms.Kingdom1, kingdoms.Kingdom2);

            dailyRecord = new(eventDate, 1, faction1WarExhaustion, 1, faction2WarExhaustion, faction1IsOccupied, faction2IsOccupied);
            return new(faction1WarExhaustion, faction2WarExhaustion, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
        }

        private static void GetWarExhaustionPerDay(out float warExhaustionPerDay, out float warExhaustionPerDayOccupied)
        {
            warExhaustionPerDay = Settings.Instance!.WarExhaustionPerDay;
            warExhaustionPerDayOccupied = Math.Max(warExhaustionPerDay * Settings.Instance!.FieflessWarExhaustionMultiplier, 1f);
        }

        private WarExhaustionRecord GetCasualtyWarExhaustionDelta(Kingdoms kingdoms, MapEvent mapEvent, out BattleCasualtyRecord battleCasualtyRecord)
        {
            int attackerSideCasualties = mapEvent.AttackerSide.Casualties;
            int defenderSideCasualties = mapEvent.DefenderSide.Casualties;

            var warExhaustionPerCasualty = Settings.Instance!.WarExhaustionPerCasualty;
            float attackerSideWarExhaustion = attackerSideCasualties * warExhaustionPerCasualty;
            float defenderSideWarExhaustion = defenderSideCasualties * warExhaustionPerCasualty;

            TextObject attackerSidePartyName = mapEvent.AttackerSide.LeaderParty?.Name ?? kingdoms.Kingdom1.Name;
            TextObject defenderSidePartyName = mapEvent.DefenderSide.LeaderParty?.Name ?? kingdoms.Kingdom2.Name;
            var eventRelatedSettlement = mapEvent.MapEventSettlement;
            var hasActiveQuest = !IsValidQuestState(kingdoms.Kingdom1, kingdoms.Kingdom2);

            var rates = GetWarExhaustionRates(kingdoms);
            if (kingdoms.ReversedKeyOrder)
            {
                battleCasualtyRecord = new(CampaignTime.Now, defenderSideCasualties, defenderSideWarExhaustion * rates.Faction1Value, attackerSideCasualties, attackerSideWarExhaustion * rates.Faction2Value, defenderSidePartyName, attackerSidePartyName, eventRelatedSettlement);
                return new(defenderSideWarExhaustion * rates.Faction1Value, attackerSideWarExhaustion * rates.Faction2Value, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
            }
            else
            {
                battleCasualtyRecord = new(CampaignTime.Now, attackerSideCasualties, attackerSideWarExhaustion * rates.Faction1Value, defenderSideCasualties, defenderSideWarExhaustion * rates.Faction2Value, attackerSidePartyName, defenderSidePartyName, eventRelatedSettlement);
                return new(attackerSideWarExhaustion * rates.Faction1Value, defenderSideWarExhaustion * rates.Faction2Value, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
            }
        }

        private WarExhaustionRecord GetCasualtyWarExhaustionDelta(Kingdoms kingdoms, int kingdom1Casualties, int kingdom2Casualties, TextObject? kingdom1PartyName, TextObject? kingdom2PartyName, out BattleCasualtyRecord battleCasualtyRecord)
        {
            var warExhaustionPerCasualty = Settings.Instance!.WarExhaustionPerCasualty;
            float kingdom1WarExhaustion = kingdom1Casualties * warExhaustionPerCasualty;
            float kingdom2WarExhaustion = kingdom2Casualties * warExhaustionPerCasualty;

            kingdom1PartyName ??= kingdoms.Kingdom1.Name;
            kingdom2PartyName ??= kingdoms.Kingdom2.Name;
            var hasActiveQuest = !IsValidQuestState(kingdoms.Kingdom1, kingdoms.Kingdom2);

            var rates = GetWarExhaustionRates(kingdoms);
            if (kingdoms.ReversedKeyOrder)
            {
                battleCasualtyRecord = new(CampaignTime.Now, kingdom2Casualties, kingdom2WarExhaustion * rates.Faction1Value, kingdom1Casualties, kingdom1WarExhaustion * rates.Faction2Value, kingdom2PartyName, kingdom1PartyName, eventRelatedSettlement: null);
                return new(kingdom2WarExhaustion * rates.Faction1Value, kingdom1WarExhaustion * rates.Faction2Value, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
            }
            else
            {
                battleCasualtyRecord = new(CampaignTime.Now, kingdom1Casualties, kingdom1WarExhaustion * rates.Faction1Value, kingdom2Casualties, kingdom2WarExhaustion * rates.Faction2Value, kingdom1PartyName, kingdom2PartyName, eventRelatedSettlement: null);
                return new(kingdom1WarExhaustion * rates.Faction1Value, kingdom2WarExhaustion * rates.Faction2Value, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
            }
        }

        private WarExhaustionRecord GetRaidWarExhaustionDelta(Kingdoms kingdoms, Village raidedVillage, TextObject raidingPartyName, out RaidRecord raidRecord)
        {
            float attackerSideWarExhaustion = 0f;
            float defenderSideWarExhaustion = Settings.Instance!.WarExhaustionPerRaid * GetDiminishingReturnsFactor<Village, RaidRecord>(kingdoms, raidedVillage, out var yieldsDiminishingReturns);
            var hasActiveQuest = !IsValidQuestState(kingdoms.Kingdom1, kingdoms.Kingdom2);

            var rates = GetWarExhaustionRates(kingdoms);
            if (kingdoms.ReversedKeyOrder)
            {
                raidRecord = new(CampaignTime.Now, raidedVillage, 1, defenderSideWarExhaustion * rates.Faction1Value, 0, attackerSideWarExhaustion * rates.Faction2Value, raidingPartyName, yieldsDiminishingReturns);
                return new(defenderSideWarExhaustion * rates.Faction1Value, attackerSideWarExhaustion * rates.Faction2Value, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
            }
            else
            {
                raidRecord = new(CampaignTime.Now, raidedVillage, 0, attackerSideWarExhaustion * rates.Faction1Value, 1, defenderSideWarExhaustion * rates.Faction2Value, raidingPartyName, yieldsDiminishingReturns);
                return new(attackerSideWarExhaustion * rates.Faction1Value, defenderSideWarExhaustion * rates.Faction2Value, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
            }
        }

        private WarExhaustionRecord GetSiegeWarExhaustionDelta(Kingdoms kingdoms, MapEvent mapEvent, out SiegeRecord siegeRecord)
        {
            var eventRelatedSettlement = mapEvent.MapEventSettlement;
            var importanceFactor = eventRelatedSettlement.IsCastle ? 0.5f : 1f;
            float attackerSideWarExhaustion = 0f;
            float defenderSideWarExhaustion = Settings.Instance!.WarExhaustionPerSiege * importanceFactor * GetDiminishingReturnsFactor<Settlement, SiegeRecord>(kingdoms, eventRelatedSettlement, out var yieldsDiminishingReturns);
            var hasActiveQuest = !IsValidQuestState(kingdoms.Kingdom1, kingdoms.Kingdom2);

            TextObject attackerSidePartyName = mapEvent.AttackerSide.LeaderParty?.Name ?? kingdoms.Kingdom1.Name;
            TextObject defenderSidePartyName = mapEvent.DefenderSide.LeaderParty?.Name ?? new("{=fnPa5bas}garrisoned troops");

            var rates = GetWarExhaustionRates(kingdoms);
            if (kingdoms.ReversedKeyOrder)
            {
                siegeRecord = new(CampaignTime.Now, eventRelatedSettlement, 1, defenderSideWarExhaustion * rates.Faction1Value, 0, attackerSideWarExhaustion * rates.Faction2Value, defenderSidePartyName, attackerSidePartyName, yieldsDiminishingReturns);
                return new(defenderSideWarExhaustion * rates.Faction1Value, attackerSideWarExhaustion * rates.Faction2Value, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
            }
            else
            {
                siegeRecord = new(CampaignTime.Now, eventRelatedSettlement, 0, attackerSideWarExhaustion * rates.Faction1Value, 1, defenderSideWarExhaustion * rates.Faction2Value, attackerSidePartyName, defenderSidePartyName, yieldsDiminishingReturns);
                return new(attackerSideWarExhaustion * rates.Faction1Value, defenderSideWarExhaustion * rates.Faction2Value, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
            }
        }

        private WarExhaustionRecord GetHeroImprisonedWarExhaustionDelta(Kingdoms kingdoms, Hero hero, TextObject otherPartyName, out HeroImprisonedRecord heroImprisonedRecord)
        {
            var importanceFactor = GetImportanceFactor(GetHeroImportanceForFaction(hero, kingdoms.Kingdom2));
            float attackerSideWarExhaustion = 0f;
            float defenderSideWarExhaustion = Settings.Instance!.WarExhaustionPerImprisonment * importanceFactor * GetDiminishingReturnsFactor<Hero, HeroImprisonedRecord>(kingdoms, hero, out var yieldsDiminishingReturns);
            var hasActiveQuest = !IsValidQuestState(kingdoms.Kingdom1, kingdoms.Kingdom2);

            var rates = GetWarExhaustionRates(kingdoms);
            if (kingdoms.ReversedKeyOrder)
            {
                heroImprisonedRecord = new(CampaignTime.Now, hero, 1, defenderSideWarExhaustion * rates.Faction1Value, 0, attackerSideWarExhaustion * rates.Faction2Value, otherPartyName, yieldsDiminishingReturns);
                return new(defenderSideWarExhaustion * rates.Faction1Value, attackerSideWarExhaustion * rates.Faction2Value, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
            }
            else
            {
                heroImprisonedRecord = new(CampaignTime.Now, hero, 0, attackerSideWarExhaustion * rates.Faction1Value, 1, defenderSideWarExhaustion * rates.Faction2Value, otherPartyName, yieldsDiminishingReturns);
                return new(attackerSideWarExhaustion * rates.Faction1Value, defenderSideWarExhaustion * rates.Faction2Value, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
            }
        }

        private WarExhaustionRecord GetHeroPerishedWarExhaustionDelta(Kingdoms kingdoms, Hero hero, TextObject otherPartyName, KillCharacterAction.KillCharacterActionDetail deathDetail, out HeroPerishedRecord heroPerishedRecord)
        {
            var importanceFactor = GetImportanceFactor(GetHeroImportanceForFaction(hero, kingdoms.Kingdom2));
            float attackerSideWarExhaustion = 0f;
            float defenderSideWarExhaustion = Settings.Instance!.WarExhaustionPerDeath * importanceFactor * GetDiminishingReturnsFactor<Hero, HeroPerishedRecord>(kingdoms, hero, out var yieldsDiminishingReturns);
            var hasActiveQuest = !IsValidQuestState(kingdoms.Kingdom1, kingdoms.Kingdom2);

            var rates = GetWarExhaustionRates(kingdoms);
            if (kingdoms.ReversedKeyOrder)
            {
                heroPerishedRecord = new(CampaignTime.Now, hero, 1, defenderSideWarExhaustion * rates.Faction1Value, 0, attackerSideWarExhaustion * rates.Faction2Value, otherPartyName, deathDetail, yieldsDiminishingReturns);
                return new(defenderSideWarExhaustion * rates.Faction1Value, attackerSideWarExhaustion * rates.Faction2Value, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
            }
            else
            {
                heroPerishedRecord = new(CampaignTime.Now, hero, 0, attackerSideWarExhaustion * rates.Faction1Value, 1, defenderSideWarExhaustion * rates.Faction2Value, otherPartyName, deathDetail, yieldsDiminishingReturns);
                return new(attackerSideWarExhaustion * rates.Faction1Value, defenderSideWarExhaustion * rates.Faction2Value, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
            }
        }

        private WarExhaustionRecord GetCaravanRaidWarExhaustionDelta(Kingdoms kingdoms, MobileParty cp, MapEventSide winningSide, out CaravanRaidRecord caravanRaidRecord)
        {
            float winningSideWarExhaustion = 0f;
            float loosingSideWarExhaustion = Settings.Instance!.WarExhaustionPerCaravanRaid;
            var hasActiveQuest = !IsValidQuestState(kingdoms.Kingdom1, kingdoms.Kingdom2);

            TextObject winningSidePartyName = winningSide.LeaderParty?.Name ?? kingdoms.Kingdom1.Name;
            TextObject loosingSidePartyName = cp.Name ?? new("{=rwFeQWky}A caravan of {KINGDOM_NAME}", new() { ["KINGDOM_NAME"] = kingdoms.Kingdom2.Name });

            var rates = GetWarExhaustionRates(kingdoms);
            if (kingdoms.ReversedKeyOrder)
            {
                caravanRaidRecord = new(CampaignTime.Now, 1, loosingSideWarExhaustion * rates.Faction1Value, 0, winningSideWarExhaustion * rates.Faction2Value, loosingSidePartyName, winningSidePartyName);
                return new(loosingSideWarExhaustion * rates.Faction1Value, winningSideWarExhaustion * rates.Faction2Value, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
            }
            else
            {
                caravanRaidRecord = new(CampaignTime.Now, 0, winningSideWarExhaustion * rates.Faction1Value, 1, loosingSideWarExhaustion * rates.Faction2Value, winningSidePartyName, loosingSidePartyName);
                return new(winningSideWarExhaustion * rates.Faction1Value, loosingSideWarExhaustion * rates.Faction2Value, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
            }
        }

        private WarExhaustionRecord GetOccupiedWarExhaustionDelta(Kingdoms kingdoms, out OccupiedRecord occupiedRecord)
        {
            float attackerSideWarExhaustion = 0f;
            float defenderSideWarExhaustion = Settings.Instance!.WarExhaustionWhenOccupied * GetDiminishingReturnsFactor<Kingdom, OccupiedRecord>(kingdoms, kingdoms.Kingdom2, out var yieldsDiminishingReturns);
            var attackerSide = kingdoms.Kingdom1;
            var defenderSide = kingdoms.Kingdom2;
            var hasActiveQuest = !IsValidQuestState(kingdoms.Kingdom1, kingdoms.Kingdom2);

            if (kingdoms.ReversedKeyOrder)
            {
                occupiedRecord = new(CampaignTime.Now, 1, defenderSideWarExhaustion, 0, attackerSideWarExhaustion, defenderSide, attackerSide, yieldsDiminishingReturns);
                return new(defenderSideWarExhaustion, attackerSideWarExhaustion, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
            }
            else
            {
                occupiedRecord = new(CampaignTime.Now, 0, attackerSideWarExhaustion, 1, defenderSideWarExhaustion, attackerSide, defenderSide, yieldsDiminishingReturns);
                return new(attackerSideWarExhaustion, defenderSideWarExhaustion, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
            }
        }

        private WarExhaustionRecord GetDivineWarExhaustionDelta(Kingdoms kingdoms, int targetWarExhaustion, out DivineInterventionRecord divineInterventionRecord)
        {
            var currentWarExhaustion = GetWarExhaustion(kingdoms.Kingdom1, kingdoms.Kingdom2);
            var warExhaustionValueDelta = targetWarExhaustion - currentWarExhaustion;
            var hasActiveQuest = !IsValidQuestState(kingdoms.Kingdom1, kingdoms.Kingdom2);

            if (kingdoms.ReversedKeyOrder)
            {
                divineInterventionRecord = new(CampaignTime.Now, 0, 0f, 1, warExhaustionValueDelta, kingdoms.Kingdom2.Name, kingdoms.Kingdom1.Name);
                return new(0f, warExhaustionValueDelta, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
            }
            else
            {
                divineInterventionRecord = new(CampaignTime.Now, 1, warExhaustionValueDelta, 0, 0f, kingdoms.Kingdom1.Name, kingdoms.Kingdom2.Name);
                return new(warExhaustionValueDelta, 0f, hasActiveQuest: hasActiveQuest, considerRangeLimits: false);
            }
        }

        private float GetDiminishingReturnsFactor<TObj, TRec>(Kingdoms kingdoms, TObj eventRelatedObject, out bool yieldsDiminishingReturns) where TObj : MBObjectBase where TRec : WarExhaustionEventRecord =>
            GetDiminishingReturnsFactor<TObj, TRec>(eventRelatedObject, _warExhaustionEventRecords.TryGetValue(kingdoms.Key!, out var currentRecords) ? currentRecords : new(), out yieldsDiminishingReturns);

        private static float GetDiminishingReturnsFactor<TObj, TRec>(TObj eventRelatedObject, List<WarExhaustionEventRecord> warExhaustionEventRecords, out bool yieldsDiminishingReturns) where TObj : MBObjectBase where TRec : WarExhaustionEventRecord
        {
            int numberOfPreviousOccurrences = Settings.Instance!.EnableDiminishingReturns ? eventRelatedObject switch
            {
                Hero hero when typeof(TRec) == typeof(HeroImprisonedRecord) => warExhaustionEventRecords.OfType<HeroImprisonedRecord>().Where(r => r.Hero == hero).Count(),
                Hero hero when typeof(TRec) == typeof(HeroPerishedRecord) => warExhaustionEventRecords.OfType<HeroPerishedRecord>().Where(r => r.Hero.Clan == hero.Clan).Count(),
                Kingdom kingdom when typeof(TRec) == typeof(OccupiedRecord) => warExhaustionEventRecords.OfType<OccupiedRecord>().Where(r => (r.Faction1Effected ? r.Faction1 : r.Faction2) == kingdom).Count(),
                Village village when typeof(TRec) == typeof(RaidRecord) => warExhaustionEventRecords.OfType<RaidRecord>().Where(r => r.RaidedVillage == village).Count(),
                Settlement settlement when typeof(TRec) == typeof(SiegeRecord) => warExhaustionEventRecords.OfType<SiegeRecord>().Where(r => r.Settlement == settlement).Count(),
                _ => 0
            } : 0;
            yieldsDiminishingReturns = numberOfPreviousOccurrences > 0;
            return GetDiminishingReturnsFactor(numberOfPreviousOccurrences);
        }

        private static float GetDiminishingReturnsFactor(int numberOfPreviousOccurrences) => (float) (1 / Math.Pow(2, Math.Max(numberOfPreviousOccurrences, 0)));

        public void UpdateDailyWarExhaustionForAllKingdoms()
        {
            List<string> listKeys = new();
            foreach (var kingdom in KingdomExtensions.AllActiveKingdoms)
            {
                var enemyKingdoms = FactionManager.GetEnemyKingdoms(kingdom);
                foreach (var enemyKingdom in enemyKingdoms)
                {
                    var key = CreateKey(kingdom, enemyKingdom, out var kingdoms);
                    if (kingdoms != null && key != null && !listKeys.Contains(key))
                    {
                        var warStartDate = kingdom.GetStanceWith(enemyKingdom).WarStartDate;
                        var daysElapsed = warStartDate.ElapsedDaysUntilNow;
                        if (daysElapsed >= 1.0f) AddDailyWarExhaustion(kingdoms, warStartDate);
                        listKeys.Add(key);
                    }
                }
            }
        }

        private ImportanceEnum GetHeroImportanceForFaction(Hero hero, IFaction faction)
        {
            if (hero.MapFaction != null && hero.MapFaction == faction)
            {
                if (hero.Clan is Clan clan)
                {
                    if (hero.IsFactionLeader)
                        return ImportanceEnum.MatterOfLifeAndDeath;
                    if (faction.Leader.Clan is Clan factionLeaderClan && factionLeaderClan == clan)
                        return hero.IsNoncombatant ? ImportanceEnum.Important : ImportanceEnum.ExtremelyImportant;

                    int inportance = Math.Min(clan.Tier, 6) - (clan.IsUnderMercenaryService ? 5 : 3) + (hero.IsCommander ? 1 : 0) + (clan.Leader == hero ? 3 : 0) - (hero.IsLord ? 0 : 3) - (hero.IsNoncombatant ? 3 : 0);
                    return (ImportanceEnum) MBMath.ClampInt(inportance, 1, (int) ImportanceEnum.ExtremelyImportant);
                }

                return ImportanceEnum.Unimportant;
            }
            else if (hero.Clan is Clan clan && clan == faction)
                return clan.Leader == hero ? ImportanceEnum.MatterOfLifeAndDeath : hero.IsLord ? ImportanceEnum.ExtremelyImportant : hero.IsCommander ? ImportanceEnum.ReasonablyImportant : ImportanceEnum.Unimportant;

            return ImportanceEnum.Zero;
        }

        private float GetImportanceFactor(ImportanceEnum importance) => importance switch
        {
            ImportanceEnum.Zero => 0f,
            < ImportanceEnum.ReasonablyImportant => 1f - (0.25f * ((int) ImportanceEnum.ReasonablyImportant - (int) importance)),
            ImportanceEnum.ReasonablyImportant => 1f,
            <= ImportanceEnum.MatterOfLifeAndDeath => 1f + (0.2f * ((int) importance - (int) ImportanceEnum.ReasonablyImportant)),
            _ => throw new ArgumentOutOfRangeException(nameof(importance)),
        };

        /* INITIALIZATION */
        private void TryInitWarExhaustion(out Dictionary<string, WarExhaustionRecord> warExhaustionScores, out Dictionary<string, WarExhaustionRecord> warExhaustionRates, out Dictionary<string, List<WarExhaustionEventRecord>> warExhaustionEventRecords)
        {
            warExhaustionScores = new Dictionary<string, WarExhaustionRecord>();
            warExhaustionRates = new Dictionary<string, WarExhaustionRecord>();
            warExhaustionEventRecords = new Dictionary<string, List<WarExhaustionEventRecord>>();
            foreach (var kingdom in KingdomExtensions.AllActiveKingdoms)
            {
                var kingdomLastOccupationDate = GetLastOccupationDate(kingdom, out var kingdomLastOccupatorFaction);

                var enemyKingdoms = FactionManager.GetEnemyKingdoms(kingdom).Where(k => !k.IsEliminated).ToList();
                foreach (var enemyKingdom in enemyKingdoms)
                {
                    var key = CreateKey(kingdom, enemyKingdom, out var kingdoms);
                    if (key is not null && kingdoms is not null && !warExhaustionScores.ContainsKey(key))
                    {
                        //Variables
                        var enemyKingdomLastOccupationDate = GetLastOccupationDate(enemyKingdom, out var enemyKingdomLastOccupatorFaction);
                        var stance = kingdom.GetStanceWith(enemyKingdom);
                        CampaignTime warStartDate = stance.WarStartDate;
                        CalculateWarExhaustionMultiplier(kingdoms, out var multiplier1, out var multiplier2);
                        var eventRecs = new List<WarExhaustionEventRecord>();
                        var hasActiveQuest = !IsValidQuestState(kingdom, enemyKingdom);

                        //Exhaustion entries
                        //FIXME: Add proper logics for diminishing returns
                        var dailyWarExhaustion = AccountForDailyExhaustion(kingdoms, warStartDate, kingdomLastOccupationDate, enemyKingdomLastOccupationDate, eventRecs, hasActiveQuest);
                        var casualtiesWarExhaustion = AccountForCasualtyExhaustion(kingdoms, stance, warStartDate, multiplier1, multiplier2, eventRecs, hasActiveQuest);
                        var raidsWarExhaustion = AccountForRaidExhaustion(kingdoms, stance, warStartDate, multiplier1, multiplier2, eventRecs, hasActiveQuest);
                        var siegesWarExhaustion = AccountForSiegeExhaustion(kingdoms, stance, warStartDate, multiplier1, multiplier2, eventRecs, hasActiveQuest);
                        var heroesImprisonedWarExhaustion = AccountForImprisonedExhaustion(kingdoms, stance, warStartDate, multiplier1, multiplier2, eventRecs, hasActiveQuest);
                        var heroesPerishedWarExhaustion = AccountForPerishedExhaustion(kingdoms, stance, warStartDate, multiplier1, multiplier2, eventRecs, hasActiveQuest);
                        var occupiedWarExhaustion = AccountForOccupationExhaustion(kingdoms, warStartDate, kingdomLastOccupationDate, kingdomLastOccupatorFaction, enemyKingdomLastOccupationDate, enemyKingdomLastOccupatorFaction, eventRecs, hasActiveQuest);

                        //Result
                        warExhaustionScores[key] = dailyWarExhaustion + casualtiesWarExhaustion + raidsWarExhaustion + siegesWarExhaustion + heroesImprisonedWarExhaustion + heroesPerishedWarExhaustion + occupiedWarExhaustion;
                        warExhaustionRates[key] = new(multiplier1, multiplier2, considerRangeLimits: false);
                        warExhaustionEventRecords[key] = eventRecs;
                    }
                }
            }
        }

        private static WarExhaustionRecord AccountForDailyExhaustion(Kingdoms kingdoms, CampaignTime warStartDate, CampaignTime kingdomLastOccupationDate, CampaignTime enemyKingdomLastOccupationDate, List<WarExhaustionEventRecord> eventRecs, bool hasActiveQuest)
        {
            if (kingdomLastOccupationDate == CampaignTime.Never && enemyKingdomLastOccupationDate == CampaignTime.Never)
            {
                var daysElapsed = (int) warStartDate.ElapsedDaysUntilNow;
                float warExhaustionPerDay = Settings.Instance!.WarExhaustionPerDay;
                var totalDailyExhaustion = warExhaustionPerDay * daysElapsed;
                eventRecs.Add(new DailyRecord(warStartDate, daysElapsed, totalDailyExhaustion, daysElapsed, totalDailyExhaustion));
                return new(totalDailyExhaustion, totalDailyExhaustion, hasActiveQuest: hasActiveQuest);
            }
            else
            {
                var dailyWarExhaustion = new WarExhaustionRecord(0f, 0f, hasActiveQuest: hasActiveQuest);
                GetWarExhaustionPerDay(out var warExhaustionPerDay, out var warExhaustionPerDayOccupied);
                var milestones = new List<CampaignTime>
                            {
                                kingdomLastOccupationDate,
                                enemyKingdomLastOccupationDate
                            };
                var warPeriods = SplitCampaignTimePeriod(warStartDate, CampaignTime.Now, milestones);
                foreach (var (periodStart, periodEnd) in warPeriods)
                {
                    var daysElapsed = (int) CampaignTimeHelper.ElapsedDaysBetween(periodEnd, warStartDate) - (int) CampaignTimeHelper.ElapsedDaysBetween(periodStart, warStartDate);
                    if (daysElapsed > 0)
                    {
                        var faction1IsOccupied = (kingdoms.ReversedKeyOrder ? enemyKingdomLastOccupationDate : kingdomLastOccupationDate) <= periodStart;
                        var faction2IsOccupied = (kingdoms.ReversedKeyOrder ? kingdomLastOccupationDate : enemyKingdomLastOccupationDate) <= periodStart;
                        var faction1WarExhaustion = daysElapsed * (faction1IsOccupied ? warExhaustionPerDayOccupied : warExhaustionPerDay);
                        var faction2WarExhaustion = daysElapsed * (faction2IsOccupied ? warExhaustionPerDayOccupied : warExhaustionPerDay);
                        dailyWarExhaustion += new WarExhaustionRecord(faction1WarExhaustion, faction2WarExhaustion, hasActiveQuest: hasActiveQuest);
                        eventRecs.Add(new DailyRecord(periodStart, daysElapsed, faction1WarExhaustion, daysElapsed, faction2WarExhaustion, faction1IsOccupied, faction2IsOccupied));
                    }
                }
                return dailyWarExhaustion;
            }
        }

        private static WarExhaustionRecord AccountForCasualtyExhaustion(Kingdoms kingdoms, StanceLink stance, CampaignTime warStartDate, float multiplier1, float multiplier2, List<WarExhaustionEventRecord> eventRecs, bool hasActiveQuest)
        {
            var valueFaction1 = kingdoms.ReversedKeyOrder ? stance.GetCasualties(kingdoms.Kingdom2) : stance.GetCasualties(kingdoms.Kingdom1);
            var valueFaction2 = kingdoms.ReversedKeyOrder ? stance.GetCasualties(kingdoms.Kingdom1) : stance.GetCasualties(kingdoms.Kingdom2);
            var warExhaustionPerCasualty = Settings.Instance!.WarExhaustionPerCasualty;
            var exhaustionFaction1 = warExhaustionPerCasualty * valueFaction1 * multiplier1;
            var exhaustionFaction2 = warExhaustionPerCasualty * valueFaction2 * multiplier2;
            eventRecs.Add(new SummaryCasualtyRecord(warStartDate, CampaignTime.Now, valueFaction1, exhaustionFaction1, valueFaction2, exhaustionFaction2));
            return new WarExhaustionRecord(exhaustionFaction1, exhaustionFaction2, hasActiveQuest: hasActiveQuest);
        }

        private static WarExhaustionRecord AccountForRaidExhaustion(Kingdoms kingdoms, StanceLink stance, CampaignTime warStartDate, float multiplier1, float multiplier2, List<WarExhaustionEventRecord> eventRecs, bool hasActiveQuest)
        {
            var warExhaustionPerRaid = Settings.Instance!.WarExhaustionPerRaid;
            var exhaustionFaction1 = 0f;
            var exhaustionFaction2 = 0f;
            for (int index = 0; index < Campaign.Current.LogEntryHistory.GameActionLogs.Count; index++)
            {
                LogEntry gameActionLog = Campaign.Current.LogEntryHistory.GameActionLogs[index];
                if (IsLogInTimeRange(gameActionLog, warStartDate) && gameActionLog is VillageStateChangedLogEntry stateChangedLogEntry2 && stateChangedLogEntry2.IsRelatedToWar(stance, out IFaction effector, out IFaction effected))
                {
                    var valueFaction1 = kingdoms.ReversedKeyOrder ? IsFactionEffected(kingdoms.Kingdom2, effected) : IsFactionEffected(kingdoms.Kingdom1, effected);
                    var valueFaction2 = kingdoms.ReversedKeyOrder ? IsFactionEffected(kingdoms.Kingdom1, effected) : IsFactionEffected(kingdoms.Kingdom2, effected);
                    var eventExhaustionFaction1 = warExhaustionPerRaid * valueFaction1 * multiplier1;
                    var eventExhaustionFaction2 = warExhaustionPerRaid * valueFaction2 * multiplier2;
                    exhaustionFaction1 += eventExhaustionFaction1;
                    exhaustionFaction2 += eventExhaustionFaction2;
                    var raiderPartyName = stateChangedLogEntry2.RaidLeader?.Name ?? stateChangedLogEntry2.RaiderPartyMapFaction?.Name ?? effector.Name;
                    eventRecs.Add(new RaidRecord(gameActionLog.GameTime, stateChangedLogEntry2.Village, valueFaction1, eventExhaustionFaction1, valueFaction2, eventExhaustionFaction2, raiderPartyName!));
                }
            }
            return new WarExhaustionRecord(exhaustionFaction1, exhaustionFaction2, hasActiveQuest: hasActiveQuest);
        }

        private static WarExhaustionRecord AccountForSiegeExhaustion(Kingdoms kingdoms, StanceLink stance, CampaignTime warStartDate, float multiplier1, float multiplier2, List<WarExhaustionEventRecord> eventRecs, bool hasActiveQuest)
        {
            var warExhaustionPerSiege = Settings.Instance!.WarExhaustionPerSiege;
            var exhaustionFaction1 = 0f;
            var exhaustionFaction2 = 0f;
            for (int index = 0; index < Campaign.Current.LogEntryHistory.GameActionLogs.Count; index++)
            {
                LogEntry gameActionLog = Campaign.Current.LogEntryHistory.GameActionLogs[index];
                if (IsLogInTimeRange(gameActionLog, warStartDate) && gameActionLog is ChangeSettlementOwnerLogEntry settlementOwnerLogEntry2 && settlementOwnerLogEntry2.IsRelatedToWar(stance, out IFaction effector, out IFaction effected))
                {
                    var valueFaction1 = kingdoms.ReversedKeyOrder ? IsFactionEffected(kingdoms.Kingdom2, effected) : IsFactionEffected(kingdoms.Kingdom1, effected);
                    var valueFaction2 = kingdoms.ReversedKeyOrder ? IsFactionEffected(kingdoms.Kingdom1, effected) : IsFactionEffected(kingdoms.Kingdom2, effected);
                    var importanceFactor = settlementOwnerLogEntry2.Settlement.IsCastle ? 0.5f : 1f;
                    var eventExhaustionFaction1 = warExhaustionPerSiege * valueFaction1 * multiplier1 * importanceFactor;
                    var eventExhaustionFaction2 = warExhaustionPerSiege * valueFaction2 * multiplier2 * importanceFactor;
                    exhaustionFaction1 += eventExhaustionFaction1;
                    exhaustionFaction2 += eventExhaustionFaction2;
                    var settlementConquerorName = settlementOwnerLogEntry2.NewClan?.Name ?? effector.Name;
                    var settlementDefenderName = effected.Name;
                    eventRecs.Add(new SiegeRecord(gameActionLog.GameTime, settlementOwnerLogEntry2.Settlement,
                                                  valueFaction1, eventExhaustionFaction1,
                                                  valueFaction2, eventExhaustionFaction2,
                                                  IsFactionEffected(kingdoms.Kingdom1, effected) == 1 ? settlementDefenderName : settlementConquerorName,
                                                  IsFactionEffected(kingdoms.Kingdom2, effected) == 1 ? settlementDefenderName : settlementConquerorName));
                }
            }
            return new WarExhaustionRecord(exhaustionFaction1, exhaustionFaction2, hasActiveQuest: hasActiveQuest);
        }

        private WarExhaustionRecord AccountForImprisonedExhaustion(Kingdoms kingdoms, StanceLink stance, CampaignTime warStartDate, float multiplier1, float multiplier2, List<WarExhaustionEventRecord> eventRecs, bool hasActiveQuest)
        {
            var warExhaustionPerImprisonment = Settings.Instance!.WarExhaustionPerImprisonment;
            var exhaustionFaction1 = 0f;
            var exhaustionFaction2 = 0f;
            for (int index = 0; index < Campaign.Current.LogEntryHistory.GameActionLogs.Count; index++)
            {
                LogEntry gameActionLog = Campaign.Current.LogEntryHistory.GameActionLogs[index];
                if (IsLogInTimeRange(gameActionLog, warStartDate) && gameActionLog is TakePrisonerLogEntry takePrisonerLogEntry2 && takePrisonerLogEntry2.IsRelatedToWar(stance, out IFaction effector, out IFaction effected))
                {
                    var valueFaction1 = kingdoms.ReversedKeyOrder ? IsFactionEffected(kingdoms.Kingdom2, effected) : IsFactionEffected(kingdoms.Kingdom1, effected);
                    var valueFaction2 = kingdoms.ReversedKeyOrder ? IsFactionEffected(kingdoms.Kingdom1, effected) : IsFactionEffected(kingdoms.Kingdom2, effected);
                    var importanceFactor = GetImportanceFactor(GetHeroImportanceForFaction(takePrisonerLogEntry2.Prisoner, effected));
                    var eventExhaustionFaction1 = warExhaustionPerImprisonment * valueFaction1 * multiplier1 * importanceFactor;
                    var eventExhaustionFaction2 = warExhaustionPerImprisonment * valueFaction2 * multiplier2 * importanceFactor;
                    exhaustionFaction1 += eventExhaustionFaction1;
                    exhaustionFaction2 += eventExhaustionFaction2;
                    var capturerName = takePrisonerLogEntry2.CapturerHero?.Name ?? takePrisonerLogEntry2.CapturerSettlement?.Name ?? effector.Name;
                    eventRecs.Add(new HeroImprisonedRecord(gameActionLog.GameTime, takePrisonerLogEntry2.Prisoner,
                                                           valueFaction1, eventExhaustionFaction1,
                                                           valueFaction2, eventExhaustionFaction2,
                                                           capturerName));
                }
            }
            return new WarExhaustionRecord(exhaustionFaction1, exhaustionFaction2, hasActiveQuest: hasActiveQuest);
        }

        private WarExhaustionRecord AccountForPerishedExhaustion(Kingdoms kingdoms, StanceLink stance, CampaignTime warStartDate, float multiplier1, float multiplier2, List<WarExhaustionEventRecord> eventRecs, bool hasActiveQuest)
        {
            var warExhaustionPerDeath = Settings.Instance!.WarExhaustionPerDeath;
            var exhaustionFaction1 = 0f;
            var exhaustionFaction2 = 0f;
            for (int index = 0; index < Campaign.Current.LogEntryHistory.GameActionLogs.Count; index++)
            {
                LogEntry gameActionLog = Campaign.Current.LogEntryHistory.GameActionLogs[index];
                if (IsLogInTimeRange(gameActionLog, warStartDate) && gameActionLog is CharacterKilledLogEntry characterKilledLogEntry2 && characterKilledLogEntry2.IsRelatedToWar(stance, out IFaction effector, out IFaction effected))
                {
                    var valueFaction1 = kingdoms.ReversedKeyOrder ? IsFactionEffected(kingdoms.Kingdom2, effected) : IsFactionEffected(kingdoms.Kingdom1, effected);
                    var valueFaction2 = kingdoms.ReversedKeyOrder ? IsFactionEffected(kingdoms.Kingdom1, effected) : IsFactionEffected(kingdoms.Kingdom2, effected);
                    var importanceFactor = GetImportanceFactor(GetHeroImportanceForFaction(characterKilledLogEntry2.Victim, effected));
                    var eventExhaustionFaction1 = warExhaustionPerDeath * valueFaction1 * multiplier1 * importanceFactor;
                    var eventExhaustionFaction2 = warExhaustionPerDeath * valueFaction2 * multiplier2 * importanceFactor;
                    exhaustionFaction1 += eventExhaustionFaction1;
                    exhaustionFaction2 += eventExhaustionFaction2;
                    var killerName = characterKilledLogEntry2.Killer?.Name ?? effector.Name;
                    eventRecs.Add(new HeroPerishedRecord(gameActionLog.GameTime, characterKilledLogEntry2.Victim,
                                                           valueFaction1, eventExhaustionFaction1,
                                                           valueFaction2, eventExhaustionFaction2,
                                                           killerName, FieldAccessHelper.CharacterKilledLogEntryActionDetailByRef(characterKilledLogEntry2)));
                }
            }
            return new WarExhaustionRecord(exhaustionFaction1, exhaustionFaction2, hasActiveQuest: hasActiveQuest);
        }

        private static WarExhaustionRecord AccountForOccupationExhaustion(Kingdoms kingdoms, CampaignTime warStartDate, CampaignTime kingdomLastOccupationDate, IFaction? kingdomLastOccupatorFaction, CampaignTime enemyKingdomLastOccupationDate, IFaction? enemyKingdomLastOccupatorFaction,
                                                                          List<WarExhaustionEventRecord> eventRecs, bool hasActiveQuest)
        {
            var occupiedWarExhaustion = new WarExhaustionRecord(0f, 0f, hasActiveQuest: hasActiveQuest);
            AddOccupationEventForFaction(kingdoms.Kingdom1, kingdomLastOccupationDate, kingdomLastOccupatorFaction, kingdoms, warStartDate, eventRecs, ref occupiedWarExhaustion, hasActiveQuest);
            AddOccupationEventForFaction(kingdoms.Kingdom2, enemyKingdomLastOccupationDate, enemyKingdomLastOccupatorFaction, kingdoms, warStartDate, eventRecs, ref occupiedWarExhaustion, hasActiveQuest);
            return occupiedWarExhaustion;
        }

        private static void AddOccupationEventForFaction(IFaction faction, CampaignTime LastOccupationDate, IFaction? lastOccupatorFaction, Kingdoms kingdoms, CampaignTime warStartDate, List<WarExhaustionEventRecord> eventRecs, ref WarExhaustionRecord occupiedWarExhaustion, bool hasActiveQuest)
        {
            if (!kingdoms.Contains(faction))
                return;

            if (LastOccupationDate != CampaignTime.Never && LastOccupationDate > warStartDate && kingdoms.Contains(lastOccupatorFaction))
            {
                int valueFaction1, valueFaction2;
                Kingdom faction1, faction2;
                if (kingdoms.ReversedKeyOrder && faction == kingdoms.Kingdom1)
                {
                    valueFaction1 = 0;
                    valueFaction2 = 1;
                    faction1 = kingdoms.Kingdom2;
                    faction2 = kingdoms.Kingdom1;
                }
                else
                {
                    valueFaction1 = 1;
                    valueFaction2 = 0;
                    faction1 = kingdoms.Kingdom1;
                    faction2 = kingdoms.Kingdom2;
                }
                var warExhaustionWhenOccupied = Settings.Instance!.WarExhaustionWhenOccupied;
                var exhaustionFaction1 = valueFaction1 * warExhaustionWhenOccupied;
                var exhaustionFaction2 = valueFaction2 * warExhaustionWhenOccupied;
                occupiedWarExhaustion += new WarExhaustionRecord(exhaustionFaction1, exhaustionFaction2, hasActiveQuest: hasActiveQuest);
                eventRecs.Add(new OccupiedRecord(LastOccupationDate, valueFaction1, exhaustionFaction1, valueFaction2, exhaustionFaction2, faction1, faction2));
            }
        }

        private CampaignTime GetLastOccupationDate(Kingdom kingdom, out IFaction? effector)
        {
            if (!kingdom.Fiefs.Any())
            {
                for (int index = Campaign.Current.LogEntryHistory.GameActionLogs.Count - 1; index >= 0; --index)
                {
                    LogEntry gameActionLog = Campaign.Current.LogEntryHistory.GameActionLogs[index];
                    if (gameActionLog is ChangeSettlementOwnerLogEntry changeSettlementOwnerLogEntry2 && changeSettlementOwnerLogEntry2.PreviousClan?.MapFaction is Kingdom previousKingom && previousKingom == kingdom && (effector = changeSettlementOwnerLogEntry2.NewClan?.MapFaction) != kingdom)
                    {
                        return changeSettlementOwnerLogEntry2.GameTime;
                    }
                }
            }

            effector = default;
            return CampaignTime.Never;
        }

        private static List<(CampaignTime PeriodStart, CampaignTime PeriodEnd)> SplitCampaignTimePeriod(CampaignTime dateTimeFrom, CampaignTime dateTimeTo, List<CampaignTime> milestones)
        {
            var splitDateTimes = milestones.Where(d => d > dateTimeFrom && d < dateTimeTo).ToList();
            return Enumerable.Range(0, splitDateTimes.Count + 1).Select(i => (i == 0 ? dateTimeFrom : splitDateTimes[i - 1], i == splitDateTimes.Count ? dateTimeTo : splitDateTimes[i] - CampaignTime.Minutes(1))).ToList();
        }

        private static int IsFactionEffected(IFaction faction, IFaction effected) => faction == effected ? 1 : 0;

        private static bool IsLogInTimeRange(LogEntry entry, CampaignTime time) => entry.GameTime >= time;

        internal void OnAfterSaveLoaded()
        {
            //Fill the data on first load
            if (RequiresRefill(_warExhaustionScores) || RequiresRefill(_warExhaustionRates) || RequiresRefill(_warExhaustionEventRecords))
            {
                //It is possible to process those separately, but then we may have potential discrepancies, so we will do it in conjunction.
                TryInitWarExhaustion(out _warExhaustionScores, out _warExhaustionRates, out _warExhaustionEventRecords);
            }

            static bool RequiresRefill<T>(IEnumerable<T>? source) => source is null || source.IsEmpty();
        }

        /* RESOLUTION */
        public void ProcessWarExhaustion(ILogger logger)
        {
            List<string> keyList = new();
            foreach (var kingdom in KingdomExtensions.AllActiveKingdoms.ToList())
            {
                ConsiderPeaceActions(logger, kingdom, keyList);
            }
        }

        private void ConsiderPeaceActions(ILogger logger, Kingdom kingdom, List<string> keyList)
        {
            foreach (var enemyKingdom in FactionManager.GetEnemyKingdoms(kingdom).ToList())
            {
                var key = CreateKey(kingdom, enemyKingdom, out var kingdoms);
                if (key is null || kingdoms is null || enemyKingdom.IsEliminated || !_warExhaustionScores.TryGetValue(key, out _) || keyList.Contains(key))
                    continue;

                var warResult = GetWarResult(kingdom, enemyKingdom);
                if ((warResult == WarResult.Loss || warResult == WarResult.Tie) && IsValidQuestState(kingdom, enemyKingdom))
                {
                    keyList.Add(key);
                    {
                        logger.LogTrace($"[{CampaignTime.Now}] {kingdom.Name}, due to max war exhaustion, will peace out with {enemyKingdom.Name}.");
                        KingdomPeaceAction.ApplyPeace(kingdom, enemyKingdom);
                    }
                }
            }
        }
    }
}