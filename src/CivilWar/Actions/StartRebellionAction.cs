﻿using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar
{
    public class StartRebellionAction
    {
        private const string _SRebelKingdomName = "{=FajeZClv}{REBELS} ({KINGDOM} Rebels)";

        public static void Apply(RebelFaction rebelFaction)
        {
            foreach (RebelFaction rf in RebelFactionManager.GetRebelFaction(rebelFaction.ParentKingdom))
            {
                if (rf == rebelFaction)
                    continue;

                RebelFactionManager.DestroyRebelFaction(rf);
            }

            var rebelKingdomName = new TextObject(_SRebelKingdomName, new Dictionary<string, object>() { { "REBELS", rebelFaction.Name}, { "KINGDOM", rebelFaction.ParentKingdom.Name } });

            Campaign.Current.KingdomManager.CreateKingdom(
                rebelKingdomName,
                rebelKingdomName,
                rebelFaction.ParentKingdom.Culture,
                rebelFaction.SponsorClan);

            var kingdom = Kingdom.All.First(x => !x.IsEliminated && x.RulingClan == rebelFaction.SponsorClan);
            rebelFaction.StartRebellion(kingdom);

            ChangeKingdomBannerAction.Apply(rebelFaction.RebelKingdom!, true);

            foreach (Clan clan in rebelFaction.Clans)
            {
                // make sure to retain influence
                float influence = clan.Influence;
                ChangeKingdomAction.ApplyByJoinToKingdom(clan, rebelFaction.RebelKingdom, false);
                clan.Influence = influence;
            }

            DeclareWarAction.Apply(rebelFaction.RebelKingdom, rebelFaction.ParentKingdom);

            var strVars = new Dictionary<string, object> {
                { "KINGDOM", rebelFaction.ParentKingdom.Name },
                { "REBEL_KINGDOM", rebelFaction.RebelKingdom!.Name },
                { "CLAN_NAME", rebelFaction.SponsorClan.Name },
                { "DEMAND", rebelFaction.DemandDescription }
            };

            InformationManager.ShowInquiry(
                    new InquiryData(
                        new TextObject("{=nAvLFv5Q}Civil War Breaks Out").ToString(),
                        new TextObject("{=2PpZkBOB}A civil war has broken out in {KINGDOM}. The {REBEL_KINGDOM} are led by clan {CLAN_NAME}. {DEMAND}", strVars).ToString(),
                        true,
                        false,
                        GameTexts.FindText("str_ok").ToString(),
                        null,
                        null,
                        null), true);
        }
    }
}
