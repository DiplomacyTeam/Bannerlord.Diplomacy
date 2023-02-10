using Diplomacy.DiplomaticAction.Alliance;
using Diplomacy.DiplomaticAction.WarPeace;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace Diplomacy.Events
{
    public sealed class DiplomacyEvents
    {
        private readonly MbEvent<AllianceEvent> _allianceBroken = new();
        private readonly MbEvent<AllianceEvent> _allianceFormed = new();
        private readonly MbEvent<Town> _fiefGranted = new();
        private readonly MbEvent<Kingdom> _kingdomBannerChanged = new();
        private readonly List<IMbEventBase> _listeners;

        private readonly MbEvent<Hero> _messengerSent = new();
        private readonly MbEvent<Kingdom> _peaceProposalSent = new();
        private readonly MbEvent<Settlement> _playerSettlementTaken = new();
#if v100 || v101 || v102 || v103
        private readonly MbEvent<WarDeclaredEvent> _warDeclared = new();
#endif
        private readonly MbEvent<WarExhaustionInitializedEvent> _warExhaustionInitialized = new();
        private readonly MbEvent<WarExhaustionAddedEvent> _warExhaustionAdded = new();

        public DiplomacyEvents()
        {
            Instance = this;
            _listeners = new List<IMbEventBase>
            {
                _allianceBroken,
                _allianceFormed,
                _fiefGranted,
                _messengerSent,
                _peaceProposalSent,
                _playerSettlementTaken,
#if v100 || v101 || v102 || v103
                _warDeclared,
#endif
                _kingdomBannerChanged,
                _warExhaustionInitialized,
                _warExhaustionAdded
            };
        }

        public static DiplomacyEvents Instance { get; set; } = null!;

        public static IMbEvent<AllianceEvent> AllianceFormed => Instance._allianceFormed;

        public static IMbEvent<AllianceEvent> AllianceBroken => Instance._allianceBroken;

        public static IMbEvent<Hero> MessengerSent => Instance._messengerSent;

        public static IMbEvent<Kingdom> PeaceProposalSent => Instance._peaceProposalSent;

        public static IMbEvent<Town> FiefGranted => Instance._fiefGranted;

        public static IMbEvent<Settlement> PlayerSettlementTaken => Instance._playerSettlementTaken;

#if v100 || v101 || v102 || v103
        public static IMbEvent<WarDeclaredEvent> WarDeclared => Instance._warDeclared;
#endif

        public static IMbEvent<Kingdom> KingdomBannerChanged => Instance._kingdomBannerChanged;

        public static IMbEvent<WarExhaustionInitializedEvent> WarExhaustionInitialized => Instance._warExhaustionInitialized;

        public static IMbEvent<WarExhaustionAddedEvent> WarExhaustionAdded => Instance._warExhaustionAdded;

        internal void OnMessengerSent(Hero hero)
        {
            Instance._messengerSent.Invoke(hero);
        }

        internal void OnPeaceProposalSent(Kingdom kingdom)
        {
            Instance._peaceProposalSent.Invoke(kingdom);
        }

        internal void OnFiefGranted(Town fief)
        {
            Instance._fiefGranted.Invoke(fief);
        }

        internal void OnAllianceFormed(AllianceEvent allianceEvent)
        {
            Instance._allianceFormed.Invoke(allianceEvent);
        }

        internal void OnAllianceBroken(AllianceEvent allianceEvent)
        {
            Instance._allianceBroken.Invoke(allianceEvent);
        }

        internal void OnPlayerSettlementTaken(Settlement currentSettlement)
        {
            Instance._playerSettlementTaken.Invoke(currentSettlement);
        }

#if v100 || v101 || v102 || v103
        internal void OnWarDeclared(WarDeclaredEvent warDeclaredEvent)
        {
            Instance._warDeclared.Invoke(warDeclaredEvent);
        }
#endif

        internal void OnKingdomBannerChanged(Kingdom kingdom)
        {
            Instance._kingdomBannerChanged.Invoke(kingdom);
        }

        internal void OnWarExhaustionInitialized(WarExhaustionInitializedEvent warExhaustionEvent)
        {
            Instance._warExhaustionInitialized.Invoke(warExhaustionEvent);
        }

        internal void OnWarExhaustionAdded(WarExhaustionAddedEvent warExhaustionEvent)
        {
            Instance._warExhaustionAdded.Invoke(warExhaustionEvent);
        }

        public static void RemoveListeners(object o)
        {
            Instance.RemoveListenersInternal(o);
        }

        private void RemoveListenersInternal(object obj)
        {
            foreach (var listener in _listeners)
                listener.ClearListeners(obj);
        }
    }
}