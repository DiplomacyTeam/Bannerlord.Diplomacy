using Diplomacy.Costs;
using Diplomacy.Helpers;

using HarmonyLib.BUTR.Extensions;

using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;

namespace Diplomacy.Messengers
{
    internal sealed class MessengerManager : IMissionListener
    {
        private static float MessengerHourlySpeedMin = 2f;
        private static float MessengerHourlySpeed = 5f;

        private static readonly List<MissionMode> AllowedMissionModes = new() { MissionMode.Conversation, MissionMode.Barter };

        private static readonly TextObject _TMessengerSent = new("{=zv12jjyW}Messenger Sent");

        private Vec2 _position2D = Vec2.Invalid;
        private Messenger? _activeMessenger;
        private Mission? _currentMission;
        private int _cachedMessengerTravelTime;

        private delegate int GetBribeInternalDelegate(DefaultBribeCalculationModel instance, Settlement settlement);
        private static readonly GetBribeInternalDelegate? deGetBribeInternal = AccessTools2.GetDelegate<GetBribeInternalDelegate>(typeof(DefaultBribeCalculationModel), "GetBribeInternal");

        [SaveableField(1)][UsedImplicitly] private List<Messenger> _messengers;

        public MBReadOnlyList<Messenger> Messengers { get; private set; }

        internal MessengerManager()
        {
            _messengers = new();
            Messengers = new(_messengers);
        }

        public void OnEndMission()
        {
            RemoveMessenger(_activeMessenger!);
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
            AddMessenger(targetHero);
        }

        private void AddMessenger(Hero targetHero)
        {
            _messengers.Add(new Messenger(targetHero, CampaignTime.Now));
            Messengers = new(_messengers);
        }

        internal void CheckForAccidents()
        {
            if (!Settings.Instance!.EnableMessengerAccidents)
                return;

            foreach (var messenger in Messengers.Where(m => !m.Arrived).ToList())
            {
                if (MessengerHasAccident(messenger))
                    break;
            }
        }

