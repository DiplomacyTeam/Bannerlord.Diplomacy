using System.Collections.Generic;
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

            rebelFaction.AtWar = true;
            Campaign.Current.KingdomManager.CreateKingdom(
                rebelKingdomName,
                rebelKingdomName,
                rebelFaction.ParentKingdom.Culture,
                rebelFaction.SponsorClan);


            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (kingdom.RulingClan == rebelFaction.SponsorClan)
                {
                    rebelFaction.RebelKingdom = kingdom;
                }
            }

            ChangeKingdomBannerAction.Apply(rebelFaction.RebelKingdom!, true);

            foreach (Clan clan in rebelFaction.Clans)
            {
                ChangeKingdomAction.ApplyByJoinToKingdom(clan, rebelFaction.RebelKingdom, false);
            }

            DeclareWarAction.Apply(rebelFaction.RebelKingdom, rebelFaction.ParentKingdom);

            var strVars = new Dictionary<string, object> {
                { "KINGDOM", rebelFaction.ParentKingdom.Name },
                { "REBEL_KINGDOM", rebelFaction.RebelKingdom!.Name },
                { "CLAN_NAME", rebelFaction.SponsorClan.Name },
                { "DEMAND", rebelFaction.DemandDescription }
            };

            // FIXME add string localization
            InformationManager.ShowInquiry(
                    new InquiryData(
                        new TextObject("{=nAvLFv5Q}Civil War Breaks Out").ToString(),
                        new TextObject("{=2PpZkBOB}A civil war has broken out in {KINGDOM}. The {REBEL_KINGDOM} are led by clan {CLAN_NAME}. {DEMAND}", strVars).ToString(),
                        true,
                        false,
                        GameTexts.FindText("str_ok", null).ToString(),
                        null,
                        null,
                        null), true);
        }
    }
}
