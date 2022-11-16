using System;

using TaleWorlds.CampaignSystem.CharacterDevelopment;

namespace Diplomacy.Character
{
    internal class PlayerCharacterTraitEventExperience
    {
        public int Experience { get; }
        public TraitObject Trait { get; }

        public PlayerCharacterTraitEventExperience(int experience, TraitObject trait)
        {
            Experience = experience;
            Trait = trait;
        }

        public void Apply()
        {
            PlayerCharacterTraitHelper.UpdateTrait(this);
        }

        private static readonly Func<int, TraitObject, PlayerCharacterTraitEventExperience> Create = (a, b) => new PlayerCharacterTraitEventExperience(a, b);

        public static readonly PlayerCharacterTraitEventExperience FiefGranted = Create(50, DefaultTraits.Generosity);
        public static readonly PlayerCharacterTraitEventExperience FiefClaimed = Create(-50, DefaultTraits.Generosity);
    }
}