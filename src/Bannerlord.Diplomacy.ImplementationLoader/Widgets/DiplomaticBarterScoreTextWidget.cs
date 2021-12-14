using System;
using JetBrains.Annotations;
using TaleWorlds.GauntletUI;

namespace Widgets
{
    [UsedImplicitly]
    public class DiplomaticBarterScoreTextWidget : TextWidget
    {
        private float _score;

        public DiplomaticBarterScoreTextWidget(UIContext context) : base(context)
        {
        }

        protected override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
            if (Score > 0)
            {
                base.SetState("Positive");
            }
            else if (Score == 0)
            {
                base.SetState("Default");
            }
            else if (Score < 0)
            {
                base.SetState("Negative");
            }
        }

        [Editor()]
        public float Score
        {
            get => _score;
            set { _score = value; base.SetText($"{Math.Floor(_score * 10) / 10:0.0}"); }
        }
    }
}
