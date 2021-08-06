using System.Collections.Generic;
using System.Linq;
using Diplomacy.Costs;
using Diplomacy.DiplomaticAction.Barter.Barterables;
using Diplomacy.DiplomaticAction.NonAggressionPact;
using JetBrains.Annotations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Barter
{
    internal class NonAggressionPactBarterable : AbstractDiplomaticBarterable, IDurationBarterItem, IBreakableAgreement
    {
        private static readonly TextObject _TFormPact = new("{=xdjqRqu1}Form a Non-Aggression Pact");
        private static readonly TextObject _TBreakPact = new("{=}Break a Non-Aggression Pact");
        private readonly NonAggressionPactAgreement? _agreement;
        public override ContributionParty ContributionParty => ContributionParty.Mutual;

        public override InfluenceCost InfluenceCost => IsKingdomAgreement()
            ? DiplomacyCostCalculator.DetermineInfluenceCostForFormingNonAggressionPact(Kingdom1!, Kingdom2!)
            : InfluenceCost.NullCost;

        public override bool IsExclusive => false;

        public override TextObject Name => IsKingdomAgreement() && DiplomaticAgreementManager.HasNonAggressionPact(Kingdom1!, Kingdom2!, out _)
            ? _TBreakPact
            : _TFormPact;

        public NonAggressionPactBarterable([NotNull] IFaction proposingFaction, [NotNull] IFaction consideringFaction) : base(proposingFaction,
            consideringFaction)
        {
            _agreement = IsKingdomAgreement() && DiplomaticAgreementManager.HasNonAggressionPact(Kingdom1!, Kingdom2!, out var agreement)
                ? agreement
                : default;
        }

        public override bool IsDeclaration => IsBreakAgreement;

        public int MinDuration => 1;
        public int MaxDuration => 500;
        public int Duration { get; set; } = 100;

        protected override float GetDealValueInternal(ContributionParty contributionParty)
        {
            if (IsBreakAgreement)
                return 0f;

            var thisKingdom = Kingdoms[contributionParty];
            var otherKingdom = Kingdoms[GetOtherParty(contributionParty)];
            var explainedNumber = NonAggressionPactScoringModel.Instance.GetScore(thisKingdom, otherKingdom!);
            var dealValue = (explainedNumber.ResultNumber - NonAggressionPactScoringModel.AcceptOrProposeThreshold) *
                            (NonAggressionPactScoringModel.Instance.BaseDiplomaticBarterValue / 100);
            return IsBreakAgreement ? -dealValue : dealValue;
        }

        protected override bool IsValidOption(IReadOnlyList<AbstractDiplomaticBarterable> currentProposal)
        {
            return IsBreakAgreement ||
                   IsKingdomAgreement() && NonAggressionPactConditions.Instance.CanApply(Kingdom1!, Kingdom2!, bypassCosts: true) &&
                   !currentProposal.Any(x => x is AllianceBarterable);
        }

        public override void Execute()
        {
            if (IsBreakAgreement)
                _agreement!.Expire();
            else
                FormNonAggressionPactAction.Apply(Kingdom1!, Kingdom2!, bypassCosts: true, customDurationInDays: Duration, queryPlayer: false);

            InfluenceCost.ApplyCost();
        }

        public bool IsBreakAgreement => _agreement != null;
    }
}