using Diplomacy.CivilWar.Factions;
using Diplomacy.Extensions;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar.Actions
{
    public class StartRebellionAction
    {
        private static readonly TextObject _TRebelKingdomName = new("{=FajeZClv}{REBELS} ({KINGDOM} Rebels)");

        public static void Apply(RebelFaction rebelFaction)
        {
            foreach (var rf in RebelFactionManager.GetRebelFaction(rebelFaction.ParentKingdom).ToList())
            {
                if (rf == rebelFaction)
                    continue;

                RebelFactionManager.DestroyRebelFaction(rf);
            }

            if (rebelFaction.SponsorClan.IsEliminated)
            {
                rebelFaction.RemoveClan(rebelFaction.SponsorClan);
                if (RebelFactionManager.GetRebelFaction(rebelFaction.ParentKingdom).Any(x => x == rebelFaction))
                    return;
            }

            var rebelKingdomName = _TRebelKingdomName.CopyTextObject()
                .SetTextVariable("REBELS", rebelFaction.Name)
                .SetTextVariable("KINGDOM", rebelFaction.ParentKingdom.Name);

            Campaign.Current.KingdomManager.CreateKingdom(
                rebelKingdomName,
                rebelKingdomName,
                rebelFaction.ParentKingdom.Culture,
                rebelFaction.SponsorClan);

            var kingdom = Kingdom.All.FirstOrDefault(x => !x.IsEliminated && x.RulingClan == rebelFaction.SponsorClan);
            if (kingdom is null)
                return;

            rebelFaction.StartRebellion(kingdom);

            ChangeKingdomBannerAction.Apply(rebelFaction.RebelKingdom!, true);

            foreach (var clan in rebelFaction.Clans.ToList())
            {
                if (clan.IsEliminated)
                {
                    rebelFaction.RemoveClan(clan);
                    continue;
                }
                // make sure to retain influence
                var influence = clan.Influence;
                ChangeKingdomAction.ApplyByJoinToKingdom(clan, rebelFaction.RebelKingdom, false);
                clan.Influence = influence;
            }

#if v100 || v101 || v102 || v103
            DeclareWarAction.Apply(rebelFaction.RebelKingdom, rebelFaction.ParentKingdom);
#else
            DeclareWarAction.ApplyByKingdomCreation(rebelFaction.RebelKingdom, rebelFaction.ParentKingdom);
#endif

            var strVars = new Dictionary<string, object>
            {
                {"KINGDOM", rebelFaction.ParentKingdom.Name},
                {"REBEL_KINGDOM", rebelFaction.RebelKingdom!.Name},
                {"CLAN_NAME", rebelFaction.SponsorClan.Name},
                {"DEMAND", rebelFaction.DemandDescription}
            };

            InformationManager.ShowInquiry(
                new InquiryData(
                    new TextObject("{=nAvLFv5Q}Civil War Breaks Out").ToString(),
                    new TextObject("{=2PpZkBOB}A civil war has broken out in {KINGDOM}. The {REBEL_KINGDOM} are led by clan {CLAN_NAME}. {DEMAND}",
                        strVars).ToString(),
                    true,
                    false,
                    GameTexts.FindText("str_ok").ToString(),
                    null,
                    null,
                    null), true);
        }
    }
}