using DiplomacyFixes.DiplomaticAction.NonAggressionPact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.CampaignEventBehaviors
{
    class NonAggressionPactBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, )
        }

        private void DailyTickClan(Clan clan)
        {
            if (clan.Leader == clan.Kingdom?.Leader && clan.Leader != Hero.MainHero)
            {
                Kingdom kingdom = clan.Kingdom;
                ConsiderFormingNonAggressionPacts(kingdom);
            }
        }

        private void ConsiderFormingNonAggressionPacts(Kingdom kingdom)
        {
            List<Kingdom> potentialPartners = 
                Kingdom.All.Where(otherKingdom => otherKingdom != kingdom).Where(otherKingdom => NAPactConditions.Instance.CanExecuteAction(kingdom, otherKingdom)).ToList();
            foreach (Kingdom potentialPartner in potentialPartners)
            {
                if (MBRandom.RandomFloat < 0.05f && AllianceScoringModel.ShouldFormAlliance(kingdom, potentialAlly))
                {
                    DeclareAllianceAction.Apply(kingdom, potentialAlly);
                }
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            throw new NotImplementedException();
        }
    }
}
