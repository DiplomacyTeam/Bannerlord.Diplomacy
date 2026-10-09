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
            var refusal = GetFactionRefusal(faction) ?? GetClanRefusal(clan, faction) ?? (checkCooldown ? GetCooldownRefusal(clan) : null);
            reason = refusal ?? TextObject.GetEmpty();
            return refusal is null;
        }

        private static TextObject? GetFactionRefusal(RebelFaction faction)
        {
            if (!Settings.Instance!.EnablePlayerFactionRecruitment)
                return new("{=FRrecOff}Player faction recruitment is disabled.");
            if (faction.SponsorClan != Clan.PlayerClan || faction.SponsorClan.Leader != Hero.MainHero)
                return new("{=FRnotLed}You must lead this faction to recruit supporters.");
            var kingdomFactions = RebelFactionManager.GetRebelFaction(faction.ParentKingdom);
            if (!kingdomFactions.Contains(faction) || faction.ParentKingdom.IsEliminated || kingdomFactions.Any(f => f.AtWar))
                return new("{=FRnotAct}Recruitment is only available while your faction is gathering support.");
            return null;
        }

        private static TextObject? GetClanRefusal(Clan clan, RebelFaction faction)
        {
            if (clan.Leader is null)
                return new("{=FRnoLeader}This clan has no leader to approach.");
            if (clan.Leader.IsPrisoner)
                return new("{=FRprisoner}This clan's leader is a prisoner and cannot pledge support.");
            if (!clan.Leader.IsActive)
                return new("{=FRunavailable}This clan's leader is currently unavailable.");
            if (clan.IsMinorFaction)
                return new("{=FRminor}Minor factions cannot be recruited into a rebel faction.");
            return JoinFactionAction.CanApply(clan, faction, out var joinReason) ? null : joinReason;
        }

        private static TextObject? GetCooldownRefusal(Clan clan)
        {
            if (FactionRecruitmentManager.Instance!.HasPendingAttempt(clan))
                return new("{=FRpending}You are already making your case to this clan.");
            var days = FactionRecruitmentManager.Instance!.GetCooldownDaysRemaining(clan);
            return days > 0
                ? new TextObject("{=FRonCool}You must wait {DAYS} more days before approaching this clan again.").SetTextVariable("DAYS", days)
                : null;
        }

        public static float GetSupport(Clan clan, RebelFaction faction)
            => RebelFactionScoringModel.GetDemandScore(clan, faction).ResultNumber;

        public static bool CanSeekSupport(Clan clan, RebelFaction faction, out TextObject reason)
            => CanRecruit(clan, faction, out reason) && CanSeekSupport(GetSupport(clan, faction), out reason);

        /// <summary>
        /// For a clan that already passed <see cref="CanRecruit"/>, so callers that need the score can compute it once.
        /// </summary>
        public static bool CanSeekSupport(float score, out TextObject reason)
        {
            reason = TextObject.GetEmpty();
            return score >= RebelFactionScoringModel.RequiredScore || CanPersuade(score, out reason);
        }

        public static bool CanPersuade(Clan clan, RebelFaction faction, out TextObject reason)
            => CanRecruit(clan, faction, out reason) && CanPersuade(GetSupport(clan, faction), out reason);

        private static bool CanPersuade(float score, out TextObject reason)
        {
            reason = TextObject.GetEmpty();
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