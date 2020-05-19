using DiplomacyFixes.Alliance;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes
{
    class Events
    {
        public static Events Instance { get; set; }

        private readonly MbEvent<Hero> _messengerSent = new MbEvent<Hero>();
        private readonly MbEvent<Kingdom> _peaceProposalSent = new MbEvent<Kingdom>();
        private readonly MbEvent<Town> _fiefGranted = new MbEvent<Town>();
        private readonly MbEvent<AllianceFormedEvent> _allianceFormed = new MbEvent<AllianceFormedEvent>();

        public static IMbEvent<AllianceFormedEvent> AllianceFormed
        {
            get
            {
                return Instance._allianceFormed;
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

        internal void OnAllianceFormed(AllianceFormedEvent allianceFormedEvent)
        {
            Instance._allianceFormed.Invoke(allianceFormedEvent);
        }

    }
}
