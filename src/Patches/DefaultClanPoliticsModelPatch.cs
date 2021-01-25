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

        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch("CalculateInfluenceChange")]
        public static void CalculateInfluenceChangePatch(Clan clan, ref ExplainedNumber __result)
        {
            if (!Settings.Instance!.EnableInfluenceBalancing)
                return;

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

            /// Minimum Influence Gain (Maximum Influence Loss)

            int lossLimit = Settings.Instance.MaximumInfluenceLoss;

            if (__result.ResultNumber < -lossLimit)
                __result.LimitMin(-lossLimit);
        }
    }
}
