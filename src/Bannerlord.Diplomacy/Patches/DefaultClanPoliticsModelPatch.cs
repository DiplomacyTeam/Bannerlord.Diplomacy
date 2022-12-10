using Diplomacy.Extensions;
using Diplomacy.PatchTools;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Localization;

namespace Diplomacy.Patches
{
    internal sealed class DefaultClanPoliticsModelPatch : PatchClass<DefaultClanPoliticsModelPatch, DefaultClanPoliticsModel>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Postfix(nameof(CalculateInfluenceChangePostfix),
                        nameof(DefaultClanPoliticsModel.CalculateInfluenceChange),
                        Priority.Last),
        };

        private static void CalculateInfluenceChangePostfix(Clan clan, ref ExplainedNumber __result)
        {
            if (!Settings.Instance!.EnableInfluenceBalancing)
                return;

            // Corruption

            if (Settings.Instance!.EnableCorruption)
            {
                var corruption = clan.GetCorruption();

                if (corruption >= 0.01f)
                    __result.Add(-corruption, _TCorruption);
            }

            // Influence Decay

            if (Settings.Instance!.EnableInfluenceDecay && clan.Influence > Settings.Instance!.InfluenceDecayThreshold)
            {
                var decayFactor = Settings.Instance!.InfluenceDecayPercentage / 100f;
                var decay = (int) (decayFactor * (clan.Influence - Settings.Instance!.InfluenceDecayThreshold));

                if (decay > 0)
                    __result.Add(-decay, _TInfluenceDecay);
            }

            // Minimum Influence Gain (Maximum Influence Loss)
            __result.LimitMin(-Settings.Instance!.MaximumInfluenceLoss);
        }

        private static readonly TextObject _TCorruption = new("{=dUCOV7km}Corruption: Too Many Fiefs");
        private static readonly TextObject _TInfluenceDecay = new("{=koTNaZUX}Decay of High Influence");
    }
}