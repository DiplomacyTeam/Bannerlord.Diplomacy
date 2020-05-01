using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;

namespace DiplomacyFixes.Messengers
{
    [SaveableClass(4)]
    class MessengerManager : IMissionListener
    {
        [SaveableField(1)]
        private List<Messenger> _messengers;
        
        public MBReadOnlyList<Messenger> Messengers { get; private set; }

        public bool CanStartMessengerConversation { get; private set; }
        private Messenger _activeMessenger;
        internal MessengerManager()
        {
            _messengers = new List<Messenger>();
            Messengers = new MBReadOnlyList<Messenger>(_messengers);
            CanStartMessengerConversation = true;
        }

        public void SendMessenger(Hero targetHero)
        {
            InformationManager.ShowInquiry(new InquiryData(new TextObject("{=zv12jjyW}Messenger Sent").ToString(), GetMessengerSentText(Hero.MainHero.MapFaction, targetHero.MapFaction, Settings.Instance.MessengerTravelTime).ToString(), true, false, GameTexts.FindText("str_ok", null).ToString(), "", delegate ()
            {
            }, null, ""), false);
            _messengers.Add(new Messenger(targetHero, CampaignTime.Now));
        }

        public void MessengerArrived(Messenger messenger)
        {
            if (CanStartMessengerConversation)
            {
                CanStartMessengerConversation = false;
                _activeMessenger = messenger;
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=uy86VZX2}Messenger Arrived").ToString(), GetMessengerArrivedText(Hero.MainHero.MapFaction, messenger.TargetHero.MapFaction).ToString(), true, false, GameTexts.FindText("str_ok", null).ToString(), "", delegate ()
                {
                    StartDialogue(messenger.TargetHero);
                }, null, ""), true);

            }
        }

        internal void Sync()
        {
            Messengers = new MBReadOnlyList<Messenger>(_messengers);
            CanStartMessengerConversation = true;
        }

        public void StartDialogue(Hero targetHero)
        {
            Mission conversationMission = (Mission)Campaign.Current.CampaignMissionManager.OpenConversationMission(
                new ConversationCharacterData(Hero.MainHero.CharacterObject, null, false, false, false, false),
                new ConversationCharacterData(targetHero.CharacterObject, null, false, false, false, false), "", "");
            conversationMission.AddListener(this);
        }


        private TextObject GetMessengerArrivedText(IFaction faction1, IFaction faction2)
        {
            TextObject textObject = new TextObject("{=YnRmSele}The messenger from {FACTION1_NAME} has arrived at the {FACTION2_NAME} leader.");
            textObject.SetTextVariable("FACTION1_NAME", faction1.Name.ToString());
            textObject.SetTextVariable("FACTION2_NAME", faction2.Name.ToString());
            return textObject;
        }

        private TextObject GetMessengerSentText(IFaction faction1, IFaction faction2, int travelDays)
        {
            TextObject textObject = new TextObject("{=qNWMZP0z}The messenger from {FACTION1_NAME} will arrive at the {FACTION2_NAME} leader in {TRAVEL_TIME} days.");
            textObject.SetTextVariable("FACTION1_NAME", faction1.Name.ToString());
            textObject.SetTextVariable("FACTION2_NAME", faction2.Name.ToString());
            textObject.SetTextVariable("TRAVEL_TIME", travelDays);
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
            _messengers.Remove(_activeMessenger);
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
