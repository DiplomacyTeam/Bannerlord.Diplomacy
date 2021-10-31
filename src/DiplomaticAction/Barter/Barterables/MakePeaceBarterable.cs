using System.Collections.Generic;
using Diplomacy.Costs;
using Diplomacy.DiplomaticAction.Barter.Barterables;
using Diplomacy.DiplomaticAction.WarPeace;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Barter
{
    internal class MakePeaceBarterable : AbstractDiplomaticBarterable
    {
        public override InfluenceCost InfluenceCost =>
            IsKingdomAgreement() ? DiplomacyCostCalculator.DetermineCostForMakingPeace(Kingdom1!, Kingdom2!).InfluenceCost : InfluenceCost.NullCost;

        public override bool IsExclusive => false;

        public override ContributionParty ContributionParty => ContributionParty.Mutual;

        public override TextObject Name => GameTexts.FindText("str_kingdom_propose_peace_action");

        public MakePeaceBarterable(IFaction proposingFaction, IFaction consideringFaction) : base(proposingFaction, consideringFaction)
        {
        }

        protected override float GetDealValueInternal(ContributionParty contributionParty)
        {
            return 0f;
        }

        protected override bool IsValidOption(IReadOnlyList<AbstractDiplomaticBarterable> currentProposal)
        {
            return IsKingdomAgreement() && FactionManager.IsAtWarAgainstFaction(Kingdom1, Kingdom2) &&
                   MakePeaceConditions.Instance.CanApply(Kingdom1!, Kingdom2!, bypassCosts: true);
        }

        public override void Execute()
        {
            MakePeaceAction.Apply(_proposingFaction, _consideringFaction);
        }
    }
}