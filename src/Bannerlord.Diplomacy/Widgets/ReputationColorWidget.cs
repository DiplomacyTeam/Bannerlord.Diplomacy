using System;
using TaleWorlds.GauntletUI;

namespace Widgets
{
    public class ReputationColorWidget : BrushWidget
    {
        private string _relationState = null!;

        [Editor()]
        public string State
        {
            get => _relationState;
            set { _relationState = value; RefreshState();}
        }

        public ReputationColorWidget(UIContext context) : base(context)
        {
            foreach (var name in Enum.GetNames(typeof(RelationState)))
            {
                AddState(name);
            }
        }

        protected override void RefreshState()
        {
            base.SetState(State);
            base.RefreshState();
        }

        public enum RelationState
        {
            VeryHigh,
            High,
            Default,
            Low,
            VeryLow
        }
    }
}
