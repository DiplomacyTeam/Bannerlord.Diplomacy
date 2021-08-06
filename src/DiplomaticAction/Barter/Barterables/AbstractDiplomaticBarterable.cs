using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diplomacy.Costs;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Barter.Barterables
{
    public abstract class AbstractDiplomaticBarterable
    {
        protected IFaction _consideringFaction;
        protected IFaction _proposingFaction;

        protected Dictionary<ContributionParty, Kingdom> Kingdoms;
        protected Kingdom? Kingdom1 { get; }
        protected Kingdom? Kingdom2 { get; }

        public abstract InfluenceCost InfluenceCost { get; }
        public abstract bool IsExclusive { get; }

        public abstract ContributionParty ContributionParty { get; }

        public abstract TextObject Name { get; }

        public virtual bool IsDeclaration => false;

        protected AbstractDiplomaticBarterable(IFaction proposingFaction, IFaction consideringFaction)
        {
            _proposingFaction = proposingFaction;
            _consideringFaction = consideringFaction;


            Kingdom1 = proposingFaction as Kingdom;
            Kingdom2 = consideringFaction as Kingdom;

            Kingdoms = new Dictionary<ContributionParty, Kingdom>
                {[ContributionParty.Proposing] = Kingdom1!, [ContributionParty.Considering] = Kingdom2!};
        }

        protected bool IsKingdomAgreement()
        {
            return Kingdom1 is not null && Kingdom2 is not null;
        }

        public float GetNetScore()
        {
            return GetDealValueInternal(ContributionParty.Considering);
        }

        public float GetDealValue(ContributionParty contributionParty)
        {
            if (contributionParty is ContributionParty.Proposing && Kingdoms[contributionParty].RulingClan == Clan.PlayerClan) return 1f;

            return GetDealValueInternal(contributionParty);
        }

        protected abstract float GetDealValueInternal(ContributionParty contributionParty);

        protected abstract bool IsValidOption(IReadOnlyList<AbstractDiplomaticBarterable> currentProposal);

        public bool IsValid(IReadOnlyList<AbstractDiplomaticBarterable> currentProposal)
        {
            return !currentProposal.Any(x => x.IsExclusive) && currentProposal.All(x => x.GetType() != GetType()) && IsValidOption(currentProposal);
        }

        public abstract void Execute();

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(new TextObject("{=}{ITEM_NAME} ", new Dictionary<string, object>() {{"ITEM_NAME", Name}}));
            if (this is IAmountBarterItem or IDurationBarterItem)
            {
                StringBuilder contentBuilder = new();

                if (this is IAmountBarterItem amountItem)
                    contentBuilder.Append(new TextObject("{=}Amount: {AMOUNT}", new Dictionary<string, object>() {{"AMOUNT", amountItem.Amount}}));

                if (this is IAmountBarterItem and IDurationBarterItem) contentBuilder.Append(", ");

                if (this is IDurationBarterItem durationItem)
                    contentBuilder.Append(new TextObject("{=}Duration: {DURATION}",
                        new Dictionary<string, object>() {{"DURATION", durationItem.Duration}}));

                sb.Append(new TextObject("{=}({CONTENT})", new Dictionary<string, object>() {{"CONTENT", contentBuilder.ToString()}}));
            }

            return sb.ToString();
        }

        public bool HasEnoughInfluence()
        {
            return InfluenceCost.CanPayCost();
        }

        protected ContributionParty GetOtherParty(ContributionParty contributionParty)
        {
            return contributionParty == ContributionParty.Proposing ? ContributionParty.Considering : ContributionParty.Proposing;
        }

        protected bool IsPlayerProposal(ContributionParty contributionParty)
        {
            return contributionParty == ContributionParty.Proposing && Kingdoms[contributionParty].RulingClan == Clan.PlayerClan;
        }
    }

    public enum ContributionParty
    {
        Proposing,
        Considering,
        Mutual
    }

    internal interface IAmountBarterItem
    {
        public int MinAmount { get; }
        public int MaxAmount { get; }
        public int Amount { get; set; }
    }

    internal interface IDurationBarterItem
    {
        public int MinDuration { get; }
        public int MaxDuration { get; }
        public int Duration { get; set; }
    }

    internal interface IBreakableAgreement
    {
        public bool IsBreakAgreement { get; }
    }
}