using System.Collections.Generic;
using Diplomacy.CivilWar.Actions;
using Diplomacy.PatchTools;
using SandBox.GauntletUI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;

namespace Diplomacy.Patches
{
    internal sealed class MBBannerEditorGauntletScreenPatch : PatchClass<MBBannerEditorGauntletScreenPatch, MBBannerEditorGauntletScreen>
    {
        protected override IEnumerable<Patch> Prepare()
        {
            return new Patch[]
            {
                new Postfix(nameof(SetClanRelatedRulesPostfix),
                    typeof(MBBannerEditorGauntletScreen).GetConstructor(new[] {typeof(BannerEditorState)})!),
                new Prefix(nameof(SetColorsPrefix), nameof(MBBannerEditorGauntletScreen.OnDone))
            };
        }

        private static void SetClanRelatedRulesPostfix(BannerEditorState bannerEditorState, BannerEditorView ____bannerEditorLayer)
        {
            ____bannerEditorLayer.DataSource.SetClanRelatedRules(bannerEditorState.GetClan().Kingdom is not Kingdom kingdom ||
                                                                 kingdom.RulingClan == bannerEditorState.GetClan());
        }

        private static bool SetColorsPrefix(Clan ____clan, BannerEditorView ____bannerEditorLayer)
        {
            if (____clan.MapFaction is not Kingdom kingdom || kingdom.RulingClan != ____clan) return true;

            var primaryColor = ____bannerEditorLayer.DataSource.BannerVM.GetPrimaryColor();
            var sigilColor = ____bannerEditorLayer.DataSource.BannerVM.GetSigilColor();

            ChangeKingdomBannerAction.Apply(kingdom, primaryColor, sigilColor);
            Game.Current.GameStateManager.PopState();
            return false;
        }
    }
}