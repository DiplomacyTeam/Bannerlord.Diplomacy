using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes
{
    class Events
    {
        private static Events _instance;
        public static Events Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Events();
                }
                return _instance;
            }
        }

        private readonly MbEvent<Hero> _messengerSent = new MbEvent<Hero>();
        private readonly MbEvent<Kingdom> _peaceProposalSent = new MbEvent<Kingdom>();

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

        internal void OnMessengerSent(Hero hero)
        {
            Instance._messengerSent.Invoke(hero);
        }

        internal void OnPeaceProposalSent(Kingdom kingdom)
        {
            Instance._peaceProposalSent.Invoke(kingdom);
        }

    }
}
