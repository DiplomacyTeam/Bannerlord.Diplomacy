using Diplomacy.CivilWar.Factions;
using Diplomacy.CivilWar.Scoring;
using Diplomacy.Helpers;

using JetBrains.Annotations;

using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Items;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    public class RebelFactionParticipantVM : EncyclopediaFactionVM
    {
        private static readonly TextObject _TRequiredScore = new("{=XIBUWDlT}Required Score");
        private static readonly TextObject _TCurrentScore = new("{=5r6fsHgm}Current Score");
        private static readonly TextObject _TClanText = GameTexts.FindText("str_clan");
        private readonly Action _onComplete;

        [DataSourceProperty]
        public BasicTooltipViewModel Hint { get; set; }

        public RebelFactionParticipantVM(Clan clan, RebelFaction rebelFaction, Action onComplete) : base(clan)
        {
            var explainedNumber = RebelFactionScoringModel.GetDemandScore(clan, rebelFaction);
            _onComplete = onComplete;

            List<TooltipProperty> list = new()
            {
                new TooltipProperty(_TClanText.ToString(), clan.Name.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.Title),
                new TooltipProperty(new TextObject("{=TvSHYACm}Strength").ToString(), Convert.ToString((int) Math.Round(clan.TotalStrength)), 0),
                new TooltipProperty(_TCurrentScore.ToString(), $"{explainedNumber.ResultNumber:0.##}", 0, false,
                    TooltipProperty.TooltipPropertyFlags.Title)
            };

            foreach (var (name, number) in explainedNumber.GetLines())
                list.Add(new TooltipProperty(name, StringHelper.GetPlusPrefixed(number), 0));

            list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, TooltipProperty.TooltipPropertyFlags.RundownSeperator));
            list.Add(new TooltipProperty(_TRequiredScore.ToString(), $"{RebelFactionScoringModel.RequiredScore:0.##}", 0, false,
                TooltipProperty.TooltipPropertyFlags.RundownResult));
            Hint = new BasicTooltipViewModel(() => list);
        }

        [UsedImplicitly]
        protected new void ExecuteLink()
        {
            base.ExecuteLink();
            _onComplete();
        }
    }
}