using JetBrains.Annotations;

using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace Diplomacy.Widgets
{
    [UsedImplicitly]
    public class CriticalThresholdTextWidget : TextWidget
    {
        public CriticalThresholdTextWidget(UIContext context) : base(context) { }

        private bool _isCritical;

        [Editor]
        public bool IsCritical
        {
            get => _isCritical;
            set
            {
                if (_isCritical != value)
                {
                    _isCritical = value;
                    if (_isCritical)
                    {
                        base.SetState("Critical");
                    }
                    else
                    {
                        base.SetState("Default");
                    }
                    OnPropertyChanged(value, nameof(IsCritical));
                }
            }
        }
    }
}