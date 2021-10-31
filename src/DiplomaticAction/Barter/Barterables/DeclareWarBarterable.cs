using System.Collections.Generic;
using Diplomacy.Costs;
using Diplomacy.DiplomaticAction.Barter.Barterables;
using Diplomacy.DiplomaticAction.WarPeace;
using JetBrains.Annotations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Barter
{
    internal class DeclareWarBarterable : AbstractDiplomaticBarterable
    {
        public override ContributionParty ContributionParty => ContributionParty.Mutual;
        public override InfluenceCost InfluenceCost => DiplomacyCostCalculator.DetermineCostForDeclaringWar((_proposingFaction as Kingdom)!);
        public override bool IsExclusive => true;

        public override TextObject Name => GameTexts.FindText("str_kingdom_declate_war_action");

        public DeclareWarBarterable([NotNull] IFaction proposingFaction, [NotNull] IFaction consideringFaction) : base(proposingFaction,
            consideringFaction)
        {
        }

        protected override float GetDealValueInternal(ContributionParty contributionParty)
        {
            return 0f;
        }

        protected override bool IsValidOption(IReadOnlyList<AbstractDiplomaticBarterable> currentProposal)
        {
            return currentProposal.IsEmpty() &&
                   !_proposingFaction.IsAtWarWith(_consideringFaction) &&
                   (_proposingFaction is not Kingdom kingdom1 || _consideringFaction is not Kingdom kingdom2 ||
                    DeclareWarConditions.Instance.CanApply(kingdom1, kingdom2));
        }

        public override void Execute()
        {
            DeclareWarAction.Apply(_proposingFaction, _consideringFaction);
            InfluenceCost.ApplyCost();
        }
    }
}