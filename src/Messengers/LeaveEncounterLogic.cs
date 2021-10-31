using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;

namespace Diplomacy.Messengers
{
    public class LeaveEncounterLogic : MissionLogic
    {
#if e165
        public override void OnRemoveBehavior()
#else
        public override void OnRemoveBehaviour()
#endif
        {
            PlayerEncounter.Finish();
#if e159 || e1510
            CampaignEvents.RemoveListeners(this);
#else
            CampaignEventDispatcher.Instance.RemoveListeners(this);
#endif
        }
    }
}
