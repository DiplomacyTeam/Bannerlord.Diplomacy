using Diplomacy.CivilWar;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.EncyclopediaItems;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    public class RebelFactionParticipantVM : EncyclopediaFactionVM
    {
        Action _onComplete;

        private static readonly string _SPlus = new TextObject("{=eTw2aNV5}+").ToString();
        private static readonly string _SRequiredScore = new TextObject("{=XIBUWDlT}Required Score").ToString();
        private static readonly string _SCurrentScore = new TextObject("{=5r6fsHgm}Current Score").ToString();
        private static readonly string _SClanText = GameTexts.FindText("str_clan").ToString();

        public RebelFactionParticipantVM(Clan clan, RebelFaction rebelFaction, Action _onComplete) : base(clan)
        {
            var explainedNumber = RebelFactionScoringModel.GetDemandScore(clan, rebelFaction);
            this._onComplete = _onComplete;


            static string PlusPrefixed(float value) => $"{(value >= 0.005f ? _SPlus : string.Empty)}{value:0.##}";
            List<TooltipProperty> list = new();

            list.Add(new(_SClanText, clan.Name.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.Title));
            list.Add(new(new TextObject("{=TvSHYACm}Strength").ToString(), Convert.ToString((int)Math.Round(clan.TotalStrength)), 0, false, TooltipProperty.TooltipPropertyFlags.None));

            list.Add(new(_SCurrentScore, $"{explainedNumber.ResultNumber:0.##}", 0, false, TooltipProperty.TooltipPropertyFlags.Title));
            foreach (var (name, number) in explainedNumber.GetLines())
                list.Add(new(name, PlusPrefixed(number), 0, false, TooltipProperty.TooltipPropertyFlags.None));

            list.Add(new(string.Empty, string.Empty, 0, false, TooltipProperty.TooltipPropertyFlags.RundownSeperator));
            list.Add(new(_SRequiredScore, $"{RebelFactionScoringModel.RequiredScore:0.##}", 0, false, TooltipProperty.TooltipPropertyFlags.RundownResult));
            Hint = new BasicTooltipViewModel(() => list);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        protected new void ExecuteLink()
        {
            base.ExecuteLink();
            _onComplete();
        }

        [DataSourceProperty]
        public BasicTooltipViewModel Hint { get; set; }
    }
}
