using Diplomacy.CivilWar.Factions;
using Diplomacy.CivilWar.Scoring;
using Diplomacy.Extensions;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar.Actions
{
    public class JoinFactionAction
    {
        public static void Apply(Clan clan, RebelFaction rebelFaction)
        {
            rebelFaction.AddClan(clan);
        }

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
            IEnumerable<TextObject> exceptions;
            reason = (exceptions = CanApply(clan, rebelFaction)).FirstOrDefault();

            return !exceptions.Any();
        }

        /// <summary>
        /// Determines the set of reasons that the given faction cannot be joined by the clan.
        /// </summary>
        /// <param name="clan"></param>
        /// <param name="rebelFaction"></param>
        /// <returns>
        /// Set of reasons for which the faction cannot be joined. As the reasons are not currently used
        /// in the UI, currently supplying empty TextObjects.
        /// </returns>
        public static IEnumerable<TextObject> CanApply(Clan clan, RebelFaction rebelFaction)
            => GetExceptions(clan, rebelFaction, existingMember: false);

        private static IEnumerable<TextObject> GetExceptions(Clan clan, RebelFaction rebelFaction, bool existingMember)
        {
            // can only join a faction of a kingdom that they're in
            if (rebelFaction.ParentKingdom != clan.Kingdom)
            {
                yield return TextObject.GetEmpty();
                yield break;
            }

            // rebel kingdom members can't join factions
            if (clan.Kingdom.IsRebelKingdom())
            {
                yield return TextObject.GetEmpty();
            }

            // ruling clan can't join factions
            if (clan == clan.Kingdom.RulingClan)
            {
                yield return TextObject.GetEmpty();
            }

            // mercenaries can't join factions
            if (clan.IsUnderMercenaryService)
            {
                yield return TextObject.GetEmpty();
            }

            // can't join a faction during an active rebellion
            if (rebelFaction.AtWar)
            {
                yield return TextObject.GetEmpty();
            }

            // faction sponsors can't join another faction 
            if (clan.Kingdom.GetRebelFactions().Any(x => x.SponsorClan == clan && (!existingMember || x != rebelFaction)))
            {
                yield return TextObject.GetEmpty();
            }

            // can't join a faction you're already a member of
            if (!existingMember && rebelFaction.Clans.Contains(clan))
            {
                yield return TextObject.GetEmpty();
            }

            // can't join a faction when member of a secession faction
            if (clan.Kingdom.GetRebelFactions().Any(x => x.Clans.Contains(clan) && x.RebelDemandType == RebelDemandType.Secession && (!existingMember || x != rebelFaction)))
            {
                yield return TextObject.GetEmpty();
            }

            // can't join a faction when eliminated
            if (clan.IsEliminated)
            {
                yield return TextObject.GetEmpty();
            }
        }
    }
}