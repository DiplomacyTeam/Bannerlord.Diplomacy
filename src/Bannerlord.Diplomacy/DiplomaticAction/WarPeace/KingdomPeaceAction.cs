using Diplomacy.Costs;
using Diplomacy.Event;

using Microsoft.Extensions.Logging;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.WarPeace
{
    internal sealed class KingdomPeaceAction
    {
        private static readonly TextObject _TDefeatTitle = new("{=BXluvRnJ}Bitter Defeat");

        private static void AcceptPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom, DiplomacyCost diplomacyCost)
        {
            LogFactory.Get<KingdomPeaceAction>()
                .LogTrace($"[{CampaignTime.Now}] {kingdomMakingPeace.Name} secured peace with {otherKingdom.Name} (cost: {diplomacyCost.Value}).");
            diplomacyCost.ApplyCost();
            MakePeaceAction.Apply(kingdomMakingPeace, otherKingdom);
        }

        private static string CreateMakePeaceInquiryText(Kingdom kingdomMakingPeace, Kingdom otherKingdom, int payment)
        {
            TextObject peaceText;
            if (Settings.Instance!.EnableWarExhaustion && WarExhaustionManager.Instance.HasMaxWarExhaustion(kingdomMakingPeace, otherKingdom))
            {
                peaceText = new TextObject("{=HWiDa4R1}Exhausted from the war, the {KINGDOM} is proposing peace with the {PLAYER_KINGDOM}.");
                peaceText.SetTextVariable("KINGDOM", kingdomMakingPeace.Name.ToString());
                peaceText.SetTextVariable("PLAYER_KINGDOM", otherKingdom.Name.ToString());
            }
            else
            {
                peaceText = new TextObject("{=t0ZS9maD}{KINGDOM_LEADER} of the {KINGDOM} is proposing peace with the {PLAYER_KINGDOM}.");
                peaceText.SetTextVariable("KINGDOM_LEADER", kingdomMakingPeace.Leader.Name.ToString());
                peaceText.SetTextVariable("KINGDOM", kingdomMakingPeace.Name.ToString());
                peaceText.SetTextVariable("PLAYER_KINGDOM", otherKingdom.Name.ToString());
            }

            var inquiryText = new List<string> { peaceText.ToString() };

            if (payment > 0)
            {
                var warReparationText = new TextObject("{=ZrwszZww} They are willing to pay war reparations of {DENARS} denars.");
                warReparationText.SetTextVariable("DENARS", payment);
                inquiryText.Add(warReparationText.ToString());
            }
            return string.Concat(inquiryText);
        }

        private static void CreatePeaceInquiry(Kingdom kingdom, Kingdom faction, HybridCost diplomacyCost)
        {
            var payment = (int) diplomacyCost.GoldCost.Value;
            InformationManager.ShowInquiry(new InquiryData(new TextObject("{=BkGSVccZ}Peace Proposal").ToString(), CreateMakePeaceInquiryText(kingdom, faction, payment), true, true, new TextObject("{=3fTqLwkC}Accept").ToString(), new TextObject("{=dRoMejb0}Decline").ToString(), () =>
            {
                AcceptPeace(kingdom, faction, diplomacyCost);
            }, () =>
            {
                Events.Instance.OnPeaceProposalSent(kingdom);
            }), true);
        }

        public static void ApplyPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, bool skipPlayerPrompts = false)
        {
            var diplomacyCost = DiplomacyCostCalculator.DetermineCostForMakingPeace(kingdomMakingPeace, otherKingdom, forcePlayerCharacterCosts);
            if (kingdomMakingPeace.Leader.IsHumanPlayerCharacter && !skipPlayerPrompts)
            {
                var hasFiefsRemaining = kingdomMakingPeace.Fiefs.Count > 0;
                var strArgs = new Dictionary<string, object>
                {
                    {"DENARS", diplomacyCost.GoldCost.Value},
                    {"INFLUENCE", diplomacyCost.InfluenceCost.Value},
                    {"ENEMY_KINGDOM", otherKingdom.Name}
                };

                if (hasFiefsRemaining)
                {
                    InformationManager.ShowInquiry(new InquiryData(
                        _TDefeatTitle.ToString(),
                        new TextObject("{=vLfbqXjq}Your armies and people are exhausted from the conflict with {ENEMY_KINGDOM} and have given up the fight. You must accept terms of defeat and pay war reparations of {DENARS} denars. The shame of defeat will also cost you {INFLUENCE} influence.", strArgs).ToString(),
                        true,
                        false,
                        GameTexts.FindText("str_ok").ToString(),
                        null,
                        () => ApplyPeace(kingdomMakingPeace, otherKingdom, skipPlayerPrompts: true),
                        null), true);
                }
                else
                {
                    InformationManager.ShowInquiry(new InquiryData(
                    _TDefeatTitle.ToString(),
                    new TextObject("{=ghZCj7hb}With your final stronghold falling to your enemies, you can no longer continue the fight with {ENEMY_KINGDOM}. You must accept terms of defeat and pay war reparations of {DENARS} denars. The shame of defeat will also cost you {INFLUENCE} influence.", strArgs).ToString(),
                    true,
                    false,
                    GameTexts.FindText("str_ok").ToString(),
                    null,
                    () => ApplyPeace(kingdomMakingPeace, otherKingdom, skipPlayerPrompts: true),
                    null), true);
                }
            }
            else if (!otherKingdom.Leader.IsHumanPlayerCharacter)
            {
                AcceptPeace(kingdomMakingPeace, otherKingdom, diplomacyCost);
            }
            else if (!CooldownManager.HasPeaceProposalCooldownWithPlayerKingdom(kingdomMakingPeace))
            {
                CreatePeaceInquiry(kingdomMakingPeace, otherKingdom, diplomacyCost);
            }
        }
    }
}