using DiplomacyFixes.Alliance;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes
{
    class Events
    {
        public static Events Instance { get; set; }

        private readonly MbEvent<Hero> _messengerSent = new MbEvent<Hero>();
        private readonly MbEvent<Kingdom> _peaceProposalSent = new MbEvent<Kingdom>();
        private readonly MbEvent<Town> _fiefGranted = new MbEvent<Town>();
        private readonly MbEvent<AllianceEvent> _allianceFormed = new MbEvent<AllianceEvent>();
        private readonly MbEvent<AllianceEvent> _allianceBroken = new MbEvent<AllianceEvent>();
        private readonly MbEvent<Settlement> _playerSettlementTaken = new MbEvent<Settlement>();
        private List<object> _listeners;

        public Events()
        {
            _listeners = new List<object>
            {
                _allianceBroken,
                _allianceFormed,
                _fiefGranted,
                _messengerSent,
                _peaceProposalSent,
                _playerSettlementTaken
            };
        }

        public static IMbEvent<AllianceEvent> AllianceFormed
        {
            get
            {
                return Instance._allianceFormed;
            }
        }

        public static IMbEvent<AllianceEvent> AllianceBroken
        {
            get
            {
                return Instance._allianceBroken;
            }
        }

        public static IMbEvent<Hero> MessengerSent
        {
            get
            {
                return Instance._messengerSent;
            }
        }

        public static IMbEvent<Kingdom> PeaceProposalSent
        {
            get
            {
                return Instance._peaceProposalSent;
            }
        }

        public static IMbEvent<Town> FiefGranted
        {
            get
            {
                return Instance._fiefGranted;
            }
        }

        public static IMbEvent<Settlement> PlayerSettlementTaken
        {
            get
            {
                return Instance._playerSettlementTaken;
            }
        }

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

        public static void RemoveListeners(object o)
        {
            Events.Instance.RemoveListenersInternal(o);
        }

        internal void RemoveListenersInternal(object obj)
        {
            foreach (dynamic listener in _listeners)
            {
                listener.ClearListeners(obj);
            }
        }
    }
}
