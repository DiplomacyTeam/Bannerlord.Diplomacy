using Diplomacy.DiplomaticAction;
using Diplomacy.Extensions;
using Diplomacy.PatchTools;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;

namespace Diplomacy.Patches
{
    class TributeClanFinancePatch : PatchClass<TributeClanFinancePatch, DefaultClanFinanceModel>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Postfix(nameof(CalculateTributeIncome), nameof(DefaultClanFinanceModel.CalculateClanGoldChange)),
            new Postfix(nameof(CalculateTributeExpenses), nameof(DefaultClanFinanceModel.CalculateClanExpenses)),
        };

        private static void CalculateTributeIncome(Clan clan, bool includeDescriptions, bool applyWithdrawals, ref ExplainedNumber __result)
        {
            if (clan.MapFaction is Kingdom kingdom && kingdom.RulingClan == clan)
            {
                var income = 0;
                foreach (var otherKingdom in Kingdom.All.Where(x => !x.IsEliminated && !x.IsRebelKingdom() && x != kingdom))
                {
                    if (DiplomaticAgreementManager.Instance!.Agreements.TryGetValue(new FactionPair(kingdom, otherKingdom), out List<DiplomaticAgreement> agreements))
                    {
                        var tributeAgreements = agreements.OfType<TributeAgreement>().Where(x => !x.IsExpired());

                        foreach (var agreement in tributeAgreements)
                        {
                            agreement.RegisterPayment(ref __result, kingdom, applyWithdrawals);
                        }
                    }
                }

                __result.Add(income, new TextObject("Tributes test"));
            }
        }

        private static void CalculateTributeExpenses(Clan clan, ref ExplainedNumber __result)
        {

        }
    }
}