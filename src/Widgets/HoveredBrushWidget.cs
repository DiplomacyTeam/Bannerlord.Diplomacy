using TaleWorlds.GauntletUI;

namespace Diplomacy.Widgets
{
    public class HoveredBrushWidget : BrushWidget
    {
        public HoveredBrushWidget(UIContext context) : base(context)
        {
            AddState("Hovered");
            AddState("Disabled");
        }

        public bool OverrideDefaultStateSwitchingEnabled { get; set; }

        protected override void RefreshState()
        {
            if (IsDisabled)
                SetState("Disabled");
            else if (IsHovered)
                SetState("Hovered");
            else
                SetState("Default");
            base.RefreshState();
        }
    }
}