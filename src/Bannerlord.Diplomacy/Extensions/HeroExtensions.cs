using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;

namespace Diplomacy.Extensions
{
    public static class HeroExtensions
    {
        public static float GetNormalizedTraitValue(this Hero hero, TraitObject trait)
        {
            var zeroMinMaxTraitLevel = (float) Math.Abs(trait.MinValue) + trait.MaxValue;
            var zeroMinTraitLevel = hero.GetTraitLevel(trait) + Math.Abs(trait.MinValue);
            return zeroMinTraitLevel / zeroMinMaxTraitLevel;
        }
    }
}