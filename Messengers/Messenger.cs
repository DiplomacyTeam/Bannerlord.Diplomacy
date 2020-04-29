using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.Messengers
{
    class Messenger
    {
        public Messenger(Hero targetHero, CampaignTime campaignTime)
        {
            TargetHero = targetHero;
            DispatchTime = campaignTime;
        }
        public CampaignTime DispatchTime { get; }
        public Hero TargetHero { get; }
    }
}
