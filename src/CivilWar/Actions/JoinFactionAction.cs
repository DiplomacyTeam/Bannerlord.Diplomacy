using System.Linq;
using TaleWorlds.CampaignSystem;

namespace Diplomacy.CivilWar
{
    public class JoinFactionAction
    {
        public static void Apply(Clan clan, RebelFaction rebelFaction) 
        {
            rebelFaction.AddClan(clan);
        }

        public static bool ShouldApply(Clan clan, RebelFaction rebelFaction) 
        {
            if (rebelFaction.Clans.Contains(clan) || RebelFactionManager.GetRebelFaction(clan.Kingdom).Where(x => x.Clans.Contains(clan) && x.RebelDemandType == RebelDemandType.Secession).Any())
                return false;
            var score = RebelFactionScoringModel.GetDemandScore(clan, rebelFaction);
            return score.ResultNumber >= RebelFactionScoringModel.RequiredScore;
        }
    }
}
