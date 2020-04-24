using SandBox;
using System.Linq;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace DiplomacyFixes.Messengers
{
    class MessengerManager : IMissionListener
    {

        private static MessengerManager _instance;
        public static MessengerManager Instance {
            get
            {
                if(_instance == null)
                {
                    _instance = new MessengerManager();
                }
                return _instance;
            }
        }

        public MBBindingList<Messenger> Messengers { get; }
        public bool CanStartMessengerConversation { get; private set; }
        private Messenger _activeMessenger;
        private MessengerManager()
        {
            Messengers = new MBBindingList<Messenger>();
            CanStartMessengerConversation = true;
        }

        public void SendMessenger(Hero targetHero)
        {
            Messengers.Add(new Messenger(targetHero, CampaignTime.Now));
            InformationManager.ShowInquiry(new InquiryData("Messenger Sent", GetMessengerSentText(Hero.MainHero.MapFaction, targetHero.MapFaction, Settings.Instance.MessengerTravelTime).ToString(), true, false, GameTexts.FindText("str_ok", null).ToString(), "", delegate ()
            {
            }, null, ""), false);
        }

        public void MessengerArrived(Messenger messenger)
        {
            if (CanStartMessengerConversation)
            {
                CanStartMessengerConversation = false;
                _activeMessenger = messenger;
                InformationManager.ShowInquiry(new InquiryData("Messenger Arrived", GetMessengerArrivedText(Hero.MainHero.MapFaction, messenger.TargetHero.MapFaction).ToString(), true, false, GameTexts.FindText("str_ok", null).ToString(), "", delegate ()
                {
                    StartDialogue(messenger.TargetHero);
                }, null, ""), true);

            }
        }

        public void StartDialogue(Hero targetHero)
        {
            Mission conversationMission = (Mission) Campaign.Current.CampaignMissionManager.OpenConversationMission(
                new ConversationCharacterData(Hero.MainHero.CharacterObject, null, false, false, false, false),
                new ConversationCharacterData(targetHero.CharacterObject, null, false, false, false, false), "", "");
            conversationMission.AddListener(this);
        }


        private TextObject GetMessengerArrivedText(IFaction faction1, IFaction faction2)
        {
            TextObject textObject = new TextObject("The messenger from {FACTION1_NAME} has arrived at {FACTION2_NAME}'s leader.", null);
            textObject.SetTextVariable("FACTION1_NAME", faction1.EncyclopediaLinkWithName);
            textObject.SetTextVariable("FACTION2_NAME", faction2.EncyclopediaLinkWithName);
            return textObject;
        }

        private TextObject GetMessengerSentText(IFaction faction1, IFaction faction2, int travelDays)
        {
            TextObject textObject = new TextObject("The messenger from {FACTION1_NAME} will arrive at {FACTION2_NAME}'s leader in {TRAVEL_DAYS} {DAY_WORD}.", null);
            textObject.SetTextVariable("FACTION1_NAME", faction1.EncyclopediaLinkWithName);
            textObject.SetTextVariable("FACTION2_NAME", faction2.EncyclopediaLinkWithName);
            textObject.SetTextVariable("TRAVEL_DAYS", travelDays);
            textObject.SetTextVariable("DAY_WORD", travelDays > 1 ? "days" : "day");
            return textObject;
        }

        public void SendMessengerWithInfluenceCost(Hero targetHero, float influenceCost)
        {
            DiplomacyCostManager.deductInfluenceFromPlayerClan(influenceCost);
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

        public void OnEquipItemsFromSpawnEquipmentBegin(Agent agent, Agent.CreationType creationType)
        {
        }

        public void OnEquipItemsFromSpawnEquipment(Agent agent, Agent.CreationType creationType)
        {
        }

        public void OnEndMission()
        {
            Messengers.Remove(_activeMessenger);
            _activeMessenger = null;
            CanStartMessengerConversation = true;
        }

        public void OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
        {
        }

        public void OnConversationCharacterChanged()
        {
        }

        public void OnResetMission()
        {
        }
    }
}
