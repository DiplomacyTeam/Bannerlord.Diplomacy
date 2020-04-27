using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace DiplomacyFixes.Messengers
{
    [SaveableClass(1)]
    class Messenger
    {
        public Messenger(Hero targetHero, CampaignTime dispatchTime)
        {
            TargetHero = targetHero;
            DispatchTime = dispatchTime;
        }

        [SaveableProperty(1)]
        public CampaignTime DispatchTime { get; private set; }

        [SaveableProperty(2)]
        public Hero TargetHero { get; private set; }
    }
}