        private bool MessengerHasAccident(Messenger messenger)
        {
            var accidentHappened = MBRandom.RandomFloat < 0.005f;
            if (accidentHappened)
            {
                //FIXME: Need different random events
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=nYrezEOX}Messenger killed").ToString(),
                    new TextObject("{=A5lug0JY}Your messenger was ambushed and killed by highwaymen while trying to reach {HERO NAME}!", new() { ["HERO_NAME"] = messenger.TargetHero.Name })
                        .ToString(),
                    true,
                    false,
                    GameTexts.FindText("str_ok").ToString(),
                    null,
                    delegate { RemoveMessenger(messenger); },
                    null));
            }
            return accidentHappened;
        }

        private void RemoveMessenger(Messenger messenger)
        {
            _messengers.Remove(messenger);
            Messengers = new(_messengers);
        }

        public void UpdateMessengerPositions()
        {
            SyncMessengerHourlySpeed();
            foreach (var messenger in Messengers.ToList())
            {
                if (IsTargetHeroAvailable(messenger.TargetHero) && !messenger.Arrived)
                    UpdateMessengerPosition(messenger);

                if (messenger.Arrived && MessengerArrived(messenger))
                    break;
            }
        }

        private void SyncMessengerHourlySpeed(bool forceRecalculation = false)
        {
            if (!forceRecalculation && _cachedMessengerTravelTime == Settings.Instance!.MessengerTravelTime)
                return;

            MessengerHourlySpeed = Math.Max(Campaign.MapDiagonal / (CampaignTime.HoursInDay * Math.Max(Settings.Instance!.MessengerTravelTime, 0.5f) * 1.5f), MessengerHourlySpeedMin);
            _cachedMessengerTravelTime = Settings.Instance!.MessengerTravelTime;
        }

        private static void UpdateMessengerPosition(Messenger messenger)
        {
            if (messenger.DispatchTime.ElapsedDaysUntilNow >= Settings.Instance!.MessengerTravelTime)
            {
                messenger.Arrived = true;
                return;
            }

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
                    delegate { RemoveMessenger(messenger); },
                    null));
                return false;
            }

            if (IsTargetHeroAvailableNow(messenger.TargetHero) && IsPlayerHeroAvailable())
            {
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=uy86VZX2}Messenger Arrived").ToString(),
                    GetMessengerArrivedText(Hero.MainHero.MapFaction, messenger.TargetHero.MapFaction, messenger.TargetHero, out var additionalExpenses).ToString(), additionalExpenses?.CanPayCost() ?? true, true,
                    GameTexts.FindText("str_ok").ToString(), new TextObject("{=kMjfN2fB}Cancel Messenger").ToString(), delegate
                    {
                        _activeMessenger = messenger;
                        int bribeValue = (int) (additionalExpenses?.Value ?? 0f);
                        if (bribeValue > 0)
                            BribeGuardsAction.Apply(messenger.TargetHero.CurrentSettlement, bribeValue);
                        StartDialogue(messenger.TargetHero, messenger);
                    },
                    () => { RemoveMessenger(messenger); }), true);
                return true;
            }

            if (messenger.TargetHero.IsDead)
            {
                RemoveMessenger(messenger);
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
            Messengers = new(_messengers);
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

        private TextObject GetMessengerArrivedText(IFaction faction1, IFaction faction2, Hero targetHero, out GoldCost? additionalExpenses)
        {
            bool requiresBribing = false;
            additionalExpenses = null;

            if ((targetHero.CurrentSettlement?.IsTown ?? false) && targetHero.IsLord && !targetHero.IsPrisoner)
            {
                Campaign.Current.Models.SettlementAccessModel.CanMainHeroEnterLordsHall(targetHero.CurrentSettlement, out var accessDetails);
                requiresBribing = (accessDetails.AccessLevel == SettlementAccessModel.AccessLevel.NoAccess || accessDetails.AccessLevel == SettlementAccessModel.AccessLevel.LimitedAccess);

                if (requiresBribing)
                {
                    var model = (Campaign.Current.Models.BribeCalculationModel as DefaultBribeCalculationModel);
                    additionalExpenses = new(Hero.MainHero, deGetBribeInternal!(model ?? new DefaultBribeCalculationModel(), targetHero.CurrentSettlement!));
                }
            }

            var textObject = new TextObject("{=YnRmSele}The messenger from {FACTION1_NAME} has arrived at {ADDRESSEE_TEXT}.{BRIBE_TEXT}");
            TextObject addressee = new("{=vGyBQeEk}{HERO_NAME}{?HAS_FACTION} of {FACTION2_NAME}{?}{\\?}", new()
            {
                ["HERO_NAME"] = targetHero.Name,
                ["HAS_FACTION"] = new TextObject(faction2 != null ? 1 : 0),
                ["FACTION2_NAME"] = faction2?.Name ?? TextObject.Empty
            });
            TextObject bribeTextObject;
            if (requiresBribing && additionalExpenses?.Value > 0)
            {
                bribeTextObject = new("{=sQxAljGF}{NEW_LINE} {NEW_LINE}Unfortunately, the messenger is denied access to the keep, but can try to bribe the guards. It would cost {?IS_FEMALE}her{?}him{\\?} {GOLD_COST}{GOLD_ICON} and you will have to cover the expenses{?CAN_AFFORD}.{?}, which you can't afford at the moment.{\\?}");
                bribeTextObject.SetTextVariable("NEW_LINE", Environment.NewLine);
                bribeTextObject.SetTextVariable("IS_FEMALE", Hero.MainHero.IsFemale ? 1 : 0);
                bribeTextObject.SetTextVariable("GOLD_COST", (int) additionalExpenses.Value);
                bribeTextObject.SetTextVariable("GOLD_ICON", StringConstants.GoldIcon);
                bribeTextObject.SetTextVariable("CAN_AFFORD", additionalExpenses.CanPayCost() ? 1 : 0);
            }
            else
                bribeTextObject = TextObject.Empty;

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

        public void SendMessengerWithCost(Hero targetHero, GoldCost diplomacyCost)
        {
            diplomacyCost.ApplyCost();
            SendMessenger(targetHero);
        }

        public static bool IsTargetHeroAvailable(Hero targetHero)
        {
            var available = targetHero.IsActive || targetHero.IsWanderer && targetHero.HeroState == Hero.CharacterStates.NotSpawned;
            return available && !targetHero.IsHumanPlayerCharacter;
        }

        public static bool IsTargetHeroAvailable(Hero targetHero, out TextObject exception)
        {
            if (targetHero.IsHumanPlayerCharacter)
            {
                exception = new("{=hPra5uwZ}The messenger either does not understand or does not like your joke and refuses the task.");
                return false;
            }

            if (Settings.Instance!.EnableMessengerRestictions && !HeroIsKnownToPlayer(targetHero))
            {
                exception = new("{=1mlXBnSs}You know too little of {HERO_NAME} to point them out for a messenger.", new() { ["HERO_NAME"] = targetHero.Name });
                return false;
            }

            var available = targetHero.IsActive || targetHero.IsWanderer && targetHero.HeroState == Hero.CharacterStates.NotSpawned;
            if (!available)
            {
                exception = new("{=bLR91Eob}{REASON}The messenger won't be able to reach the addressee.");
                var reason = GetUnavailabilityReason(targetHero);
                exception.SetTextVariable("REASON", reason);
                return false;
            }

            exception = TextObject.Empty;
            return true;
        }

        private static bool HeroIsKnownToPlayer(Hero hero)
        {
#if v100 || v101 || v102 || v103
            if (hero.HasMet || (hero.Clan != null && hero.Clan == Clan.PlayerClan))
                return true;
            if (hero.MapFaction is Kingdom heroKingdom && (heroKingdom.Leader == hero || (Clan.PlayerClan.MapFaction is Kingdom playerKingdom && playerKingdom == heroKingdom && hero.Clan is Clan heroClan && heroClan.Tier >= 4 && hero == heroClan.Leader)))
                return true;
            return false;
#else
            return Campaign.Current.Models.InformationRestrictionModel.DoesPlayerKnowDetailsOf(hero);
#endif
        }

        private static TextObject GetUnavailabilityReason(Hero targetHero)
        {
            TextObject reason;
            if (targetHero.IsDead)
                reason = new("{=vhsHDMil}{HERO_NAME} is dead. ", new() { ["HERO_NAME"] = targetHero.Name });
            else if (targetHero.IsPrisoner && targetHero.PartyBelongedToAsPrisoner != null)
                reason = new("{=CusN2JMb}{HERO_NAME} is imprisoned {?IS_MOBILE}by{?}in{\\?} {DETENTION_PLACE}. ", new()
                {
                    ["HERO_NAME"] = targetHero.Name,
                    ["IS_MOBILE"] = targetHero.PartyBelongedToAsPrisoner.IsSettlement ? 0 : 1,
                    ["DETENTION_PLACE"] = targetHero.PartyBelongedToAsPrisoner.IsSettlement ? targetHero.PartyBelongedToAsPrisoner.Settlement.Name : ((targetHero.PartyBelongedToAsPrisoner.LeaderHero?.Name ?? targetHero.PartyBelongedToAsPrisoner.Name) ?? TextObject.Empty)
                });
            else if (targetHero.IsFugitive)
                reason = new("{=1BISlFYx}{HERO_NAME} is fugitive and doesn't want to be found. ", new() { ["HERO_NAME"] = targetHero.Name });
            else if (targetHero.IsReleased)
                reason = new("{=ze8KJ1oL}{HERO_NAME} has just been released from custody and is not yet ready to make appointments. ", new() { ["HERO_NAME"] = targetHero.Name });
            else if (targetHero.IsTraveling)
                reason = new("{=N5T6IXWJ}{HERO_NAME} is traveling incognito. ", new() { ["HERO_NAME"] = targetHero.Name });
            else if (targetHero.IsChild)
                reason = new("{=3lknR86H}{HERO_NAME} is too inexperienced to participate in formal meetings. ", new() { ["HERO_NAME"] = targetHero.Name });
            else
                reason = TextObject.Empty;
            return reason;
        }

        public static bool IsTargetHeroAvailableNow(Hero targetHero)
        {
            return IsTargetHeroAvailable(targetHero) && targetHero.PartyBelongedTo?.MapEvent == null;
        }

        public static bool CanSendMessengerWithCost(Hero targetHero, GoldCost diplomacyCost)
        {
            var canPayCost = diplomacyCost.CanPayCost();
            return canPayCost && IsTargetHeroAvailable(targetHero);
        }

        public static bool CanSendMessengerWithCost(Hero targetHero, GoldCost diplomacyCost, out TextObject exception)
        {
            var canPayCost = diplomacyCost.CanPayCost();
            if (!canPayCost)
            {
                exception = new("{=IWZ91JVk}Not enough gold!");
                return false;
            }

            return IsTargetHeroAvailable(targetHero, out exception);
        }

        private void CleanUpSettlementEncounter(float obj)
        {
            PlayerEncounter.Finish();

            if (_position2D.IsValid)
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
            SyncMessengerHourlySpeed(true);
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