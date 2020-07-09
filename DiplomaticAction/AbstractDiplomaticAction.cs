
using System;
using System.ComponentModel;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.DiplomaticAction
{
    abstract class AbstractDiplomaticAction<T> where T : AbstractDiplomaticAction<T>, new()
    {
        public static void Apply(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            Instance.TryApply(kingdom, otherKingdom, forcePlayerCharacterCosts, bypassCosts);
        }

        public static void CanApply(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            Instance.PassesConditions(kingdom, otherKingdom, forcePlayerCharacterCosts, bypassCosts);
        }

        protected static T Instance { get; } = new T();

        public abstract bool PassesConditions(Kingdom proposingKingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, bool bypassCosts = false);
        protected void TryApply(Kingdom proposingKingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            if (otherKingdom == Clan.PlayerClan.Kingdom && otherKingdom.Leader.IsHumanPlayerCharacter)
            {
                ShowPlayerInquiry(proposingKingdom, () => ApplyInternalWithCosts(proposingKingdom, otherKingdom, forcePlayerCharacterCosts, bypassCosts));
            }
            else
            {
                ApplyInternalWithCosts(proposingKingdom, otherKingdom, forcePlayerCharacterCosts, bypassCosts);
            }
        }
        protected abstract void ShowPlayerInquiry(Kingdom proposingKingdom, Action applyAction);
        protected abstract void ApplyInternal(Kingdom proposingKingdom, Kingdom otherKingdom);
        protected void ApplyInternalWithCosts(Kingdom proposingKingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts, bool bypassCosts)
        {
            if (!bypassCosts)
            {
                this.AssessCosts(proposingKingdom, otherKingdom, forcePlayerCharacterCosts);
            }
            this.ApplyInternal(proposingKingdom, otherKingdom);
        }
        protected abstract void AssessCosts(Kingdom proposingKingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts);
    }
}
