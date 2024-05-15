using System;
using System.Collections.Generic;

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

        public static IEnumerable<Hero> AllRelatedHeroes(this Hero inHero, bool includeExSpouses = false)
        {
            if (inHero.Father != null)
            {
                yield return inHero.Father;

                // father side uncles/aunts are close family
                foreach (Hero hero3 in inHero.Father.Siblings)
                    yield return hero3;
            }

            if (inHero.Mother != null)
            {
                yield return inHero.Mother;

                // mother side uncles/aunts are close family
                foreach (Hero hero4 in inHero.Mother.Siblings)
                    yield return hero4;
            }

            if (inHero.Spouse != null)
                yield return inHero.Spouse;

            foreach (Hero hero in inHero.Children)
                yield return hero;

            foreach (Hero hero2 in inHero.Siblings)
                yield return hero2;

            if (includeExSpouses)
                foreach (Hero hero3 in inHero.ExSpouses)
                    yield return hero3;
        }
    }
}