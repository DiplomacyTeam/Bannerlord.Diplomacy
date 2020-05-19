using DiplomacyFixes.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace DiplomacyFixes.Alliance
{
    class AllianceScoringModel
    {
        public static float FormAllianceScoreThreshold { get; } = 20.0f;

        public static float GetFormAllianceScore(Kingdom kingdom, Kingdom otherKingdom)
        {
            float totalScore = 0;
            float totalStrength = 0;

            float[] kingdomStrengths = Kingdom.All.Select(curKingdom => curKingdom.TotalStrength).OrderBy(a => a).ToArray();

            int halfIndex = kingdomStrengths.Count() / 2;
            float medianStrength;

            if ((kingdomStrengths.Length % 2) == 0)
            {
                medianStrength = (kingdomStrengths.ElementAt(halfIndex) + kingdomStrengths.ElementAt(halfIndex - 1)) / 2;
            }
            else
            {
                medianStrength = kingdomStrengths.ElementAt(halfIndex);
            }

            // weak faction bonus
            float averageStrength = totalStrength / Kingdom.All.Count;
            if (kingdom.TotalStrength <= medianStrength)
            {
                totalScore += (int)AllianceScore.BelowMedianStrength;
            }

            // common enemies
            IEnumerable<Kingdom> commonEnemies = FactionManager.GetEnemyKingdoms(kingdom).Intersect(FactionManager.GetEnemyKingdoms(otherKingdom));
            totalScore += commonEnemies.Count() * (int)AllianceScore.HasCommonEnemy;

            // bordering or inside territory modifier
            if (kingdom.IsInsideTeritoryOf(otherKingdom))
            {
                float traitScore = 0;
                traitScore -= (int)AllianceScore.SharesBorder * kingdom.Leader.GetHeroTraits().Calculating;
                traitScore += (int)AllianceScore.SharesBorder * kingdom.Leader.GetHeroTraits().Mercy;

                totalScore += traitScore;
            }

            // existing alliances
            totalScore -= (int) AllianceScore.ExistingAlliance * Kingdom.All.Except(new[] { kingdom, otherKingdom }).Where(curKingdom => FactionManager.IsAlliedWithFaction(kingdom, curKingdom)).Count();

            // relation modifier
            float relationModifier = MBMath.ClampFloat((float)Math.Log((kingdom.Leader.GetRelation(otherKingdom.Leader) + 100f) / 100f, 1.5), -1, 1);
            totalScore += (int) AllianceScore.Relationship * relationModifier;

            return totalScore;
        }

        public static bool ShouldFormAlliance(Kingdom kingdom, Kingdom otherKingdom)
        {
            return GetFormAllianceScore(kingdom, otherKingdom) >= FormAllianceScoreThreshold && GetFormAllianceScore(otherKingdom, kingdom) >= FormAllianceScoreThreshold;
        }

        private enum AllianceScore : int
        {
            BelowMedianStrength = 10,
            HasCommonEnemy = 10,
            SharesBorder = 5,
            ExistingAlliance = 10,
            Relationship = 10
        }
    }
}
