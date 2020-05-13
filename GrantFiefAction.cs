using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace DiplomacyFixes
{
    class GrantFiefAction
    {
        public static void Apply(Settlement settlement, Clan clan)
        {
            ChangeOwnerOfSettlementAction.ApplyByDefault(clan.Leader, settlement);
            Events.Instance.OnFiefGranted(settlement.Town);
        }
    }
}
