using Diplomacy.CivilWar.Actions;
using Diplomacy.Extensions;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.CampaignBehaviors
{
    public class FactionConversationsCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, AddConversations);
        }

        private void AddConversations(CampaignGameStarter starter)
        {
            starter.AddPlayerLine("clan_join_faction", "lord_talk_speak_diplomacy_2", "lord_considers_faction_join", "{=7pojTm6C}Things are not going well in the kingdom. Will you join my faction, the {REBEL_FACTION_NAME}?", PlayerHasJoinableFaction, null);
            starter.AddDialogLine("lord_redirects_faction_join", "lord_considers_faction_join", "lord_pretalk", "{=1i9KSLAv}Please speak to {CLAN_LEADER}, the leader of our clan.", IsNotClanLeader, null);
            starter.AddDialogLine("lord_declines_faction_join_king", "lord_considers_faction_join", "lord_pretalk", "{=IIrplH6r}This is my kingdom. Rise up against me and you will regret it.", IsKing, null);
            starter.AddDialogLine("lord_declines_faction_join", "lord_considers_faction_join", "lord_pretalk", "{=xQt3pAau}The {REBEL_FACTION_NAME} does not align with my interests.", WillNotJoinFaction, null);
            starter.AddDialogLine("lord_accepts_faction_join", "lord_considers_faction_join", "lord_pretalk", "{=ps5PVvUs}I will gladly join the {REBEL_FACTION_NAME}!", WillJoinFaction, JoinFaction);
        }

        private void JoinFaction()
        {
            var kingdom = Clan.PlayerClan.Kingdom;
            var faction = kingdom.GetRebelFactions().First(x => x.SponsorClan == Clan.PlayerClan);
            JoinFactionAction.Apply(Hero.OneToOneConversationHero.Clan, faction);
        }

        private bool IsKing()
        {
            return Hero.OneToOneConversationHero.IsFactionLeader;
        }

        private bool IsNotClanLeader()
        {
            if (Hero.OneToOneConversationHero.Clan.Leader != Hero.OneToOneConversationHero)
            {
                MBTextManager.SetTextVariable("CLAN_LEADER", Hero.OneToOneConversationHero.Clan.Leader.Name);
                return true;
            }

            return false;
        }

        private bool WillNotJoinFaction()
        {
            return !IsKing() && Hero.OneToOneConversationHero.Clan.Leader == Hero.OneToOneConversationHero && !WillJoinFaction();
        }

        private bool WillJoinFaction()
        {

            var kingdom = Clan.PlayerClan.Kingdom;
            var faction = kingdom.GetRebelFactions().First(x => x.SponsorClan == Clan.PlayerClan);
            MBTextManager.SetTextVariable("REBEL_FACTION_NAME", faction.Name);
            return Hero.OneToOneConversationHero.Clan.Leader == Hero.OneToOneConversationHero && JoinFactionAction.ShouldApply(Hero.OneToOneConversationHero.Clan, faction);
        }

        private bool PlayerHasJoinableFaction()
        {
            if (Clan.PlayerClan.MapFaction is Kingdom kingdom && Hero.OneToOneConversationHero.MapFaction == kingdom)
            {
                var faction = kingdom.GetRebelFactions().FirstOrDefault(x => x.SponsorClan == Clan.PlayerClan && !x.Clans.Contains(Hero.OneToOneConversationHero.Clan) && !x.AtWar);
                if (faction != null)
                {
                    MBTextManager.SetTextVariable("REBEL_FACTION_NAME", faction.Name);
                    return true;
                }
            }

            return false;
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}
