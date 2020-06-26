using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;

namespace DiplomacyFixes.Models
{
    class InfluenceModel : DefaultClanPoliticsModel
    {
        public override float CalculateInfluenceChange(Clan clan, StatExplainer explanation = null)
        {
            float influenceChange = base.CalculateInfluenceChange(clan, explanation);

            if (Settings.Instance.EnableCorruption)
            {
                int numFiefsTooMany = clan.Fiefs.Count() - clan.Tier;
                float corruption = 0f;
                if (numFiefsTooMany > 0)
                {
                    int factor = numFiefsTooMany > 5 ? -2 : -1;
                    corruption = numFiefsTooMany * factor;
                    explanation?.AddLine(new TextObject("{=dUCOV7km}Corruption (too many fiefs)").ToString(), corruption);
                    influenceChange += corruption;
                }
            }

            if (Settings.Instance.EnableInfluenceDecay)
            {
                int influenceDecayFactor = clan.Influence > Settings.Instance.InfluenceDecayThreshold ? (int)-((clan.Influence - Settings.Instance.InfluenceDecayThreshold) * (Settings.Instance.InfluenceDecayPercentage / 100)) : 0;
                if (influenceDecayFactor < 0)
                {
                    explanation?.AddLine(new TextObject("{=koTNaZUX}Influence Decay (too much influence)").ToString(), influenceDecayFactor);
                    influenceChange += influenceDecayFactor;
                }
            }

            float maximumInfluenceLoss = Settings.Instance.MaximumInfluenceLoss;
            if (influenceChange <= -maximumInfluenceLoss)
            {
                explanation?.AddLine(new TextObject("{=uZc8Hg01}Maximum Influence Loss").ToString(), -maximumInfluenceLoss, StatExplainer.OperationType.LimitMin);
                influenceChange = -maximumInfluenceLoss;
            }

            return influenceChange;
        }
    }
}
