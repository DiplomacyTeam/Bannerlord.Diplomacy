using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes
{
    class MessengerUtil
    {
		public static void SendMessenger(Hero targetHero)
		{
			Campaign.Current.CampaignMissionManager.OpenConversationMission(
				new ConversationCharacterData(Hero.MainHero.CharacterObject, null, false, false, false, false),
				new ConversationCharacterData(targetHero.CharacterObject, null, false, false, false, false), "", "");
		}

		public static void SendMessengerWithInfluenceCost(Hero targetHero, float influenceCost)
		{
			CostUtil.deductInfluenceFromPlayerClan(influenceCost);
			SendMessenger(targetHero);
		}

		public static bool CanSendMessenger(Hero opposingLeader)
		{
			bool unavailable = opposingLeader.IsDead
				|| opposingLeader.IsOccupiedByAnEvent()
				|| opposingLeader.IsPrisoner;
			return !unavailable;
		}

		public static bool CanSendMessengerWithInfluenceCost(Hero opposingLeader, float influenceCost)
		{
			return Clan.PlayerClan.Influence >= influenceCost && CanSendMessenger(opposingLeader);
		}
	}
}
