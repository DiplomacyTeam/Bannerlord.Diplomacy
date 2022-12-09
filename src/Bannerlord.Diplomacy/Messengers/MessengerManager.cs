using Diplomacy.Costs;

using JetBrains.Annotations;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;
using TaleWorlds.CampaignSystem.Map;
using System;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using static Bannerlord.ButterLib.Common.Helpers.LocalizationHelper;
using TaleWorlds.CampaignSystem.GameComponents;
using HarmonyLib.BUTR.Extensions;
using SandBox.Missions.MissionLogics.Arena;
using TaleWorlds.CampaignSystem.CharacterDevelopment;

namespace Diplomacy.Messengers
{
    internal sealed class MessengerManager : IMissionListener
    {
        private static float MessengerHourlySpeedMin = 5f;
        private static float MessengerHourlySpeed = 5f;

        private static readonly List<MissionMode> AllowedMissionModes = new() { MissionMode.Conversation, MissionMode.Barter };

        private static readonly TextObject _TMessengerSent = new("{=zv12jjyW}Messenger Sent");

        private Vec2 _position2D = Vec2.Invalid;
        private Messenger? _activeMessenger;
        private Mission? _currentMission;

        private delegate int GetBribeInternalDelegate(DefaultBribeCalculationModel instance, Settlement settlement);
        private static readonly GetBribeInternalDelegate? deGetBribeInternal = AccessTools2.GetDelegate<GetBribeInternalDelegate>(typeof(DefaultBribeCalculationModel), "GetBribeInternal");

        [SaveableField(1)][UsedImplicitly] private List<Messenger> _messengers;

        public MBReadOnlyList<Messenger> Messengers { get; private set; }

        internal MessengerManager()
        {
            _messengers = new List<Messenger>();
            Messengers = new MBReadOnlyList<Messenger>(_messengers);
        }

        public void OnEndMission()
        {
            _messengers.Remove(_activeMessenger!);
            _activeMessenger = null;

            _currentMission!.RemoveListener(this);
            _currentMission = null;
            
            CampaignEvents.TickEvent.AddNonSerializedListener(this, CleanUpSettlementEncounter);
        }

        public void OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
        {
            if (!AllowedMissionModes.Contains(_currentMission!.Mode) && AllowedMissionModes.Contains(oldMissionMode)) _currentMission!.EndMission();
        }

