using System;
using System.Collections.Generic;
using Diplomacy.Costs;
using JetBrains.Annotations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Barter.Barterables
{
    internal class PaymentBarterable : AbstractDiplomaticBarterable, IAmountBarterItem
    {
        protected const float ScorePerGold = 0.001f;
        private static readonly TextObject _TOfferPayment = new("{=}Offer Payment");
        private static readonly TextObject _TRequestPayment = new("{=}Request Payment");
        public override ContributionParty ContributionParty { get; }

        public override InfluenceCost InfluenceCost => new(_proposingFaction.Leader.Clan, 10f);

        public override bool IsExclusive => false;

        public override TextObject Name =>
            ContributionParty == ContributionParty.Proposing ? _TOfferPayment : _TRequestPayment;

        public PaymentBarterable(ContributionParty contributionParty, [NotNull] IFaction proposingFaction, [NotNull] IFaction consideringFaction) :
            base(proposingFaction,
                consideringFaction)
        {
            ContributionParty = contributionParty;
        }

        public int MinAmount => 1;
        public int MaxAmount => ContributionParty == ContributionParty.Proposing ? _proposingFaction.Leader.Gold : _consideringFaction.Leader.Gold;
        public int Amount { get; set; } = 100;

        protected override float GetDealValueInternal(ContributionParty contributionParty)
        {
            var value = Amount * ScorePerGold;
            return contributionParty == ContributionParty ? -value : value;
        }

        public bool CanGetValueForDiplomaticScore(float requiredScore, out int value, bool includeBuffer = true)
        {
            if(includeBuffer)
                requiredScore += ContributionParty == ContributionParty.Considering ? -1.0f : 1.0f;
            value = Convert.ToInt32(Math.Ceiling(requiredScore / ScorePerGold));
            var contributingKingdom = ContributionParty == ContributionParty.Proposing ? Kingdom1! : Kingdom2!;
            return value <= contributingKingdom!.Leader.Gold / 2;
        }

        protected override bool IsValidOption(IReadOnlyList<AbstractDiplomaticBarterable> currentProposal)
        {
            return true;
        }

        public override void Execute()
        {
            var recipient = GetRecipient();
            var payer = GetPayer();
            GiveGoldAction.ApplyBetweenCharacters(payer.Leader, recipient.Leader, Amount);
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