using DiplomacyFixes.CampaignEventBehaviors;
using HarmonyLib;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
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
            new Harmony("mod.diplomacyfixes").PatchAll();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            InformationManager.DisplayMessage(new InformationMessage("Loaded Diplomacy Fixes!!!", Color.FromUint(4282569842U)));
        }
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (game.GameType is Campaign)
            {
                CampaignGameStarter gameStarter = (CampaignGameStarter)gameStarterObject;
                gameStarter.AddBehavior(new DeclareWarCooldown());
                gameStarter.AddBehavior(new MessengerArrived());
            }
            base.OnGameStart(game, gameStarterObject);
        }
    }
}
