using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace Diplomacy.Messengers
{
    class Messenger
    {
        public Messenger(Hero targetHero, CampaignTime dispatchTime)
        {
            TargetHero = targetHero;
            DispatchTime = dispatchTime;
            CurrentPosition = Hero.MainHero.GetMapPoint().Position2D;
            Arrived = false;
        }

        [SaveableProperty(1)]
        public CampaignTime DispatchTime { get; private set; }

        [SaveableProperty(2)]
        public Hero TargetHero { get; private set; }

        [SaveableProperty(3)]
        public Vec2 CurrentPosition { get; set; }

        [SaveableProperty(4)]
        public bool Arrived { get; set; }
    }
}
