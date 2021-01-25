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
        private static readonly TextObject _txtCorruption = new("{=dUCOV7km}Corruption: Too Many Fiefs");
        private static readonly TextObject _txtInfluenceDecay = new("{=koTNaZUX}Decay of High Influence");
        private static readonly TextObject _txtInfluenceLossLimit = new("{=dUCOV7km}Influence Loss Limit");

        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch("CalculateInfluenceChange")]
        public static void CalculateInfluenceChangePatch(Clan clan, ref ExplainedNumber __result)
        {
            if (Settings.Instance!.EnableInfluenceBalancing)
            {
                /// Corruption

                if (Settings.Instance.EnableCorruption)
                {
                    float corruption = clan.GetCorruption();

                    if (corruption >= 0.01f)
                        __result.Add(-corruption, _txtCorruption);
                }

                /// Influence Decay

                if (Settings.Instance.EnableInfluenceDecay && clan.Influence > Settings.Instance.InfluenceDecayThreshold)
                {
                    float decayFactor = Settings.Instance.InfluenceDecayPercentage / 100f;
                    int decay = (int)(decayFactor * (clan.Influence - Settings.Instance.InfluenceDecayThreshold));

                    if (decay > 0)
                        __result.Add(-decay, _txtInfluenceDecay);
                }

                /// Influence Loss Limit

                int lossLimit = Settings.Instance.MaximumInfluenceLoss;
                float total = __result.ResultNumber;

                // NOTE: This used to be a LimitMin, but with e1.5.7, ExplainedNumber.LimitMin does not
                // accept a description of the operation. IDK whether it'd provide a generic one or it'd
                // omit it entirely, and neither seems acceptable. Since I also ensured that this postfix
                // patch runs last in priority, I've converted the logical LimitMin operation into an Add
                // off the offset required to enforce the minimum.

                float limitDiff = -total - lossLimit;

                if (limitDiff > 0)
                    __result.Add(limitDiff, _txtInfluenceLossLimit);
            }
        }
    }
}
