using DiplomacyFixes.CampaignEventBehaviors;
using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace DiplomacyFixes
{
    class MySubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            try
            {
                new Harmony("mod.diplomacyfixes").PatchAll();
                InformationManager.DisplayMessage(new InformationMessage($"Diplomacy Fixes {"Patch Succeeded!"}"));
                FileLog.Log("Harmony Patching Succeeded!");
            }
            catch (Exception ex)
            {
                FileLog.Log("Overall Patcher " + ex.Message);
                InformationManager.DisplayMessage(new InformationMessage($"Diplomacy Fixes {"Patch Failed"} exception: {ex.Message}"));
            }
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            InformationManager.DisplayMessage(new InformationMessage("Loaded Diplomacy Fixes!!!", Color.FromUint(4282569842U)));
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if(game.GameType is Campaign)
            {
                CampaignGameStarter gameStarter = (CampaignGameStarter) gameStarterObject;
                gameStarter.AddBehavior(new DeclareWarCooldown());
            }
            base.OnGameStart(game, gameStarterObject);
        }
    }
}
