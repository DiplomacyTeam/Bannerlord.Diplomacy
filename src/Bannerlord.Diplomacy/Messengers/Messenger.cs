using JetBrains.Annotations;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace Diplomacy.Messengers
{
    internal class Messenger
    {
        [SaveableProperty(1)][UsedImplicitly] public CampaignTime DispatchTime { get; private set; }

        [SaveableProperty(2)][UsedImplicitly] public Hero TargetHero { get; private set; }

        [SaveableProperty(3)] public Vec2 CurrentPosition { get; set; }

        [SaveableProperty(4)] public bool Arrived { get; set; }

        public Messenger(Hero targetHero, CampaignTime dispatchTime)
        {
            TargetHero = targetHero;
            DispatchTime = dispatchTime;
            CurrentPosition = Hero.MainHero.GetMapPoint().Position2D;
            Arrived = false;
        }
    }
}