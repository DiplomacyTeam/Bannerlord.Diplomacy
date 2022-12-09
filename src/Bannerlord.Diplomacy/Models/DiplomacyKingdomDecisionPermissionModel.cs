using Diplomacy.DiplomaticAction.WarPeace;

using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace Diplomacy.Models
{
    public class DiplomacyKingdomDecisionPermissionModel : KingdomDecisionPermissionModel
    {
        private readonly KingdomDecisionPermissionModel _previousModel;

        public DiplomacyKingdomDecisionPermissionModel(KingdomDecisionPermissionModel previousModel)
        {
            _previousModel = previousModel;
        }

        public override bool IsWarDecisionAllowedBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2, out TextObject reason)
        {
            var isWarDecisionAllowed = _previousModel.IsWarDecisionAllowedBetweenKingdoms(kingdom1, kingdom2, out reason);

            if (isWarDecisionAllowed)
            {
                var listExceptions = DeclareWarConditions.Instance.CanApplyExceptions(kingdom1, kingdom2, bypassCosts: true);
                if (listExceptions is not null && listExceptions.Any())
                {
                    reason = listExceptions.FirstOrDefault();
                    isWarDecisionAllowed = false;
                }
            }

            return isWarDecisionAllowed;
        }

        public override bool IsPeaceDecisionAllowedBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2, out TextObject reason)
        {
            var isPeaceDecisionAllowed = _previousModel.IsPeaceDecisionAllowedBetweenKingdoms(kingdom1, kingdom2, out reason);

            if (isPeaceDecisionAllowed)
            {
                var listExceptions = MakePeaceConditions.Instance.CanApplyExceptions(kingdom1, kingdom2, bypassCosts: true);
                if (listExceptions is not null && listExceptions.Any())
                {
                    reason = listExceptions.FirstOrDefault();
                    isPeaceDecisionAllowed = false;
                }
            }

            return isPeaceDecisionAllowed;
        }

        public override bool IsPolicyDecisionAllowed(PolicyObject policy) => _previousModel.IsPolicyDecisionAllowed(policy);
        public override bool IsAnnexationDecisionAllowed(Settlement annexedSettlement) => _previousModel.IsAnnexationDecisionAllowed(annexedSettlement);
        public override bool IsExpulsionDecisionAllowed(Clan expelledClan) => _previousModel.IsExpulsionDecisionAllowed(expelledClan);
        public override bool IsKingSelectionDecisionAllowed(Kingdom kingdom) => _previousModel.IsKingSelectionDecisionAllowed(kingdom);
    }
}