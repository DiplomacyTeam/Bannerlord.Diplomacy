using JetBrains.Annotations;

using TaleWorlds.GauntletUI;

namespace Diplomacy.Widgets
{
    [UsedImplicitly]
    public class KingdomDiplomacyPanelTabControlWidget : ListPanel
    {
        private Widget? _statsPanel;
        private Widget? _overviewPanel;
        private ButtonWidget? _overviewButton;
        private ButtonWidget? _statsButton;

        public KingdomDiplomacyPanelTabControlWidget(UIContext context) : base(context)
        {
        }

        protected override void OnLateUpdate(float dt)
        {
            base.OnLateUpdate(dt);
            OverviewButton.IsSelected = OverviewPanel.IsVisible;
            StatsButton.IsSelected = StatsPanel.IsVisible;
        }

        [Editor()]
        public ButtonWidget OverviewButton
        {
            get => _overviewButton!;
            set
            {
                if (_overviewButton != value)
                {
                    _overviewButton = value;
                    OnPropertyChanged(value, nameof(OverviewButton));
                }
            }
        }

        [Editor()]
        public ButtonWidget StatsButton
        {
            get => _statsButton!;
            set
            {
                if (_statsButton != value)
                {
                    _statsButton = value;
                    OnPropertyChanged(value, nameof(StatsButton));
                }
            }
        }

        [Editor()]
        public Widget OverviewPanel
        {
            get => _overviewPanel!;
            set
            {
                if (_overviewPanel != value)
                {
                    _overviewPanel = value;
                    OnPropertyChanged(value, nameof(OverviewPanel));
                }
            }
        }

        [Editor()]
        public Widget StatsPanel
        {
            get => _statsPanel!;
            set
            {
                if (_statsPanel != value)
                {
                    _statsPanel = value;
                    OnPropertyChanged(value, nameof(StatsPanel));
                }
            }
        }
    }
}