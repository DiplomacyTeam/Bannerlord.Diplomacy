using StoryMode;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Usurp
{
    public class UsurpKingdomAction
    {
        public static void Apply(Clan usurpingClan)
        {
            if (Settings.Instance.EnableStorylineProtection && StoryMode.StoryMode.Current.MainStoryLine.MainStoryLineSide == MainStoryLineSide.None)
            {
                InformationManager.ShowInquiry(new InquiryData(
                    new TextObject("{=fQxiCdBA}Main Storyline").ToString(),
                    new TextObject("{=3wXqST66}By usurping this throne, you are committing to {STANCE} the empire in the main storyline.")
                    .SetTextVariable("STANCE", StoryModeData.IsKingdomImperial(usurpingClan.Kingdom) ? new TextObject("{=yAFwbD9B}unifying") : new TextObject("{=IGJVx5XI}destroying"))
                    .ToString(),
                    true,
                    true,
                    new TextObject(StringConstants.Accept).ToString(),
                    new TextObject(StringConstants.Decline).ToString(),
                    () => ApplyInternal(usurpingClan),
                    null,
                    ""), true);
            }
            else
            {
                ApplyInternal(usurpingClan);
            }
        }

        private static void ApplyInternal(Clan usurpingClan)
        {
            if (Settings.Instance.EnableStorylineProtection && StoryMode.StoryMode.Current.MainStoryLine.MainStoryLineSide == MainStoryLineSide.None)
            {
                if (StoryModeData.IsKingdomImperial(usurpingClan.Kingdom))
                {
                    StoryMode.StoryMode.Current.MainStoryLine.SetStoryLineSide(MainStoryLineSide.SupportImperialKingdom);
                }
                else
                {
                    StoryMode.StoryMode.Current.MainStoryLine.SetStoryLineSide(MainStoryLineSide.SupportAntiImperialKingdom);
                }
            }
            List<Clan> supportingClans, opposingClans;
            GetClanSupport(usurpingClan, out supportingClans, out opposingClans);

            usurpingClan.Influence -= usurpingClan.Kingdom.RulingClan.Influence;
            usurpingClan.Kingdom.RulingClan.Influence = 0;
            usurpingClan.Kingdom.RulingClan = usurpingClan;

            AdjustRelations(usurpingClan, supportingClans, 10);
            AdjustRelations(usurpingClan, opposingClans, 20);
        }

        private static void AdjustRelations(Clan usurpingClan, List<Clan> clans, int baseValue)
        {
            Hero leader = usurpingClan.Leader;
            foreach (Clan clan in clans)
            {
                if (usurpingClan == clan)
                {
                    continue;
                }

                Hero otherLeader = clan.Leader;
                int honor = otherLeader.GetHeroTraits().Honor;
                int calculating = otherLeader.GetHeroTraits().Calculating;
                int value = honor >= calculating ? baseValue : default;
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(leader, otherLeader, -value);
            }
        }

        public static bool CanUsurp(Clan usurpingClan, out string errorMessage)
        {
            errorMessage = null;
            if (Settings.Instance.EnableStorylineProtection && (!(StoryMode.StoryMode.Current?.MainStoryLine?.FirstPhase?.AllPiecesCollected ?? false)))
            {
                errorMessage = new TextObject("{=Euy6Mwcq}You must progress further in the main quest to unlock this action. You can disable storyline protection in the mod options.").ToString();
                return false;
            }

            if (!usurpingClan.MapFaction.IsKingdomFaction || usurpingClan.Kingdom.RulingClan == usurpingClan)
            {
                return false;
            }

            Clan rulingClan = usurpingClan.Kingdom.RulingClan;
            GetClanSupport(usurpingClan, out int supportingClanTiers, out int opposingClanTiers);
            return usurpingClan.Influence > GetUsurpInfluenceCost(usurpingClan) && supportingClanTiers > opposingClanTiers;
        }

        public static void GetClanSupport(Clan usurpingClan, out int supportingClanTiers, out int opposingClanTiers)
        {
            Clan rulingClan = usurpingClan.Kingdom.RulingClan;
            List<Clan> supportingClans, opposingClans;
            GetClanSupport(usurpingClan, out supportingClans, out opposingClans);

            supportingClanTiers = supportingClans.Select(clan => clan.Tier).Sum() + usurpingClan.Tier;
            opposingClanTiers = opposingClans.Select(clan => clan.Tier).Sum() + rulingClan.Tier;
        }

        public static float GetUsurpInfluenceCost(Clan usurpingClan)
        {
            Clan rulingClan = usurpingClan.Kingdom.RulingClan;
            return rulingClan.Influence;
        }

        private static void GetClanSupport(Clan usurpingClan, out List<Clan> supportingClans, out List<Clan> opposingClans)
        {
            Kingdom kingdom = usurpingClan.Kingdom;
            Clan rulingClan = usurpingClan.Kingdom.RulingClan;

            IEnumerable<Clan> validClans = kingdom.Clans.Except(new Clan[] { usurpingClan, rulingClan });

            supportingClans = validClans.Where(clan => usurpingClan.Leader.GetRelation(clan.Leader) > rulingClan.Leader.GetRelation(clan.Leader)).ToList();
            opposingClans = validClans.Except(supportingClans).ToList();
        }
    }
}