        public void SendMessenger(Hero targetHero)
        {
            InformationManager.ShowInquiry(
                new InquiryData(_TMessengerSent.ToString(),
                    GetMessengerSentText(Hero.MainHero.MapFaction,
                        targetHero.MapFaction,
                        targetHero,
                        Settings.Instance!.MessengerTravelTime).ToString(),
                    true,
                    false,
                    GameTexts.FindText("str_ok").ToString(),
                    string.Empty,
                    delegate { },
                    null));

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
                    new TextObject("{=JmgAuONd}Oh no. The messenger was ambushed and eaten by a Grue while trying to reach {HERO_NAME}!", new() { ["HERO_NAME"] = messenger.TargetHero.Name })
                        .ToString(),
                    true,
                    false,
                    GameTexts.FindText("str_ok").ToString(),
                    null,
                    delegate { _messengers.Remove(messenger); },
                    null));
                return false;
            }

            if (IsTargetHeroAvailableNow(messenger.TargetHero) && IsPlayerHeroAvailable())
            {
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=uy86VZX2}Messenger Arrived").ToString(),
                    GetMessengerArrivedText(Hero.MainHero.MapFaction, messenger.TargetHero.MapFaction, messenger.TargetHero, out var additionalExpenses).ToString(), additionalExpenses.CanPayCost(), true,
                    GameTexts.FindText("str_ok").ToString(), new TextObject("{=kMjfN2fB}Cancel Messenger").ToString(), delegate
                    {
                        _activeMessenger = messenger;
                        int bribeValue = (int) additionalExpenses.Value;
                        if (bribeValue > 0)
                            BribeGuardsAction.Apply(messenger.TargetHero.CurrentSettlement, bribeValue);
                        StartDialogue(messenger.TargetHero, messenger);
                    },
                    () => { _messengers.Remove(messenger); }), true);
                return true;
            }

            if (messenger.TargetHero.IsDead)
            {
                _messengers.Remove(messenger);
            }

            return false;
        }

        private static bool IsPlayerHeroAvailable()
        {
            return PartyBase.MainParty is not null
                   && PlayerEncounter.Current is null
                   && GameStateManager.Current.ActiveState is MapState { AtMenu: false };
        }

        internal void Sync()
        {
            Messengers = new MBReadOnlyList<Messenger>(_messengers);
        }

        public void StartDialogue(Hero targetHero, Messenger messenger)
        {
            var heroParty = PartyBase.MainParty;

            if (targetHero.IsWanderer && targetHero.HeroState == Hero.CharacterStates.NotSpawned)
            {
                targetHero.ChangeState(Hero.CharacterStates.Active);
                EnterSettlementAction.ApplyForCharacterOnly(targetHero, targetHero.BornSettlement);
            }

            PartyBase? targetParty;
            var targetSettlement = targetHero.CurrentSettlement;
            if (targetSettlement != null)
                targetParty = targetSettlement.Party ?? targetHero.BornSettlement?.Party;
            else
                targetParty = targetHero.PartyBelongedTo?.Party ?? targetHero.BornSettlement?.Party;

            PlayerEncounter.Start();
            PlayerEncounter.Current.SetupFields(heroParty, targetParty ?? heroParty);

            Campaign.Current.CurrentConversationContext = ConversationContext.Default;
            if (targetSettlement != null)
            {
                _position2D = new(Hero.MainHero.GetMapPoint().Position2D);

                PlayerEncounter.EnterSettlement();

                var locationOfTarget = LocationComplex.Current.GetLocationOfCharacter(targetHero);
                var locationOfCharacter = LocationComplex.Current.GetLocationOfCharacter(Hero.MainHero);

                CampaignEventDispatcher.Instance.OnPlayerStartTalkFromMenu(targetHero);
                _currentMission =
                    (Mission) PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(locationOfTarget, locationOfCharacter, targetHero.CharacterObject);
            }
            else
            {
                _position2D = Vec2.Invalid;

                var specialScene = "";
                var sceneLevels = "";

                _currentMission = (Mission) Campaign.Current.CampaignMissionManager.OpenConversationMission(
                    new ConversationCharacterData(Hero.MainHero.CharacterObject, heroParty, true),
                    new ConversationCharacterData(targetHero.CharacterObject, targetParty, true),
                    specialScene, sceneLevels);
            }
            _currentMission.AddListener(this);
        }

        private TextObject GetMessengerArrivedText(IFaction faction1, IFaction faction2, Hero targetHero, out GoldCost additionalExpenses)
        {
            bool requiresBribing = false;
            if ((targetHero.CurrentSettlement?.IsTown ?? false) && targetHero.IsLord && !targetHero.IsPrisoner)
            {
                Campaign.Current.Models.SettlementAccessModel.CanMainHeroEnterLordsHall(targetHero.CurrentSettlement, out var accessDetails);
                requiresBribing = (accessDetails.AccessLevel == SettlementAccessModel.AccessLevel.NoAccess || accessDetails.AccessLevel == SettlementAccessModel.AccessLevel.LimitedAccess);
            }

            var textObject = new TextObject("{=YnRmSele}The messenger from {FACTION1_NAME} has arrived at {ADDRESSEE_TEXT}.{BRIBE_TEXT}");
            TextObject addressee = new("{=vGyBQeEk}{HERO_NAME}{?HAS_FACTION} of {FACTION2_NAME}{?}{\\?}", new()
                {
                    ["HERO_NAME"] = targetHero.Name,
                    ["HAS_FACTION"] = new TextObject(faction2 != null ? 1 : 0),
                    ["FACTION2_NAME"] = faction2?.Name ?? TextObject.Empty
            });
            TextObject bribeTextObject;
            if (requiresBribing)
            {
                bribeTextObject = new("{=sQxAljGF}{NEW_LINE} {NEW_LINE}Unfortunately, the messenger is denied access to the keep, but can try to bribe the guards. It would cost {?IS_FEMALE}her{?}him{\\?} {GOLD_COST}{GOLD_ICON} and you will have to cover the expenses{?CAN_AFFORD}.{?}, which you can't afford at the moment.{\\?}");
                bribeTextObject.SetTextVariable("NEW_LINE", Environment.NewLine);
                bribeTextObject.SetTextVariable("IS_FEMALE", Hero.MainHero.IsFemale ? 1 : 0);

                var model = (Campaign.Current.Models.BribeCalculationModel as DefaultBribeCalculationModel);
                additionalExpenses = new(Hero.MainHero, null, deGetBribeInternal!(model ?? new DefaultBribeCalculationModel(), targetHero.CurrentSettlement!));

                bribeTextObject.SetTextVariable("GOLD_COST", (int) additionalExpenses.Value);
                bribeTextObject.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">");
                bribeTextObject.SetTextVariable("CAN_AFFORD", additionalExpenses.CanPayCost() ? 1 : 0);
            }
            else
            {
                bribeTextObject = TextObject.Empty;
                additionalExpenses = new(Hero.MainHero, null, 0);
            }

            textObject.SetTextVariable("FACTION1_NAME", faction1.Name.ToString());
            textObject.SetTextVariable("ADDRESSEE_TEXT", addressee.ToString());
            textObject.SetTextVariable("BRIBE_TEXT", bribeTextObject.ToString());

            return textObject;
        }

        private TextObject GetMessengerSentText(IFaction faction1, IFaction faction2, Hero targetHero, int travelDays)
        {
            TextObject textObject = new("{=qNWMZP0z}The messenger from {FACTION1_NAME} will arrive at {ADDRESSEE_TEXT} within {TRAVEL_TIME} days.");
            textObject.SetTextVariable("FACTION1_NAME", faction1.Name.ToString());
            TextObject addressee = new("{=vGyBQeEk}{HERO_NAME}{?HAS_FACTION} of {FACTION2_NAME}{?}{\\?}", new()
            {
                ["HERO_NAME"] = targetHero.Name,
                ["HAS_FACTION"] = new TextObject(faction2 != null ? 1 : 0),
                ["FACTION2_NAME"] = faction2?.Name ?? TextObject.Empty
            });
            textObject.SetTextVariable("ADDRESSEE_TEXT", addressee.ToString());
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
            var available = opposingLeader.IsActive || opposingLeader.IsWanderer && opposingLeader.HeroState == Hero.CharacterStates.NotSpawned;
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

        private void CleanUpSettlementEncounter(float obj)
        {
            PlayerEncounter.Finish();

            if (_position2D != Vec2.Invalid)
            {
                MobileParty.MainParty.Position2D = _position2D;                
            }
            _position2D = Vec2.Invalid;

            RemoveThisFromListeners();
        }

        internal void OnAfterSaveLoad(float obj)
        {
            //Cleanup possible faulty encounters
            if (PlayerEncounter.Current is PlayerEncounter playerEncounter && PlayerEncounter.LeaveEncounter
                && FieldAccessHelper.PlayerEncounterAttackerPartyByRef(playerEncounter) is PartyBase attackerParty
                && attackerParty == Hero.MainHero.PartyBelongedTo?.Party
                && !(FieldAccessHelper.PlayerEncounterDefenderPartyByRef(playerEncounter) is PartyBase defenderParty && defenderParty.IsSettlement))
            {
                PlayerEncounter.Finish();
            }
            //Recalculate speed
            MessengerHourlySpeed = Math.Max(Campaign.MapDiagonal / (CampaignTime.HoursInDay * Math.Max(Settings.Instance!.MessengerTravelTime, 0.5f)), MessengerHourlySpeedMin);
            //Remove from listeners
            RemoveThisFromListeners();
        }

        private void RemoveThisFromListeners()
        {
            CampaignEventDispatcher.Instance.RemoveListeners(this);
        }

        public void OnEquipItemsFromSpawnEquipmentBegin(Agent agent, Agent.CreationType creationType) { }

        public void OnEquipItemsFromSpawnEquipment(Agent agent, Agent.CreationType creationType) { }

        public void OnConversationCharacterChanged() { }

        public void OnResetMission() { }

        public void OnInitialDeploymentPlanMade(BattleSideEnum battleSide, bool isFirstPlan) { }
    }
}