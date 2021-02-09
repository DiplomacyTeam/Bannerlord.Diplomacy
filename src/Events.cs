﻿using Diplomacy.DiplomaticAction.Alliance;
using Diplomacy.DiplomaticAction.WarPeace;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;

namespace Diplomacy
{
    internal sealed class Events
    {
        public static Events Instance { get; set; } = null!;

        private readonly MbEvent<Hero> _messengerSent = new MbEvent<Hero>();
        private readonly MbEvent<Kingdom> _peaceProposalSent = new MbEvent<Kingdom>();
        private readonly MbEvent<Town> _fiefGranted = new MbEvent<Town>();
        private readonly MbEvent<AllianceEvent> _allianceFormed = new MbEvent<AllianceEvent>();
        private readonly MbEvent<AllianceEvent> _allianceBroken = new MbEvent<AllianceEvent>();
        private readonly MbEvent<Settlement> _playerSettlementTaken = new MbEvent<Settlement>();
        private readonly MbEvent<WarDeclaredEvent> _warDeclared = new MbEvent<WarDeclaredEvent>();
        private readonly List<object> _listeners;

        public Events()
        {
            Instance = this;
            _listeners = new List<object>
            {
                _allianceBroken,
                _allianceFormed,
                _fiefGranted,
                _messengerSent,
                _peaceProposalSent,
                _playerSettlementTaken,
                _warDeclared
            };
        }

        public static IMbEvent<AllianceEvent> AllianceFormed => Instance._allianceFormed;

        public static IMbEvent<AllianceEvent> AllianceBroken => Instance._allianceBroken;

        public static IMbEvent<Hero> MessengerSent => Instance._messengerSent;

        public static IMbEvent<Kingdom> PeaceProposalSent => Instance._peaceProposalSent;

        public static IMbEvent<Town> FiefGranted => Instance._fiefGranted;

        public static IMbEvent<Settlement> PlayerSettlementTaken => Instance._playerSettlementTaken;

        public static IMbEvent<WarDeclaredEvent> WarDeclared => Instance._warDeclared;

        internal void OnMessengerSent(Hero hero) => Instance._messengerSent.Invoke(hero);

        internal void OnPeaceProposalSent(Kingdom kingdom) => Instance._peaceProposalSent.Invoke(kingdom);

        internal void OnFiefGranted(Town fief) => Instance._fiefGranted.Invoke(fief);

        internal void OnAllianceFormed(AllianceEvent allianceEvent) => Instance._allianceFormed.Invoke(allianceEvent);

        internal void OnAllianceBroken(AllianceEvent allianceEvent) => Instance._allianceBroken.Invoke(allianceEvent);

        internal void OnPlayerSettlementTaken(Settlement currentSettlement) => Instance._playerSettlementTaken.Invoke(currentSettlement);

        internal void OnWarDeclared(WarDeclaredEvent warDeclaredEvent) => Instance._warDeclared.Invoke(warDeclaredEvent);

        public static void RemoveListeners(object o) => Instance.RemoveListenersInternal(o);

        private void RemoveListenersInternal(object obj)
        {
            foreach (dynamic listener in _listeners)
                listener.ClearListeners(obj);
        }
    }
}
