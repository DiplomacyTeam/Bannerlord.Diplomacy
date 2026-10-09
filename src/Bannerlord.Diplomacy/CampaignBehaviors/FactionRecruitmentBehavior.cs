using Diplomacy.CivilWar;
using Diplomacy.CivilWar.Actions;
using Diplomacy.CivilWar.Factions;

using System;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Diplomacy.CampaignBehaviors
{
    internal sealed class FactionRecruitmentBehavior : CampaignBehaviorBase
    {
        private FactionRecruitmentManager _manager = new();
        private RebelFaction? _faction;
        private Clan? _target;
        private FactionRecruitmentRecord? _attempt;
        private TextObject _response = TextObject.GetEmpty();
        private bool _joined;
        private bool _persuaded;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, () => _manager.Cleanup());
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_factionRecruitmentManager", ref _manager);
            if (dataStore.IsLoading)
            {
                _manager ??= new();
                _manager.Sync();
            }
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            FactionRecruitmentPersuasion.End();
            Campaign.Current.ConversationManager.ConversationEnd += EndConversation;
            starter.AddPlayerLine("diplomacy_recruit_support", "lord_talk_speak_diplomacy_2", "diplomacy_recruit_response",
                "{=FRaskSup}Will your clan support {RECRUITMENT_FACTION_NAME} and our demand?", RecruitmentOnCondition, AskForSupport,
                clickableConditionDelegate: RecruitmentClickable);
            starter.AddDialogLine("diplomacy_recruit_response", "diplomacy_recruit_response", "diplomacy_recruit_options",
                "{=FRrespTxt}{FACTION_RECRUITMENT_RESPONSE}", ResponseOnCondition, null);
            starter.AddPlayerLine("diplomacy_recruit_persuade", "diplomacy_recruit_options", "diplomacy_recruit_argument_intro",
                "{=FRmakeCas}Let me make my case. (Cost: {RECRUITMENT_COST} influence.)", PersuadeOnCondition, BeginPersuasion,
                clickableConditionDelegate: PersuadeClickable);
            starter.AddPlayerLine("diplomacy_recruit_leave", "diplomacy_recruit_options", "lord_pretalk",
                "{=FRgoodbye}Very well. Let us speak of something else.", null, EndConversation, 1);
            starter.AddDialogLine("diplomacy_recruit_argument_intro", "diplomacy_recruit_argument_intro", "diplomacy_recruit_arguments",
                "{=FRintroTx}{FACTION_RECRUITMENT_RESPONSE}", ArgumentIntroOnCondition, null);
            for (var i = 0; i < 2; i++)
            {
                var index = i;
                starter.AddPlayerLine("diplomacy_recruit_argument_" + i, "diplomacy_recruit_arguments", "diplomacy_recruit_reaction",
                    i == 0 ? "{=FRargOne}{FACTION_RECRUITMENT_ARGUMENT_0}" : "{=FRargTwo}{FACTION_RECRUITMENT_ARGUMENT_1}",
                    () => ArgumentOnCondition(index), null,
                    persuasionOptionDelegate: () => FactionRecruitmentPersuasion.Arguments![index]);
            }
            starter.AddPlayerLine("diplomacy_recruit_cancel_argument", "diplomacy_recruit_arguments", "lord_pretalk",
                "{=FRcancelA}I will leave it at that.", null, EndConversation, 1);
            starter.AddDialogLine("diplomacy_recruit_reaction", "diplomacy_recruit_reaction", "diplomacy_recruit_decision",
                "{=FRreactTx}{FACTION_RECRUITMENT_RESPONSE}", ReactionOnCondition, null);
            starter.AddPlayerLine("diplomacy_recruit_decision", "diplomacy_recruit_decision", "diplomacy_recruit_response",
                "{=FRdecision}Then what is your decision?", null, CompletePersuasion);
        }

        private static RebelFaction? GetPlayerFaction()
            => Clan.PlayerClan.Kingdom is { } kingdom
                ? RebelFactionManager.GetRebelFaction(kingdom).FirstOrDefault(f => f.SponsorClan == Clan.PlayerClan && !f.AtWar)
                : null;

        private bool RecruitmentOnCondition()
        {
            var hero = Hero.OneToOneConversationHero;
            var faction = GetPlayerFaction();
            if (!Settings.Instance!.EnablePlayerFactionRecruitment || faction is null || hero?.Clan is null
                || hero != hero.Clan.Leader || hero.Clan == Clan.PlayerClan || hero.Clan.Kingdom != faction.ParentKingdom)
                return false;
            MBTextManager.SetTextVariable("RECRUITMENT_FACTION_NAME", faction.Name);
            return true;
        }

        private bool RecruitmentClickable(out TextObject reason)
        {
            var faction = GetPlayerFaction();
            var clan = Hero.OneToOneConversationHero?.Clan;
            reason = TextObject.GetEmpty();
            return faction is not null && clan is not null && RecruitFactionSupportAction.CanSeekSupport(clan, faction, out reason);
        }

        private void AskForSupport()
        {
            EndConversation();
            _faction = GetPlayerFaction();
            _target = Hero.OneToOneConversationHero?.Clan;
            if (_faction is null || _target is null)
            {
                _response = new("{=FRchanged}Circumstances have changed. This clan can no longer be recruited.");
                return;
            }
            _joined = RecruitFactionSupportAction.TryAskForSupport(_target, _faction, out _response);
            if (_joined)
                _response = GetPledgeResponse();
        }

        private bool ResponseOnCondition()
        {
            MBTextManager.SetTextVariable("FACTION_RECRUITMENT_RESPONSE", _response);
            return true;
        }

        private bool PersuadeOnCondition()
        {
            MBTextManager.SetTextVariable("RECRUITMENT_COST", Settings.Instance!.FactionRecruitmentInfluenceCost);
            return !_joined && _faction is not null && _target is not null;
        }

        private bool PersuadeClickable(out TextObject reason)
        {
            reason = TextObject.GetEmpty();
            return _faction is not null && _target is not null
                   && RecruitFactionSupportAction.CanPersuade(_target, _faction, out reason);
        }

        private void BeginPersuasion()
        {
            if (_target is null || _faction is null || Hero.OneToOneConversationHero != _target.Leader)
            {
                _response = new("{=FRchanged}Circumstances have changed. This clan can no longer be recruited.");
                return;
            }
            _attempt = RecruitFactionSupportAction.TryBeginPersuasion(_target, _faction, out _response);
            if (_attempt is null)
                return;
            FactionRecruitmentPersuasion.Begin(_target.Leader, CreateArguments(_target.Leader, _faction.RebelDemandType));
            ConversationManager.StartPersuasion(1f, 1f, 0f, 1f, 0f, 0f, PersuasionDifficulty.Medium);
        }

        private bool ArgumentIntroOnCondition()
        {
            if (_attempt is not null)
                _response = new("{=FRhearArg}Tell me why I should take this risk with you.");
            return ResponseOnCondition();
        }

        private bool ArgumentOnCondition(int index)
        {
            var arguments = FactionRecruitmentPersuasion.Arguments;
            if (arguments is null || arguments[index].IsBlocked)
                return false;
            MBTextManager.SetTextVariable("FACTION_RECRUITMENT_ARGUMENT_" + index, arguments[index].Line);
            return true;
        }

        private bool ReactionOnCondition()
        {
            var result = ConversationManager.GetPersuasionChosenOptions().LastOrDefault();
            _persuaded = result?.Item2 is PersuasionOptionResult.Success or PersuasionOptionResult.CriticalSuccess;
            _response = _persuaded
                ? new TextObject("{=FRargWin}You make a compelling case.")
                : new TextObject("{=FRargLoss}That does not convince me.");
            return ResponseOnCondition();
        }

        private void CompletePersuasion()
        {
            if (_attempt is not null)
            {
                _joined = RecruitFactionSupportAction.TryCompletePersuasion(_attempt,
                    _persuaded && Hero.OneToOneConversationHero == _attempt.Leader, out _response);
                if (_joined)
                    _response = GetPledgeResponse();
            }
            StopPersuasion();
            _attempt = null;
        }

        private TextObject GetPledgeResponse()
        {
            var days = _manager.GetPledgeDaysRemaining(_target!, _faction!);
            return days > 0
                ? new TextObject("{=FRpledged}My clan will stand with {FACTION_NAME}. I pledge our support for {DAYS} days.")
                    .SetTextVariable("FACTION_NAME", _faction!.Name).SetTextVariable("DAYS", days)
                : new TextObject("{=FRjoinNow}My clan will stand with {FACTION_NAME}.").SetTextVariable("FACTION_NAME", _faction!.Name);
        }

        private void StopPersuasion()
        {
            if (!FactionRecruitmentPersuasion.IsActive)
                return;
            FactionRecruitmentPersuasion.End();
            ConversationManager.EndPersuasion();
        }

        private void EndConversation()
        {
            if (_attempt is not null && _manager.IsPendingAttempt(_attempt))
                _attempt.Resolve(false);
            StopPersuasion();
            _attempt = null;
            _faction = null;
            _target = null;
            _joined = false;
            _persuaded = false;
        }

        private static PersuasionOptionArgs[] CreateArguments(Hero leader, RebelDemandType demand)
        {
            var abdication = demand == RebelDemandType.Abdication;
            var firstTrait = abdication ? DefaultTraits.Honor : DefaultTraits.Calculating;
            var secondTrait = abdication ? DefaultTraits.Generosity : DefaultTraits.Valor;
            var firstLine = abdication
                ? new TextObject("{=FRabdHon}Our kingdom deserves a ruler who honors its clans. Help us choose better leadership.")
                : new TextObject("{=FRsecCal}Our clans can prosper as an independent kingdom. We should decide our own future.");
            var secondLine = abdication
                ? new TextObject("{=FRabdGen}Your clan's service deserves a fairer share of land and influence. Stand with us.")
                : new TextObject("{=FRsecVal}Together our clans can defend a realm of our own. Let us stand together.");
            return new[] { CreateArgument(leader, firstTrait, firstLine), CreateArgument(leader, secondTrait, secondLine) };
        }

        private static PersuasionOptionArgs CreateArgument(Hero leader, TraitObject trait, TextObject line)
        {
            var correlations = new[] { Tuple.Create(trait, 1) };
            var strength = Campaign.Current.Models.PersuasionModel.GetArgumentStrengthBasedOnTargetTraits(leader.CharacterObject, correlations);
            return new PersuasionOptionArgs(DefaultSkills.Charm, trait, TraitEffect.Positive, strength, false, line, correlations);
        }
    }
}