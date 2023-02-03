using Diplomacy.DiplomaticAction;
using Diplomacy.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    public sealed class DiplomacyPropertiesVM : TaleWorlds.Library.ViewModel
    {
        private static readonly TextObject _TDaysRemaining = new(StringConstants.DaysRemaining);

        private IFaction Faction1 { get; }

        private IFaction Faction2 { get; }

        private MBBindingList<DiplomacyFactionRelationshipVM> _faction1Wars;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction1Allies;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction2Wars;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction2Allies;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction1Pacts;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction2Pacts;

        public DiplomacyPropertiesVM(IFaction faction1, IFaction faction2)
        {
            Faction1 = faction1;
            Faction2 = faction2;

            _faction1Wars = new();
            _faction1Allies = new();

            _faction2Wars = new();
            _faction2Allies = new();

            _faction1Pacts = new();
            _faction2Pacts = new();
        }

        public void UpdateDiplomacyProperties()
        {
            Faction1Wars.Clear();
            Faction1Allies.Clear();

            Faction2Wars.Clear();
            Faction2Allies.Clear();

            Faction1Pacts.Clear();
            Faction2Pacts.Clear();

            AddWarRelationships(Faction1.Stances);
            AddWarRelationships(Faction2.Stances);

            foreach (var kingdom in KingdomExtensions.AllActiveKingdoms)
            {
                if (FactionManager.IsAlliedWithFaction(kingdom, Faction1) && kingdom != Faction1)
                    Faction1Allies.Add(new DiplomacyFactionRelationshipVM(kingdom, CreateAllianceHint(kingdom, (Faction1 as Kingdom)!)));

                if (FactionManager.IsAlliedWithFaction(kingdom, Faction2) && kingdom != Faction2)
                    Faction2Allies.Add(new DiplomacyFactionRelationshipVM(kingdom, CreateAllianceHint(kingdom, (Faction2 as Kingdom)!)));

                AddNonAggressionPactRelationships(kingdom, Faction1, Faction1Pacts);
                AddNonAggressionPactRelationships(kingdom, Faction2, Faction2Pacts);
            }
        }
        private HintViewModel CreateAllianceHint(Kingdom kingdom1, Kingdom kingdom2)
        {
            TextObject textObject;
            if (CooldownManager.HasBreakAllianceCooldown(kingdom1, kingdom2, out var elapsedDaysUntilNow))
            {
                textObject = _TDaysRemaining.CopyTextObject();
                var remaining = Settings.Instance!.MinimumAllianceDuration - elapsedDaysUntilNow;
                textObject.SetTextVariable("DAYS_LEFT", (int) Math.Round(remaining));
            }
            else
            {
                textObject = TextObject.Empty;
            }

            return new HintViewModel(textObject);
        }

        private void AddNonAggressionPactRelationships(Kingdom kingdom, IFaction faction, MBBindingList<DiplomacyFactionRelationshipVM> factionPacts)
        {
            if (DiplomaticAgreementManager.HasNonAggressionPact(kingdom, (Kingdom) faction, out var pact))
            {
                var textObject = _TDaysRemaining.CopyTextObject();
                textObject.SetTextVariable("DAYS_LEFT", (int) Math.Round(pact!.EndDate.RemainingDaysFromNow));
                factionPacts.Add(new DiplomacyFactionRelationshipVM(kingdom, Compat.HintViewModel.Create(textObject)));
            }
        }

        private void AddWarRelationships(IEnumerable<StanceLink> stances)
        {
            foreach (var stance in from x in stances
                                   where x.IsAtWar
                                   select x into w
                                   orderby w.Faction1.Name.ToString() + w.Faction2.Name.ToString()
                                   select w)
            {
                var (f1, f2) = (stance.Faction1, stance.Faction2);

                if (f1 is Kingdom && f2 is Kingdom && !f1.IsMinorFaction && !f2.IsMinorFaction && !f1.IsBanditFaction && !f2.IsBanditFaction)
                {
                    var isFaction1War = f1 == Faction1 || f2 == Faction1;
                    var isFaction2War = f1 == Faction2 || f2 == Faction2;

                    if (isFaction1War && isFaction2War && Faction1Wars.Any(war => war.Faction == Faction2))
                        continue; // Make sure we only add the shared war once

                    if (isFaction1War)
                        Faction1Wars.Add(new DiplomacyFactionRelationshipVM(f1 == Faction1 ? f2 : f1));

                    if (isFaction2War)
                        Faction2Wars.Add(new DiplomacyFactionRelationshipVM(f1 == Faction2 ? f2 : f1));
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<DiplomacyFactionRelationshipVM> Faction1Wars { get => _faction1Wars; set => SetField(ref _faction1Wars, value, nameof(Faction1Wars)); }

        [DataSourceProperty]
        public MBBindingList<DiplomacyFactionRelationshipVM> Faction1Allies { get => _faction1Allies; set => SetField(ref _faction1Allies, value, nameof(Faction1Allies)); }

        [DataSourceProperty]
        public MBBindingList<DiplomacyFactionRelationshipVM> Faction2Wars { get => _faction2Wars; set => SetField(ref _faction2Wars, value, nameof(Faction2Wars)); }

        [DataSourceProperty]
        public MBBindingList<DiplomacyFactionRelationshipVM> Faction2Allies { get => _faction2Allies; set => SetField(ref _faction2Allies, value, nameof(Faction2Allies)); }

        [DataSourceProperty]
        public MBBindingList<DiplomacyFactionRelationshipVM> Faction1Pacts { get => _faction1Pacts; set => SetField(ref _faction1Pacts, value, nameof(Faction1Pacts)); }

        [DataSourceProperty]
        public MBBindingList<DiplomacyFactionRelationshipVM> Faction2Pacts { get => _faction2Pacts; set => SetField(ref _faction2Pacts, value, nameof(Faction2Pacts)); }
    }
}