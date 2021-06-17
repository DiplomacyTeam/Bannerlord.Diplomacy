using SandBox;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;

namespace Diplomacy.Messengers
{
    internal sealed class MessengerManager : IMissionListener
    {
        private const float MessengerHourlySpeed = 20f;

        [SaveableField(1)]
        private List<Messenger> _messengers;

        public MBReadOnlyList<Messenger> Messengers { get; private set; }

        private Messenger? _activeMessenger;
        private Mission? _currentMission;

        internal MessengerManager()
        {
            _messengers = new List<Messenger>();
            Messengers = new MBReadOnlyList<Messenger>(_messengers);
        }

        private static string _SOK { get { return GameTexts.FindText("str_ok", null).ToString(); } }
        private static string _SMessengerSent { get; } = new TextObject("{=zv12jjyW}Messenger Sent").ToString();

        public void SendMessenger(Hero targetHero)
        {
            InformationManager.ShowInquiry(
                new InquiryData(_SMessengerSent,
                                GetMessengerSentText(Hero.MainHero.MapFaction,
                                                     targetHero.MapFaction,
                                                     targetHero,
                                                     Settings.Instance!.MessengerTravelTime).ToString(),
                                true,
                                false,
                                _SOK,
                                string.Empty,
                                delegate () { },
                                null), false);

            _messengers.Add(new Messenger(targetHero, CampaignTime.Now));
        }

        public void MessengerArrived()
        {
            foreach (var messenger in Messengers.ToList())
            {
                if (IsTargetHeroAvailable(messenger.TargetHero))
                    UpdateMessengerPosition(messenger);

                if (messenger.DispatchTime.ElapsedDaysUntilNow >= Settings.Instance!.MessengerTravelTime || messenger.Arrived)
                    if (MessengerArrived(messenger))
                        break;
            }
        }

        private static void UpdateMessengerPosition(Messenger messenger)
        {
            var targetHeroLocationPoint = messenger.TargetHero.GetMapPoint();

            if (messenger.CurrentPosition.Equals(default(Vec2)) || targetHeroLocationPoint is null)
                return;

            var targetHeroLocation = targetHeroLocationPoint.Position2D;
            var distanceToGo = targetHeroLocation - messenger.CurrentPosition;

            if (distanceToGo.Length <= MessengerHourlySpeed)
                messenger.Arrived = true;
            else
                messenger.CurrentPosition += distanceToGo.Normalized() * MessengerHourlySpeed;
        }

