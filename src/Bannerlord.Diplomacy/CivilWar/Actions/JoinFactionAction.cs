using Diplomacy.CivilWar.Factions;
using Diplomacy.CivilWar.Scoring;
using Diplomacy.Extensions;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar.Actions
{
    public class JoinFactionAction
    {
        public static void Apply(Clan clan, RebelFaction rebelFaction)
        {
            if (rebelFaction.Clans.Contains(clan))
                return;
            rebelFaction.AddClan(clan);

            // announce new supporters of factions in the player's kingdom
            if (rebelFaction.ParentKingdom == Clan.PlayerClan.Kingdom && clan != Clan.PlayerClan)
                InformationManager.DisplayMessage(new InformationMessage(GetJoinedText(clan, rebelFaction).ToString()));
        }

        private static TextObject GetJoinedText(Clan clan, RebelFaction rebelFaction)
            => (rebelFaction.SponsorClan == Clan.PlayerClan
                    ? new TextObject("{=FRjoinYou}{CLAN} has joined your faction, {FACTION}.")
                    : new TextObject("{=FRjoinLog}{CLAN} has joined {FACTION}."))
                .SetTextVariable("CLAN", clan.Name)
                .SetTextVariable("FACTION", rebelFaction.Name);

        public static bool ShouldApply(Clan clan, RebelFaction rebelFaction)
        {
            if (!CanApply(clan, rebelFaction, out _))
                return false;
            // a clan pledged to the player's faction doesn't join another faction until the pledge ends
            if (FactionRecruitmentManager.Instance?.GetPledgedFaction(clan) is { } pledgedFaction && pledgedFaction != rebelFaction)
                return false;
            var score = RebelFactionScoringModel.GetDemandScore(clan, rebelFaction);
            return score.ResultNumber >= RebelFactionScoringModel.RequiredScore;
        }

        public static bool ShouldRemain(Clan clan, RebelFaction rebelFaction)
        {
            if (!rebelFaction.Clans.Contains(clan) || !IsEligibleMember(clan, rebelFaction))
                return false;
            return (FactionRecruitmentManager.Instance?.HasActivePledge(clan, rebelFaction) ?? false)
                   || RebelFactionScoringModel.GetDemandScore(clan, rebelFaction).ResultNumber >= RebelFactionScoringModel.RequiredScore;
        }

        /// <summary>
        /// Whether an existing member still meets the membership rules, regardless of its support score.
        /// </summary>
        internal static bool IsEligibleMember(Clan clan, RebelFaction rebelFaction)
            => !GetExceptions(clan, rebelFaction, existingMember: true).Any();

        public static bool CanApply(Clan clan, RebelFaction rebelFaction, out TextObject? reason)
        {
            reason = CanApply(clan, rebelFaction).FirstOrDefault();
            return reason is null;
        }

        /// <summary>
        /// Determines the set of reasons that the given faction cannot be joined by the clan.
        /// </summary>
        /// <param name="clan"></param>
        /// <param name="rebelFaction"></param>
        /// <returns>
        /// Set of reasons for which the faction cannot be joined, for use in recruitment tooltips.
        /// </returns>
        public static IEnumerable<TextObject> CanApply(Clan clan, RebelFaction rebelFaction)
            => GetExceptions(clan, rebelFaction, existingMember: false);

        private static IEnumerable<TextObject> GetExceptions(Clan clan, RebelFaction rebelFaction, bool existingMember)
        {
            // can only join a faction of a kingdom that they're in
            if (rebelFaction.ParentKingdom != clan.Kingdom)
            {
                yield return new TextObject("{=FRotherRealm}This clan belongs to a different kingdom.");
                yield break;
            }

            // can't join a faction during an active rebellion
            if (rebelFaction.AtWar)
            {
                yield return new TextObject("{=FRrebelling}Clans cannot join a faction after its rebellion has begun.");
            }

            foreach (var exception in GetClanExceptions(clan).Concat(GetMembershipExceptions(clan, rebelFaction, existingMember)))
                yield return exception;
        }

        private static IEnumerable<TextObject> GetClanExceptions(Clan clan)
        {
            // rebel kingdom members can't join factions
            if (clan.Kingdom.IsRebelKingdom())
            {
                yield return new TextObject("{=FRrebelRealm}Clans in a rebel kingdom cannot join another rebel faction.");
            }

            // ruling clan can't join factions
            if (clan == clan.Kingdom.RulingClan)
            {
                yield return new TextObject("{=FRruler}The ruling clan cannot join a rebel faction.");
            }

            // mercenaries can't join factions
            if (clan.IsUnderMercenaryService)
            {
                yield return new TextObject("{=FRmercenary}Mercenary clans cannot join a rebel faction.");
            }

            // can't join a faction when eliminated
            if (clan.IsEliminated)
            {
                yield return new TextObject("{=FReliminated}This clan has been eliminated.");
            }
        }

        private static IEnumerable<TextObject> GetMembershipExceptions(Clan clan, RebelFaction rebelFaction, bool existingMember)
        {
            // an existing member isn't held back by the faction it's already in
            var otherFactions = clan.Kingdom.GetRebelFactions().Where(x => !existingMember || x != rebelFaction).ToList();

            // faction sponsors can't join another faction
            var sponsoredFaction = otherFactions.FirstOrDefault(x => x.SponsorClan == clan);
            if (sponsoredFaction is not null)
            {
                yield return new TextObject("{=FRleadsOther}This clan leads {FACTION_NAME} and cannot join another faction.")
                    .SetTextVariable("FACTION_NAME", sponsoredFaction.Name);
            }

            // can't join a faction you're already a member of
            if (!existingMember && rebelFaction.Clans.Contains(clan))
            {
                yield return new TextObject("{=FRisMember}This clan already supports your faction.");
            }

            // can't join a faction when member of a secession faction
            var secessionFaction = otherFactions.FirstOrDefault(x => x.RebelDemandType == RebelDemandType.Secession && x.Clans.Contains(clan));
            if (secessionFaction is not null)
            {
                yield return new TextObject("{=FRsecession}This clan supports {FACTION_NAME}, a secession faction, and cannot join another faction.")
                    .SetTextVariable("FACTION_NAME", secessionFaction.Name);
            }
        }
    }
}