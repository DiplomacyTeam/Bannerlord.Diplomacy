using System;
using System.Collections.Generic;
using Diplomacy.Costs;
using Diplomacy.DiplomaticAction.Barter.Barterables;
using JetBrains.Annotations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Barter
{
    internal class TributeBarterable : AbstractDiplomaticBarterable, IDurationBarterItem, IAmountBarterItem
    {
        protected const float ScorePerGold = 0.001f;
        private static readonly TextObject _TOfferTribute = new("{=}Offer Tribute");
        private static readonly TextObject _TRequestTribute = new("{=}Request Tribute");

        public override InfluenceCost InfluenceCost => new(_proposingFaction.Leader.Clan, 10f);
        public override bool IsExclusive => false;

        public override ContributionParty ContributionParty { get; }
        public override TextObject Name => ContributionParty == ContributionParty.Proposing ? _TOfferTribute : _TRequestTribute;

        public TributeBarterable(ContributionParty contributionParty, [NotNull] IFaction proposingFaction, [NotNull] IFaction consideringFaction) :
            base(proposingFaction, consideringFaction)
        {
            ContributionParty = contributionParty;
        }

        public int MinAmount => 1;
        public int MaxAmount => 500;
        public int Amount { get; set; } = 100;

        public int MinDuration => 2;
        public int MaxDuration => 500;
        public int Duration { get; set; } = 100;

        protected override float GetDealValueInternal(ContributionParty contributionParty)
        {
            double npv = 0;
            for (var i = 1; i <= Duration; i++) npv += Amount / Math.Pow(1 + 0.1f / CampaignTime.DaysInYear, i);
            var value = Convert.ToSingle(npv * ScorePerGold);

            return ContributionParty == contributionParty ? -value : value;
        }

        protected override bool IsValidOption(IReadOnlyList<AbstractDiplomaticBarterable> currentProposal)
        {
            return true;
        }

        public override void Execute()
        {
            DiplomaticAgreementManager.RegisterAgreement(new TributeAgreement(CampaignTime.Now, CampaignTime.Never, (GetPayer() as Kingdom)!,
                (GetRecipient() as Kingdom)!, Amount, Duration));
        }

        protected IFaction GetPayer()
        {
            return _proposingFaction == GetRecipient() ? _consideringFaction : _proposingFaction;
        }

        protected IFaction GetRecipient()
        {
            return ContributionParty == ContributionParty.Proposing ? _consideringFaction : _proposingFaction;
        }
    }
}