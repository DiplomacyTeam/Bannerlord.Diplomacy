using Diplomacy.PatchTools;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace Diplomacy.Patches
{
    internal sealed class DefaultDiplomacyModelPatch : PatchClass<DefaultDiplomacyModelPatch, DefaultDiplomacyModel>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Prefix(nameof(GetHeroesForEffectiveRelationPrefix), "GetHeroesForEffectiveRelation"),
        };

        // Relation between two kingdoms is relation between their leaders, and callers all over the
        // campaign pass Kingdom.Leader straight in without checking it (DefaultTradeAgreementModel's
        // GetRelationEffectBetweenRulers, for one). A leaderless kingdom therefore lands a null hero
        // here, where vanilla dereferences hero.Clan immediately.
        //
        // GetEffectiveRelation already treats a null result as "no relation" and returns 0, so simply
        // producing that answer is both safe and what the rest of the model expects.
        private static bool GetHeroesForEffectiveRelationPrefix(Hero hero1, Hero hero2, ref Hero? effectiveHero1, ref Hero? effectiveHero2)
        {
            if (hero1 is not null && hero2 is not null)
                return true;

            effectiveHero1 = null;
            effectiveHero2 = null;
            return false;
        }
    }
}
