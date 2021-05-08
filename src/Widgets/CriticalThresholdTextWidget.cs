using TaleWorlds.GauntletUI;

namespace Diplomacy.Widgets
{
    public class CriticalThresholdTextWidget : TextWidget
    {
        public CriticalThresholdTextWidget(UIContext context) : base(context) { }

        protected override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
            if (IsCritical)
            {
                base.SetState("Critical");
            }
        }

        private bool _isCritical;

        [Editor(false)]
        public bool IsCritical
        {
            get => _isCritical;
            set
            {
                if (this._isCritical != value)
                {
                    this._isCritical = value;
                    base.OnPropertyChanged(value, nameof(IsCritical));
                }
            }
        }
    }
}
