using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diplomacy.DiplomaticAction.Barter.Barterables;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Barter
{
    public class DiplomaticBarter
    {
        private readonly List<AbstractDiplomaticBarterable> _allOptions;
        private readonly List<AbstractDiplomaticBarterable> _proposal;

        private readonly IFaction _otherFaction;
        private readonly IFaction _proposingFaction;

        public IReadOnlyList<AbstractDiplomaticBarterable> Proposal => _proposal;

        public DiplomaticBarter(IFaction proposingFaction, IFaction otherFaction)
        {
            _proposingFaction = proposingFaction;
            _otherFaction = otherFaction;
            _proposal = new List<AbstractDiplomaticBarterable>();
            _allOptions = GetBarterItems(proposingFaction, otherFaction).ToList();
        }

        public static IEnumerable<AbstractDiplomaticBarterable> GetBarterItems(IFaction faction1, IFaction faction2)
        {
            yield return new DeclareWarBarterable(faction1, faction2);
            yield return new MakePeaceBarterable(faction1, faction2);
            yield return new NonAggressionPactBarterable(faction1, faction2);
            yield return new AllianceBarterable(faction1, faction2);
            yield return new PaymentBarterable(ContributionParty.Proposing, faction1, faction2);
            yield return new PaymentBarterable(ContributionParty.Considering, faction1, faction2);
            yield return new TributeBarterable(ContributionParty.Proposing, faction1, faction2);
            yield return new TributeBarterable(ContributionParty.Considering, faction1, faction2);
        }

        // the typical faction barter. The proposal is based around a diplomatic agreement
        public void ExecuteAIBarter()
        {
            var baseItems = GetValidOptions()
                .Where(x => x is not PaymentBarterable && x.GetDealValue(ContributionParty.Proposing) > 1.0f)
                .OrderByDescending(x => x.GetDealValue(ContributionParty.Proposing) + x.GetDealValue(ContributionParty.Considering))
                .Select(x => Tuple.Create(x, x.GetDealValue(ContributionParty.Proposing)));

            var baseItem = baseItems.FirstOrDefault();
            if (baseItem == null || baseItem.Item2 < 1.0f) return;

            AddToProposal(baseItem.Item1);

            // supplement score with gold
            BalanceBarterWithGold();

            if (IsAcceptableProposal()) ExecuteProposal();
        }

        private List<AbstractDiplomaticBarterable> GetValidOptions()
        {
            return _allOptions.Where(x => x.IsValid(_proposal)).ToList();
        }

        private void BalanceBarterWithGold()
        {
            if (IsAcceptableProposal())
                return;

            var netScore = GetNetScore();
            var goldBarterable = GetValidOptions().OfType<PaymentBarterable>().First(x =>
                x.ContributionParty == (netScore > 0.0f ? ContributionParty.Considering : ContributionParty.Proposing));
            if (goldBarterable.CanGetValueForDiplomaticScore(Math.Abs(netScore), out var value))
            {
                goldBarterable.Amount = value;
                _proposal.Add(goldBarterable);
            }
        }

        private bool IsAcceptableProposal()
        {
            var netScore = GetNetScore();
            if (_proposingFaction == Clan.PlayerClan.MapFaction)
                return netScore >= 0.0f
                       && _proposal.Sum(x => x.GetDealValue(ContributionParty.Considering)) > 0.0f;

            return netScore is >= 0.0f and <= 5.0f
                   && _proposal.Sum(x => x.GetDealValue(ContributionParty.Proposing)) > 0.0f
                   && _proposal.Sum(x => x.GetDealValue(ContributionParty.Considering)) > 0.0f;
        }

        public float GetNetScore()
        {
            return _proposal.Sum(x => x.GetNetScore());
        }

        public void AddToProposal(AbstractDiplomaticBarterable barterable)
        {
            if (barterable.IsExclusive)
                Clear();
            _proposal.Add(barterable);
        }

        public void RemoveFromProposal(AbstractDiplomaticBarterable barterable)
        {
            _proposal.Remove(barterable);
        }

        public bool IsProposalAcceptable()
        {
            return !_proposal.IsEmpty() && GetNetScore() >= 0f && CanPayInfluenceCost();
        }

        public bool CanPayInfluenceCost()
        {
            return GetInfluencePayer().Influence >= GetInfluenceCost();
        }

        public float GetInfluenceCost()
        {
            return _proposal.Sum(x => x.InfluenceCost.Value);
        }

        private Clan GetInfluencePayer()
        {
            return _proposingFaction.IsKingdomFaction ? (_proposingFaction as Kingdom)!.RulingClan : (_proposingFaction as Clan)!;
        }

        public void ExecuteProposal()
        {
            if (_otherFaction is Kingdom kingdom && kingdom.RulingClan == Clan.PlayerClan)
            {
                var barterItemText = new TextObject("{=}{newline}{BARTER_ITEM}");

                TextObject CreateBarterItemText(AbstractDiplomaticBarterable diplomaticBarterItem)
                {
                    return barterItemText!.CopyTextObject().SetTextVariable("BARTER_ITEM", diplomaticBarterItem.ToString());
                }

                StringBuilder descriptionBuilder = new();

                void ShowBarterItems(ContributionParty contributionParty, TextObject textObject)
                {
                    var barterItems = _proposal.Where(x => x.ContributionParty == contributionParty).ToList();
                    if (barterItems.Any())
                    {
                        descriptionBuilder.Append(textObject);
                        foreach (var diplomaticBarterItem in barterItems) descriptionBuilder.Append(CreateBarterItemText(diplomaticBarterItem));
                    }
                }

                ShowBarterItems(ContributionParty.Mutual, new TextObject("{=}{newline} {newline}Mutual contributions:"));
                ShowBarterItems(ContributionParty.Considering, new TextObject("{=}{newline} {newline}You will provide:"));
                ShowBarterItems(ContributionParty.Proposing, new TextObject("{=}{newline} {newline}In return, they offer:"));

                InformationManager.ShowInquiry(new InquiryData(
                    new TextObject("{=}Proposal from {KINGDOM}").SetTextVariable("KINGDOM", _proposingFaction.Name).ToString(),
                    descriptionBuilder.ToString(),
                    true,
                    true,
                    new TextObject(StringConstants.Accept).ToString(),
                    new TextObject(StringConstants.Decline).ToString(),
                    ExecuteProposalItems,
                    null
                ), true);
            }
            else
            {
                ExecuteProposalItems();
            }
        }

        private void ExecuteProposalItems()
        {
            GetInfluencePayer().Influence -= _proposal.Sum(x => x.InfluenceCost.Value);
            foreach (var barterItem in _proposal) barterItem.Execute();
        }

        public void Clear()
        {
            _proposal.Clear();
        }

    }
}