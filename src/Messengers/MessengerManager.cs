using System;
using System.Collections.Generic;
using System.Linq;
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
        private const float MessengerHourlySpeed = 20f;

        [SaveableField(1)]
        private List<Messenger> _messengers;

        public MBReadOnlyList<Messenger> Messengers { get; private set; }

        private Messenger _activeMessenger;

        internal MessengerManager()
        {
            _messengers = new List<Messenger>();
            Messengers = new MBReadOnlyList<Messenger>(_messengers);
        }

        public void SendMessenger(Hero targetHero)
        {
            InformationManager.ShowInquiry(new InquiryData(new TextObject("{=zv12jjyW}Messenger Sent").ToString(), GetMessengerSentText(Hero.MainHero.MapFaction, targetHero.MapFaction, targetHero, Settings.Instance.MessengerTravelTime).ToString(), true, false, GameTexts.FindText("str_ok", null).ToString(), "", delegate ()
            {
            }, null, ""), false);
            _messengers.Add(new Messenger(targetHero, CampaignTime.Now));
        }

        public void MessengerArrived()
        {
            foreach (Messenger messenger in Messengers.ToList())
            {
                if (IsTargetHeroAvailable(messenger.TargetHero))
                {
                    UpdateMessengerPosition(messenger);
                }

                if (messenger.DispatchTime.ElapsedDaysUntilNow >= Settings.Instance.MessengerTravelTime || messenger.Arrived)
                {
                    if (MessengerArrived(messenger))
                    {
                        break;
                    }
                }
            }
        }

        private static void UpdateMessengerPosition(Messenger messenger)
        {
            IMapPoint targetHeroLocationPoint = messenger.TargetHero.GetMapPoint();
            if (messenger.CurrentPosition.Equals(default(Vec2)) || targetHeroLocationPoint == null)
            {
                return;
            }

            Vec2 targetHeroLocation = targetHeroLocationPoint.Position2D;
            Vec2 distanceToGo = targetHeroLocation - messenger.CurrentPosition;

            if (distanceToGo.Length <= MessengerHourlySpeed)
            {
                messenger.Arrived = true;
            }
            else
            {
                messenger.CurrentPosition += distanceToGo.Normalized() * MessengerHourlySpeed;
            }
        }

        private bool MessengerArrived(Messenger messenger)
        {
            if (messenger.TargetHero.PartyBelongedTo == Hero.MainHero.PartyBelongedTo)
            {
                // Jiros: Added to catch crash when Hero and Target are in the same party. [v1.5.3-release]
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=uy86VZX2}Messenger Eaten").ToString(), new TextObject("Oh no. The messenger was ambushed and eaten by a Grue while trying to reach " + messenger.TargetHero.Name).ToString(), true, false, GameTexts.FindText("str_ok", null).ToString(), null, delegate () { _messengers.Remove(messenger); }, null, ""));
                return false;
            }
            if (IsTargetHeroAvailable(messenger.TargetHero) && IsPlayerHeroAvailable())
            {
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=uy86VZX2}Messenger Arrived").ToString(), GetMessengerArrivedText(Hero.MainHero.MapFaction, messenger.TargetHero.MapFaction, messenger.TargetHero).ToString(), true, true, GameTexts.FindText("str_ok", null).ToString(), new TextObject("{=kMjfN2fB}Cancel Messenger").ToString(), delegate ()
                {
                    _activeMessenger = messenger;
                    StartDialogue(messenger.TargetHero, messenger);
                },
                () =>
                {
                    _messengers.Remove(messenger);
                }, ""), true);
                return true;
            }
            else if (messenger.TargetHero.IsDead)
            {
                _messengers.Remove(messenger);
            }

            return false;
        }

        private static bool IsPlayerHeroAvailable()
        {
            MapState mapState = null;
            return PartyBase.MainParty != null
                && PlayerEncounter.Current == null
                && ((mapState = GameStateManager.Current.ActiveState as MapState) != null && !mapState.AtMenu);
        }

        internal void Sync()
        {
            Messengers = new MBReadOnlyList<Messenger>(_messengers);
        }

        public void StartDialogue(Hero targetHero, Messenger messenger)
        {

            PartyBase heroParty = PartyBase.MainParty;
            PartyBase targetParty = targetHero.PartyBelongedTo?.Party;

            bool isCivilian = false;
            if (targetParty == null)
            {
                targetParty = targetHero.CurrentSettlement?.Party ?? targetHero.BornSettlement?.Party;
                isCivilian = true;
            }

            PlayerEncounter.Start();
            PlayerEncounter.Current.SetupFields(heroParty, targetParty ?? heroParty);

            string specialScene = "";
            string sceneLevels = "";

            Campaign.Current.CurrentConversationContext = ConversationContext.Default;
            Mission conversationMission = (Mission)Campaign.Current.CampaignMissionManager.OpenConversationMission(
                new ConversationCharacterData(Hero.MainHero.CharacterObject, heroParty, true, false, false, isCivilian),
                new ConversationCharacterData(targetHero.CharacterObject, targetParty, true, false, false, isCivilian),
                specialScene, sceneLevels);
            conversationMission.AddListener(this);
        }

        private TextObject GetMessengerArrivedText(IFaction faction1, IFaction faction2, Hero targetHero)
        {
            TextObject textObject = new TextObject("{=YnRmSele}The messenger from {FACTION1_NAME} has arrived at {HERO_NAME} of {FACTION2_NAME}.");
            textObject.SetTextVariable("FACTION1_NAME", faction1.Name.ToString());
            textObject.SetTextVariable("FACTION2_NAME", faction2.Name.ToString());
            textObject.SetTextVariable("HERO_NAME", targetHero.Name.ToString());
            return textObject;
        }

        private TextObject GetMessengerSentText(IFaction faction1, IFaction faction2, Hero targetHero, int travelDays)
        {
            TextObject textObject = new TextObject("{=qNWMZP0z}The messenger from {FACTION1_NAME} will arrive at {HERO_NAME} of {FACTION2_NAME} within {TRAVEL_TIME} days.");
            textObject.SetTextVariable("FACTION1_NAME", faction1.Name.ToString());
            textObject.SetTextVariable("FACTION2_NAME", faction2.Name.ToString());
            textObject.SetTextVariable("HERO_NAME", targetHero.Name.ToString());
            textObject.SetTextVariable("TRAVEL_TIME", travelDays);
            return textObject;
        }

        public void SendMessengerWithCost(Hero targetHero, DiplomacyCost diplomacyCost)
        {
            diplomacyCost.ApplyCost();
            SendMessenger(targetHero);
        }

        public static bool IsTargetHeroAvailable(Hero opposingLeader)
        {
            bool unavailable = opposingLeader.IsOccupiedByAnEvent()
                || opposingLeader.IsHumanPlayerCharacter
                || !(opposingLeader.IsActive || (opposingLeader.IsWanderer && opposingLeader.HeroState == Hero.CharacterStates.NotSpawned));
            return !unavailable;
        }

        public static bool CanSendMessengerWithCost(Hero opposingLeader, DiplomacyCost diplomacyCost)
        {
            bool canPayCost = diplomacyCost.CanPayCost();
            return canPayCost && IsTargetHeroAvailable(opposingLeader);
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

            if (PlayerEncounter.Current != null)
            {
                PlayerEncounter.Finish();
            }
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
