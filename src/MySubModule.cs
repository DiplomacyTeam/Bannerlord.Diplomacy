﻿using Diplomacy.CampaignEventBehaviors;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Diplomacy
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
                Events.Instance = new Events();
                CampaignGameStarter gameStarter = (CampaignGameStarter)gameStarterObject;
                gameStarter.AddBehavior(new DiplomaticAgreementBehavior());
                gameStarter.AddBehavior(new CooldownBehavior());
                gameStarter.AddBehavior(new MessengerBehavior());
                if (Settings.Instance.EnableWarExhaustion)
                {
                    gameStarter.AddBehavior(new WarExhaustionBehavior());
                }
                if (Settings.Instance.EnableFiefFirstRight)
                {
                    gameStarter.AddBehavior(new KeepFiefAfterSiegeBehavior());
                }
                gameStarter.AddBehavior(new AllianceBehavior());
                gameStarter.AddBehavior(new MaintainInfluenceBehavior());
                gameStarter.AddBehavior(new ExpansionismBehavior());
            }
            base.OnGameStart(game, gameStarterObject);
        }
    }
}
