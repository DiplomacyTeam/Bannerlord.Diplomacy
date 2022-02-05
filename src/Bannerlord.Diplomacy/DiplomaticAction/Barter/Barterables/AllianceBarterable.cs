using System.Collections.Generic;
using System.Linq;
using Diplomacy.Costs;
using Diplomacy.DiplomaticAction.Alliance;
using Diplomacy.DiplomaticAction.Barter.Barterables;
using Diplomacy.Extensions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Barter
{
    internal class AllianceBarterable : AbstractDiplomaticBarterable, IBreakableAgreement
    {
        private static readonly TextObject _TFormAlliance = new("{=0WPWbx70}Form Alliance");
        private static readonly TextObject _TBreakAlliance = new("{=K4GraLTn}Break Alliance");

        public override InfluenceCost InfluenceCost => DiplomacyCostCalculator.DetermineCostForFormingAlliance(Kingdom1!, Kingdom2!).InfluenceCost;

        public override bool IsExclusive => false;

        public override ContributionParty ContributionParty => ContributionParty.Mutual;

        public override TextObject Name => IsBreakAgreement ? _TBreakAlliance : _TFormAlliance;

        public override bool IsDeclaration => IsBreakAgreement;

        public AllianceBarterable(IFaction faction1, IFaction faction2) : base(faction1, faction2)
        {
        }

        protected override float GetDealValueInternal(ContributionParty contributionParty)
        {
            if (IsBreakAgreement)
                return 0f;

            var thisKingdom = Kingdoms[contributionParty];
            var otherKingdom = Kingdoms[GetOtherParty(contributionParty)];
            var explainedNumber = AllianceScoringModel.Instance.GetScore(thisKingdom, otherKingdom!, contributionParty == ContributionParty.Proposing ? DiplomaticPartyType.Proposer : DiplomaticPartyType.Recipient);
            var dealValue = (explainedNumber.ResultNumber - AllianceScoringModel.AcceptOrProposeThreshold) *
                            (AllianceScoringModel.Instance.BaseDiplomaticBarterValue / AllianceScoringModel.AcceptOrProposeThreshold);
            return IsBreakAgreement ? -dealValue : dealValue;
        }

        protected override bool IsValidOption(IReadOnlyList<AbstractDiplomaticBarterable> currentProposal)
        {
            return IsKingdomAgreement() && (FormAllianceConditions.Instance.CanApply(Kingdom1!, Kingdom2!) && !currentProposal.Any(x => x is NonAggressionPactBarterable) ||
                                            IsBreakAgreement && BreakAllianceConditions.Instance.CanApply(Kingdom1!, Kingdom2!));
        }

        public override void Execute()
        {
            //TODO: rewrite! Need to check if alliance can be applied
            if (Kingdom1!.IsAlliedWith(Kingdom2!))
                BreakAllianceAction.Apply(Kingdom1!, Kingdom2!, bypassCosts: true);
            else
                DeclareAllianceAction.Apply(Kingdom1!, Kingdom2!, bypassCosts: true);
        }

        public bool IsBreakAgreement => Kingdom1!.IsAlliedWith(Kingdom2!);
    }
}