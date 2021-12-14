using JetBrains.Annotations;
using TaleWorlds.GauntletUI;

namespace Diplomacy.Widgets
{
    [UsedImplicitly]
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

        [Editor()]
        public bool IsCritical
        {
            get => _isCritical;
            set
            {
                if (_isCritical != value)
                {
                    _isCritical = value;
                    OnPropertyChanged(value, nameof(IsCritical));
                }
            }
        }
    }
}
