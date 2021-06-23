using TaleWorlds.GauntletUI;

namespace Diplomacy.Widgets
{
    public class HoveredBrushWidget : BrushWidget
    {
		public bool OverrideDefaultStateSwitchingEnabled { get; set; }
		public HoveredBrushWidget(UIContext context) : base(context)
		{
			base.AddState("Hovered");
			base.AddState("Disabled");
		}

		protected override void RefreshState()
		{
				if (base.IsDisabled)
				{
					this.SetState("Disabled");
				}
				else if (base.IsHovered)
				{
					this.SetState("Hovered");
				}
				else
				{
					this.SetState("Default");
				}
			base.RefreshState();
		}
	}
}
