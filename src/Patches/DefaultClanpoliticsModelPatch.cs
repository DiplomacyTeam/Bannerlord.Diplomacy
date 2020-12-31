using Diplomacy.Extensions;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;

namespace Diplomacy.Patches
{
    [HarmonyPatch(typeof(DefaultClanPoliticsModel))]
    class DefaultClanPoliticsModelPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("CalculateInfluenceChange")]
        public static void CalculateInfluenceChangePatch(ref float __result, Clan clan, StatExplainer explanation = null)
        {
            if (Settings.Instance.EnableInfluenceBalancing)
            {
                float influenceChange = __result;

                if (Settings.Instance.EnableCorruption)
                {
                    float corruption = clan.GetCorruption();
                    if (corruption > 0)
                    {
                        explanation?.AddLine(new TextObject("{=dUCOV7km}Corruption (too many fiefs)").ToString(), -corruption);
                        influenceChange -= corruption;
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
                if (influenceChange < -maximumInfluenceLoss)
                {
                    explanation?.AddLine(new TextObject("{=uZc8Hg01}Maximum Influence Loss").ToString(), -maximumInfluenceLoss, StatExplainer.OperationType.LimitMin);
                    influenceChange = -maximumInfluenceLoss;
                }

                __result = influenceChange;
            }
        }
    }
}
