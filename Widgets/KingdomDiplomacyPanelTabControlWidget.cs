using TaleWorlds.GauntletUI;

namespace DiplomacyFixes.Widgets
{
    class KingdomDiplomacyPanelTabControlWidget : ListPanel
    {
        private Widget _statsPanel;
        private Widget _overviewPanel;
        private ButtonWidget _overviewButton;
        private ButtonWidget _statsButton;

        public KingdomDiplomacyPanelTabControlWidget(UIContext context) : base(context)
        {
        }

        protected override void OnLateUpdate(float dt)
        {
            base.OnLateUpdate(dt);
            this.OverviewButton.IsSelected = this.OverviewPanel.IsVisible;
            this.StatsButton.IsSelected = this.StatsPanel.IsVisible;
        }

        [Editor(false)]
        public ButtonWidget OverviewButton
        {
            get
            {
                return this._overviewButton;
            }
            set
            {
                if (this._overviewButton != value)
                {
                    this._overviewButton = value;
                    base.OnPropertyChanged(value, "OverviewButton");
                }
            }
        }

        [Editor(false)]
        public ButtonWidget StatsButton
        {
            get
            {
                return this._statsButton;
            }
            set
            {
                if (this._statsButton != value)
                {
                    this._statsButton = value;
                    base.OnPropertyChanged(value, "StatsButton");
                }
            }
        }

        [Editor(false)]
        public Widget OverviewPanel
        {
            get
            {
                return this._overviewPanel;
            }
            set
            {
                if (this._overviewPanel != value)
                {
                    this._overviewPanel = value;
                    base.OnPropertyChanged(value, "OverviewPanel");
                }
            }
        }

        [Editor(false)]
        public Widget StatsPanel
        {
            get
            {
                return this._statsPanel;
            }
            set
            {
                if (this._statsPanel != value)
                {
                    this._statsPanel = value;
                    base.OnPropertyChanged(value, "StatsPanel");
                }
            }
        }
    }
}
