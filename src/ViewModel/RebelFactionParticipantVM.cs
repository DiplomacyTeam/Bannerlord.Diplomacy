using Diplomacy.CivilWar;
using System;
using System.Collections.Generic;
using Diplomacy.CivilWar.Scoring;
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
        private readonly Action _onComplete;

        private static readonly TextObject _TPlus = new("{=eTw2aNV5}+");
        private static readonly TextObject _TRequiredScore = new("{=XIBUWDlT}Required Score");
        private static readonly TextObject _TCurrentScore = new("{=5r6fsHgm}Current Score");
        private static readonly TextObject _TClanText = GameTexts.FindText("str_clan");

        public RebelFactionParticipantVM(Clan clan, RebelFaction rebelFaction, Action _onComplete) : base(clan)
        {
            var explainedNumber = RebelFactionScoringModel.GetDemandScore(clan, rebelFaction);
            this._onComplete = _onComplete;


            static string PlusPrefixed(float value)
            {
                return $"{(value >= 0.005f ? _TPlus.ToString() : string.Empty)}{value:0.##}";
            }

            List<TooltipProperty> list = new()
            {
                new TooltipProperty(_TClanText.ToString(), clan.Name.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.Title),
                new TooltipProperty(new TextObject("{=TvSHYACm}Strength").ToString(), Convert.ToString((int) Math.Round(clan.TotalStrength)), 0,
                    false,
                    TooltipProperty.TooltipPropertyFlags.None),
                new TooltipProperty(_TCurrentScore.ToString(), $"{explainedNumber.ResultNumber:0.##}", 0, false,
                    TooltipProperty.TooltipPropertyFlags.Title)
            };


            foreach (var (name, number) in explainedNumber.GetLines())
                list.Add(new TooltipProperty(name, PlusPrefixed(number), 0, false, TooltipProperty.TooltipPropertyFlags.None));

            list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, TooltipProperty.TooltipPropertyFlags.RundownSeperator));
            list.Add(new TooltipProperty(_TRequiredScore.ToString(), $"{RebelFactionScoringModel.RequiredScore:0.##}", 0, false,
                TooltipProperty.TooltipPropertyFlags.RundownResult));
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

        [DataSourceProperty] public BasicTooltipViewModel Hint { get; set; }
    }
}