        private bool MessengerArrived(Messenger messenger)
        {
            if (messenger.TargetHero.PartyBelongedTo == Hero.MainHero.PartyBelongedTo)
            {
                // FIXME: Definitely would like to remove the silliness here.
                // Jiros: Added to catch crash when Hero and Target are in the same party. [v1.5.3-release]
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=uy86VZX2}Messenger Eaten").ToString(),
                                                               new TextObject("Oh no. The messenger was ambushed and eaten by a Grue while trying to reach " + messenger.TargetHero.Name).ToString(),
                                                               true,
                                                               false,
                                                               _SOK,
                                                               null,
                                                               delegate () { _messengers.Remove(messenger); },
                                                               null,
                                                               ""));
                return false;
            }
            if (IsTargetHeroAvailableNow(messenger.TargetHero) && IsPlayerHeroAvailable())
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
                _messengers.Remove(messenger);

            return false;
        }

        private static bool IsPlayerHeroAvailable()
        {
            return PartyBase.MainParty is not null
                && PlayerEncounter.Current is null
                && GameStateManager.Current.ActiveState is MapState mapState
                && !mapState.AtMenu;
        }

        internal void Sync() => Messengers = new MBReadOnlyList<Messenger>(_messengers);

        public void StartDialogue(Hero targetHero, Messenger messenger)
        {
            var heroParty = PartyBase.MainParty;

            if (targetHero.IsWanderer && targetHero.HeroState == Hero.CharacterStates.NotSpawned)
            {
                targetHero.ChangeState(Hero.CharacterStates.Active);
                EnterSettlementAction.ApplyForCharacterOnly(targetHero, targetHero.BornSettlement);
            }

            PartyBase? targetParty;
            if (targetHero.CurrentSettlement != null)
            {
                targetParty = targetHero.CurrentSettlement?.Party ?? targetHero.BornSettlement?.Party;
            }
            else
            {
                targetParty = targetHero.PartyBelongedTo?.Party ?? targetHero.BornSettlement?.Party;
            }

            Settlement? settlement = targetHero.CurrentSettlement;

            PlayerEncounter.Start();
            PlayerEncounter.Current.SetupFields(heroParty, targetParty ?? heroParty);

            Campaign.Current.CurrentConversationContext = ConversationContext.Default;
            if (settlement!= null && (LocationComplex.Current != null || Campaign.Current.CurrentMenuContext != null))
            {
                PlayerEncounter.EnterSettlement();
                Location locationOfCharacter = LocationComplex.Current.GetLocationOfCharacter(targetHero);
                CampaignEventDispatcher.Instance.OnPlayerStartTalkFromMenu(targetHero);
                _currentMission = (Mission) PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(locationOfCharacter, null, targetHero.CharacterObject, null);
            }
            else
            {
                var specialScene = "";
                var sceneLevels = "";

                _currentMission = (Mission)Campaign.Current.CampaignMissionManager.OpenConversationMission(
                new ConversationCharacterData(Hero.MainHero.CharacterObject, heroParty, true, false, false, false),
                new ConversationCharacterData(targetHero.CharacterObject, targetParty, true, false, false, false),
                specialScene, sceneLevels);
            }
            _currentMission.AddListener(this);
        }

        private TextObject GetMessengerArrivedText(IFaction faction1, IFaction faction2, Hero targetHero)
        {
            var textObject = new TextObject("{=YnRmSele}The messenger from {FACTION1_NAME} has arrived at {HERO_NAME} of {FACTION2_NAME}.");
            textObject.SetTextVariable("FACTION1_NAME", faction1.Name.ToString());
            textObject.SetTextVariable("FACTION2_NAME", faction2.Name.ToString());
            textObject.SetTextVariable("HERO_NAME", targetHero.Name.ToString());
            return textObject;
        }

        private TextObject GetMessengerSentText(IFaction faction1, IFaction faction2, Hero targetHero, int travelDays)
        {
            var textObject = new TextObject("{=qNWMZP0z}The messenger from {FACTION1_NAME} will arrive at {HERO_NAME} of {FACTION2_NAME} within {TRAVEL_TIME} days.");
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
            var available = opposingLeader.IsActive || (opposingLeader.IsWanderer && opposingLeader.HeroState == Hero.CharacterStates.NotSpawned);
            return available && !opposingLeader.IsHumanPlayerCharacter;
        }

        public static bool IsTargetHeroAvailableNow(Hero opposingLeader)
        {
            return IsTargetHeroAvailable(opposingLeader) && opposingLeader.PartyBelongedTo?.MapEvent == null;
        }

        public static bool CanSendMessengerWithCost(Hero opposingLeader, DiplomacyCost diplomacyCost)
        {
            var canPayCost = diplomacyCost.CanPayCost();
            return canPayCost && IsTargetHeroAvailable(opposingLeader);
        }

        public void OnEquipItemsFromSpawnEquipmentBegin(Agent agent, Agent.CreationType creationType) { }
        public void OnEquipItemsFromSpawnEquipment(Agent agent, Agent.CreationType creationType) { }

        public void OnEndMission()
        {
            _messengers.Remove(_activeMessenger!);
            _activeMessenger = null;
            _currentMission = null;
            CampaignEvents.TickEvent.AddNonSerializedListener(this, CleanUpSettlementEncounter);
        }

        public void OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
        {
            if (oldMissionMode == MissionMode.Conversation)
            {
                _currentMission!.EndMission();
            }
        }
        public void OnConversationCharacterChanged() { }
        public void OnResetMission() { }

        private void CleanUpSettlementEncounter(float obj)
        {
            PlayerEncounter.Finish();
#if e159
            CampaignEvents.RemoveListeners(this);
#else
            CampaignEventDispatcher.Instance.RemoveListeners(this);
#endif
        }
    }
}
