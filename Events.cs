using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
				if(_instance == null)
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

		// Token: 0x0600057B RID: 1403 RVA: 0x0001F898 File Offset: 0x0001DA98
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
