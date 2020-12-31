using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace Diplomacy.Messengers
{
    [SaveableClass(1)]
    class Messenger
    {
        public Messenger(Hero targetHero, CampaignTime dispatchTime)
        {
            this.TargetHero = targetHero;
            this.DispatchTime = dispatchTime;
            this.CurrentPosition = Hero.MainHero.GetMapPoint().Position2D;
            this.Arrived = false;
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
