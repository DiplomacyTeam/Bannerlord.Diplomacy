using Diplomacy;
using Diplomacy.CivilWar;
using Diplomacy.CivilWar.Actions;
using Diplomacy.CivilWar.Factions;

using TaleWorlds.CampaignSystem;

using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Recruitment.Tests
{
    public sealed class RecruitmentTests
    {
        private readonly Kingdom _kingdom = new();
        private readonly Clan _player;
        private readonly Clan _target;
        private readonly RebelFaction _faction;
        private readonly FactionRecruitmentManager _manager = new();

        public RecruitmentTests()
        {
            CampaignTime.NowDays = 100;
            Settings.Instance = new();
            _player = new() { Kingdom = _kingdom, Influence = 100 };
            _target = new() { Kingdom = _kingdom, Support = 75 };
            Clan.PlayerClan = _player;
            Hero.MainHero = _player.Leader;
            _kingdom.RulingClan = new() { Kingdom = _kingdom };
            _faction = NewFaction(RebelDemandType.Abdication);
        }

        private RebelFaction NewFaction(RebelDemandType demand)
        {
            var faction = new RebelFaction { SponsorClan = _player, ParentKingdom = _kingdom, RebelDemandType = demand };
            faction.AddClan(_player);
            _kingdom.Factions.Add(faction);
            return faction;
        }

        private FactionRecruitmentRecord Begin()
        {
            var record = RecruitFactionSupportAction.TryBeginPersuasion(_target, _faction, out var reason);
            Assert.True(record is not null, reason.ToString());
            return record!;
        }

        private void Recruit()
            => Assert.True(RecruitFactionSupportAction.TryCompletePersuasion(Begin(), true, out _));

        [Theory]
        [InlineData(RebelDemandType.Abdication)]
        [InlineData(RebelDemandType.Secession)]
        public void ExistingSupporterStaysWithoutPledgeWhenItsScoreMeetsTheThreshold(RebelDemandType demand)
        {
            _faction.RebelDemandType = demand;
            _target.Support = 100;
            JoinFactionAction.Apply(_target, _faction);
            Assert.False(JoinFactionAction.CanApply(_target, _faction, out _));
            Assert.True(JoinFactionAction.ShouldRemain(_target, _faction));
            _target.Support = 99;
            Assert.False(JoinFactionAction.ShouldRemain(_target, _faction));
        }

        [Theory]
        [InlineData(RebelDemandType.Abdication)]
        [InlineData(RebelDemandType.Secession)]
        public void SuccessfulPersuasionKeepsAWeakSupporterUntilPledgeExpiry(RebelDemandType demand)
        {
            _faction.RebelDemandType = demand;
            Recruit();
            _target.Support = 0;
            Assert.Equal(75, _player.Influence);
            Assert.Equal(30, _manager.GetPledgeDaysRemaining(_target, _faction));
            CampaignTime.NowDays += 29.5f;
            Assert.True(JoinFactionAction.ShouldRemain(_target, _faction));
            Assert.Equal(1, _manager.GetPledgeDaysRemaining(_target, _faction));
            CampaignTime.NowDays += .5f;
            Assert.False(JoinFactionAction.ShouldRemain(_target, _faction));
            _target.Support = 100;
            Assert.True(JoinFactionAction.ShouldRemain(_target, _faction));
        }

        [Fact]
        public void AlreadyWillingClanJoinsWithoutAnInfluenceCharge()
        {
            _target.Support = 100;
            _player.Influence = 0;
            Assert.True(RecruitFactionSupportAction.TryAskForSupport(_target, _faction, out _));
            Assert.Contains(_target, _faction.Clans);
            Assert.Equal(0, _player.Influence);
            Assert.Equal(30, _manager.GetPledgeDaysRemaining(_target, _faction));
        }

        [Fact]
        public void AskingAHesitantClanDoesNotSpendInfluenceOrStartCooldown()
        {
            Assert.False(RecruitFactionSupportAction.TryAskForSupport(_target, _faction, out _));
            Assert.Equal(100, _player.Influence);
            Assert.Equal(0, _manager.GetCooldownDaysRemaining(_target));
            Assert.NotNull(RecruitFactionSupportAction.TryBeginPersuasion(_target, _faction, out _));
        }

        [Theory]
        [InlineData(74.99f, 100)]
        [InlineData(75, 24)]
        public void TooOpposedOrUnaffordableAttemptNeverChargesInfluence(float score, float influence)
        {
            _target.Support = score;
            _player.Influence = influence;
            Assert.Null(RecruitFactionSupportAction.TryBeginPersuasion(_target, _faction, out _));
            Assert.Equal(influence, _player.Influence);
            Assert.Equal(0, _manager.GetCooldownDaysRemaining(_target));
        }

        [Fact]
        public void FailedAttemptConsumesCostAndCooldownAndCannotBeCompletedAgain()
        {
            var record = Begin();
            Assert.False(RecruitFactionSupportAction.TryCompletePersuasion(record, false, out _));
            Assert.Equal(75, _player.Influence);
            Assert.Equal(14, _manager.GetCooldownDaysRemaining(_target));
            Assert.Null(RecruitFactionSupportAction.TryBeginPersuasion(_target, _faction, out _));
            Assert.False(RecruitFactionSupportAction.TryCompletePersuasion(record, true, out _));
            Assert.DoesNotContain(_target, _faction.Clans);
            CampaignTime.NowDays += 14;
            Assert.NotNull(RecruitFactionSupportAction.TryBeginPersuasion(_target, _faction, out _));
        }

        [Fact]
        public void CooldownAppliesAcrossDemandsAndFactions()
        {
            var attempt = Begin();
            attempt.Resolve(false);
            _kingdom.Factions.Remove(_faction);
            var other = NewFaction(RebelDemandType.Secession);
            Assert.False(RecruitFactionSupportAction.CanRecruit(_target, other, out _));
            CampaignTime.NowDays += 14;
            Assert.True(RecruitFactionSupportAction.CanRecruit(_target, other, out _));
        }

        [Fact]
        public void LoadingAnInterruptedAttemptRetainsItsCostAndCooldown()
        {
            var record = Begin();
            _ = new FactionRecruitmentManager(); // Simulate a fresh behavior being created before saved data is loaded.
            _manager.Sync();
            Assert.Same(_manager, FactionRecruitmentManager.Instance);
            Assert.True(record.Resolved);
            Assert.Equal(75, _player.Influence);
            Assert.Equal(14, _manager.GetCooldownDaysRemaining(_target));
            Assert.False(RecruitFactionSupportAction.TryCompletePersuasion(record, true, out _));
        }

        [Fact]
        public void LoadingAnExistingPledgePreservesItsExpiry()
        {
            Recruit();
            CampaignTime.NowDays += 10;
            _ = new FactionRecruitmentManager();
            _manager.Sync();
            Assert.Equal(20, _manager.GetPledgeDaysRemaining(_target, _faction));
            Assert.True(JoinFactionAction.ShouldRemain(_target, _faction));
        }

        [Fact]
        public void DisablingRecruitmentPreventsNewAttemptsAndHonorsExistingPledges()
        {
            Recruit();
            Settings.Instance.EnablePlayerFactionRecruitment = false;
            var other = new Clan { Kingdom = _kingdom, Support = 100 };
            Assert.False(RecruitFactionSupportAction.TryAskForSupport(other, _faction, out _));
            Assert.Null(RecruitFactionSupportAction.TryBeginPersuasion(other, _faction, out _));
            _target.Support = 0;
            Assert.True(JoinFactionAction.ShouldRemain(_target, _faction));
        }

        [Theory]
        [InlineData("leader")]
        [InlineData("kingdom")]
        [InlineData("kingdom eliminated")]
        [InlineData("ruler")]
        [InlineData("mercenary")]
        [InlineData("eliminated")]
        [InlineData("minor")]
        [InlineData("membership")]
        [InlineData("faction")]
        [InlineData("war")]
        [InlineData("secession")]
        public void PledgesEndWhenTheClanOrFactionBecomesIneligible(string change)
        {
            Recruit();
            switch (change)
            {
                case "secession":
                    var other = NewFaction(RebelDemandType.Secession);
                    other.SponsorClan = new() { Kingdom = _kingdom };
                    other.AddClan(_target);
                    break;
                case "leader": _target.Leader = new(); break;
                case "kingdom": _target.Kingdom = new(); break;
                case "kingdom eliminated": _kingdom.IsEliminated = true; break;
                case "ruler": _kingdom.RulingClan = _target; break;
                case "mercenary": _target.IsUnderMercenaryService = true; break;
                case "eliminated": _target.IsEliminated = true; break;
                case "minor": _target.IsMinorFaction = true; break;
                case "membership": _faction.Clans.Remove(_target); break;
                case "faction": _kingdom.Factions.Clear(); break;
                case "war": _faction.AtWar = true; break;
            }
            _manager.Cleanup();
            Assert.False(_manager.HasActivePledge(_target, _faction));
            Assert.Equal(0, _manager.GetPledgeDaysRemaining(_target, _faction));
            Assert.Equal(14, _manager.GetCooldownDaysRemaining(_target));
        }

        [Fact]
        public void PledgedClanDoesNotJoinAnotherFactionUntilThePledgeEnds()
        {
            Recruit();
            var other = NewFaction(RebelDemandType.Secession);
            other.SponsorClan = new() { Kingdom = _kingdom };
            _target.Support = 100;
            Assert.False(JoinFactionAction.ShouldApply(_target, other));
            CampaignTime.NowDays += 30;
            Assert.True(JoinFactionAction.ShouldApply(_target, other));
        }

        [Theory]
        [InlineData("leader")]
        [InlineData("kingdom")]
        [InlineData("prisoner")]
        [InlineData("faction")]
        [InlineData("score")]
        [InlineData("disabled")]
        public void ChangedCircumstancesCannotGrantSupportAfterPersuasion(string change)
        {
            var attempt = Begin();
            switch (change)
            {
                case "leader": _target.Leader = new(); break;
                case "kingdom": _target.Kingdom = new(); break;
                case "prisoner": _target.Leader.IsPrisoner = true; break;
                case "faction": _kingdom.Factions.Clear(); break;
                case "score": _target.Support = 74; break;
                case "disabled": Settings.Instance.EnablePlayerFactionRecruitment = false; break;
            }
            Assert.False(RecruitFactionSupportAction.TryCompletePersuasion(attempt, true, out _));
            Assert.DoesNotContain(_target, _faction.Clans);
            Assert.Equal(75, _player.Influence);
            Assert.True(attempt.Resolved);
        }

        [Fact]
        public void SettingsChangesOnlyAffectNewAttempts()
        {
            var attempt = Begin();
            Settings.Instance.FactionRecruitmentInfluenceCost = 1000;
            Settings.Instance.FactionRecruitmentPersuasionBonus = 0;
            Settings.Instance.FactionRecruitmentCooldownInDays = 0;
            Settings.Instance.FactionRecruitmentPledgeInDays = 0;
            Assert.True(RecruitFactionSupportAction.TryCompletePersuasion(attempt, true, out _));
            Assert.Equal(75, _player.Influence);
            Assert.Equal(14, _manager.GetCooldownDaysRemaining(_target));
            Assert.Equal(30, _manager.GetPledgeDaysRemaining(_target, _faction));
        }

        [Fact]
        public void ZeroCostCooldownAndPledgeAreSupportedWithoutOverlappingAttempts()
        {
            Settings.Instance.FactionRecruitmentInfluenceCost = 0;
            Settings.Instance.FactionRecruitmentCooldownInDays = 0;
            Settings.Instance.FactionRecruitmentPledgeInDays = 0;
            _player.Influence = 0;
            var first = Begin();
            Assert.Null(RecruitFactionSupportAction.TryBeginPersuasion(_target, _faction, out _));
            Assert.False(RecruitFactionSupportAction.TryCompletePersuasion(first, false, out _));
            Assert.True(RecruitFactionSupportAction.TryCompletePersuasion(Begin(), true, out _));
            Assert.Equal(0, _player.Influence);
            Assert.Equal(0, _manager.GetCooldownDaysRemaining(_target));
            Assert.Equal(0, _manager.GetPledgeDaysRemaining(_target, _faction));
            Assert.False(JoinFactionAction.ShouldRemain(_target, _faction));
        }

        [Fact]
        public void AnotherActiveCivilWarPreventsRecruitment()
        {
            NewFaction(RebelDemandType.Secession).AtWar = true;
            Assert.Null(RecruitFactionSupportAction.TryBeginPersuasion(_target, _faction, out _));
            Assert.Equal(100, _player.Influence);
        }

        [Fact]
        public void AiLedFactionsCannotUsePlayerRecruitment()
        {
            _faction.SponsorClan = new() { Kingdom = _kingdom };
            Assert.False(RecruitFactionSupportAction.CanRecruit(_target, _faction, out _));
        }

        [Fact]
        public void RecruitmentCannotBypassSecessionMembershipRestrictions()
        {
            var other = NewFaction(RebelDemandType.Secession);
            other.SponsorClan = new() { Kingdom = _kingdom };
            other.AddClan(_target);
            Assert.False(RecruitFactionSupportAction.CanRecruit(_target, _faction, out _));
            Assert.False(JoinFactionAction.ShouldApply(_target, _faction));
        }

        [Theory]
        [InlineData(100, 0, 25, true)]
        [InlineData(75, 25, 25, true)]
        [InlineData(74.99f, 100, 25, false)]
        [InlineData(75, 24, 25, false)]
        [InlineData(99, 100, 0, false)]
        [InlineData(100, 0, 0, true)]
        [InlineData(0, 25, 100, true)]
        public void CandidatesMustBeWillingOrReachableThroughAffordablePersuasion(float score, float influence, int bonus, bool eligible)
        {
            _target.Support = score;
            _player.Influence = influence;
            Settings.Instance.FactionRecruitmentPersuasionBonus = bonus;
            Assert.Equal(eligible, RecruitFactionSupportAction.CanSeekSupport(_target, _faction, out _));
            Assert.Equal(influence, _player.Influence);
            Assert.Equal(0, _manager.GetCooldownDaysRemaining(_target));
        }

        [Theory]
        [InlineData("member", "already supports")]
        [InlineData("leader", "leads Other faction")]
        [InlineData("secession", "supports Other faction")]
        [InlineData("ruler", "ruling clan")]
        [InlineData("mercenary", "Mercenary")]
        [InlineData("prisoner", "prisoner")]
        [InlineData("inactive", "unavailable")]
        [InlineData("no leader", "no leader")]
        [InlineData("minor", "Minor factions")]
        [InlineData("eliminated", "eliminated")]
        [InlineData("kingdom", "different kingdom")]
        [InlineData("no kingdom", "different kingdom")]
        [InlineData("rebel kingdom", "rebel kingdom")]
        public void UnavailableClansAreExcludedAndTheirDialogueExplainsWhy(string condition, string explanation)
        {
            _target.Support = 100;
            switch (condition)
            {
                case "member": _faction.AddClan(_target); break;
                case "leader": NewFaction(RebelDemandType.Abdication).SponsorClan = _target; break;
                case "secession":
                    var other = NewFaction(RebelDemandType.Secession);
                    other.SponsorClan = new() { Kingdom = _kingdom };
                    other.AddClan(_target);
                    break;
                case "ruler": _kingdom.RulingClan = _target; break;
                case "mercenary": _target.IsUnderMercenaryService = true; break;
                case "prisoner": _target.Leader.IsPrisoner = true; _target.Leader.IsActive = false; break;
                case "inactive": _target.Leader.IsActive = false; break;
                case "no leader": _target.Leader = null!; break;
                case "minor": _target.IsMinorFaction = true; break;
                case "eliminated": _target.IsEliminated = true; break;
                case "kingdom": _target.Kingdom = new(); break;
                case "no kingdom": _target.Kingdom = null!; break;
                case "rebel kingdom": _kingdom.Rebel = true; break;
            }
            Assert.False(RecruitFactionSupportAction.CanSeekSupport(_target, _faction, out var reason));
            Assert.Contains(explanation, reason.ToString());
        }

        [Fact]
        public void CandidatesOnCooldownAreHiddenUntilTheyCanBeApproachedAgain()
        {
            Begin().Resolve(false);
            Assert.False(RecruitFactionSupportAction.CanSeekSupport(_target, _faction, out var reason));
            Assert.Contains("wait 14 more days", reason.ToString());
            CampaignTime.NowDays += 14;
            Assert.True(RecruitFactionSupportAction.CanSeekSupport(_target, _faction, out _));
        }
    }
}