using Diplomacy.CivilWar.Factions;
using Diplomacy.CivilWar.Scoring;
using Diplomacy.Costs;

using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar.Actions
{
    internal static class RecruitFactionSupportAction
    {
        public static bool CanRecruit(Clan clan, RebelFaction faction, out TextObject reason, bool checkCooldown = true)
        {
            reason = TextObject.GetEmpty();
            if (!Settings.Instance!.EnablePlayerFactionRecruitment)
                reason = new("{=FRrecOff}Player faction recruitment is disabled.");
            else if (faction.SponsorClan != Clan.PlayerClan || faction.SponsorClan.Leader != Hero.MainHero)
                reason = new("{=FRnotLed}You must lead this faction to recruit supporters.");
            else if (!RebelFactionManager.GetRebelFaction(faction.ParentKingdom).Contains(faction)
                     || faction.ParentKingdom.IsEliminated || faction.AtWar
                     || RebelFactionManager.GetRebelFaction(faction.ParentKingdom).Any(f => f.AtWar))
                reason = new("{=FRnotAct}Recruitment is only available while your faction is gathering support.");
            else if (clan.Leader is null || !clan.Leader.IsActive || clan.Leader.IsPrisoner || clan.IsMinorFaction
                     || !JoinFactionAction.CanApply(clan, faction, out _))
                reason = new("{=FRnotCan}This clan cannot join your faction. Only available clan leaders in your kingdom can be recruited.");
            else if (checkCooldown && FactionRecruitmentManager.Instance!.HasPendingAttempt(clan))
                reason = new("{=FRpending}You are already making your case to this clan.");
            else if (checkCooldown && FactionRecruitmentManager.Instance!.GetCooldownDaysRemaining(clan) is var days && days > 0)
                reason = new TextObject("{=FRonCool}You must wait {DAYS} more days before approaching this clan again.").SetTextVariable("DAYS", days);
            else
                return true;
            return false;
        }

        public static float GetSupport(Clan clan, RebelFaction faction)
            => RebelFactionScoringModel.GetDemandScore(clan, faction).ResultNumber;

        public static bool CanPersuade(Clan clan, RebelFaction faction, out TextObject reason)
        {
            if (!CanRecruit(clan, faction, out reason))
                return false;
            var score = GetSupport(clan, faction);
            if (score >= RebelFactionScoringModel.RequiredScore)
            {
                reason = new("{=FRalready}This clan already supports your demand. Ask for its support directly.");
                return false;
            }
            if (score + Settings.Instance!.FactionRecruitmentPersuasionBonus < RebelFactionScoringModel.RequiredScore)
            {
                reason = new("{=FRopposed}This clan is too opposed to your demand. Improve its reasons to support you before trying persuasion.");
                return false;
            }
            if (!new InfluenceCost(Clan.PlayerClan, Settings.Instance!.FactionRecruitmentInfluenceCost).CanPayCost())
            {
                reason = new TextObject(StringConstants.NotEnoughInfluence);
                return false;
            }
            return true;
        }

        public static bool TryAskForSupport(Clan clan, RebelFaction faction, out TextObject reason)
        {
            if (!CanRecruit(clan, faction, out reason))
                return false;
            if (GetSupport(clan, faction) < RebelFactionScoringModel.RequiredScore)
            {
                reason = new("{=FRhesitnt}I am not ready to support your demand. You will have to convince me.");
                return false;
            }
            var record = StartAttempt(clan, faction, 0f);
            JoinFactionAction.Apply(clan, faction);
            record.Resolve(true);
            return true;
        }

        public static FactionRecruitmentRecord? TryBeginPersuasion(Clan clan, RebelFaction faction, out TextObject reason)
        {
            if (!CanPersuade(clan, faction, out reason))
                return null;
            new InfluenceCost(Clan.PlayerClan, Settings.Instance!.FactionRecruitmentInfluenceCost).ApplyCost();
            return StartAttempt(clan, faction, Settings.Instance!.FactionRecruitmentPersuasionBonus);
        }

        public static bool TryCompletePersuasion(FactionRecruitmentRecord record, bool persuaded, out TextObject reason)
        {
            reason = new("{=FRfailed}You have not convinced me. My clan will not join your faction.");
            if (!FactionRecruitmentManager.Instance!.IsPendingAttempt(record))
                return false;
            record.Resolve(false);
            if (!CanRecruit(record.Clan, record.Faction, out var refusal, checkCooldown: false)
                || record.Clan.Leader != record.Leader)
            {
                reason = refusal.IsEmpty() ? new TextObject("{=FRchanged}Circumstances have changed. This clan can no longer be recruited.") : refusal;
                return false;
            }
            if (!persuaded || GetSupport(record.Clan, record.Faction) + record.Bonus < RebelFactionScoringModel.RequiredScore)
                return false;
            JoinFactionAction.Apply(record.Clan, record.Faction);
            record.Resolve(true);
            return true;
        }

        private static FactionRecruitmentRecord StartAttempt(Clan clan, RebelFaction faction, float bonus)
            => FactionRecruitmentManager.Instance!.StartAttempt(clan, faction, bonus,
                Settings.Instance!.FactionRecruitmentCooldownInDays, Settings.Instance!.FactionRecruitmentPledgeInDays);
    }
}