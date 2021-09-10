using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;

namespace Diplomacy.Messengers
{
    public class LeaveEncounterLogic : MissionLogic
    {
        public override void OnRemoveBehaviour()
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
