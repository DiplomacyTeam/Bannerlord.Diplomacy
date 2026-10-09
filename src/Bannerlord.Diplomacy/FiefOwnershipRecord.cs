using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace Diplomacy
{
    /// <summary>
    /// A single change of ownership for a fief, kept so that a fief can later be handed back to
    /// whoever held it before it was taken.
    /// </summary>
    internal sealed class FiefOwnershipRecord
    {
        [SaveableField(1)]
        private readonly Kingdom? _previousKingdom;

        [SaveableField(2)]
        private readonly Clan? _previousOwnerClan;

        [SaveableField(3)]
        private readonly Kingdom? _newKingdom;

        [SaveableField(4)]
        private readonly Clan? _newOwnerClan;

        [SaveableField(5)]
        private readonly CampaignTime _transferDate;

        [SaveableField(6)]
        private readonly bool _byConquest;

        public Kingdom? PreviousKingdom => _previousKingdom;
        public Clan? PreviousOwnerClan => _previousOwnerClan;
        public Kingdom? NewKingdom => _newKingdom;
        public Clan? NewOwnerClan => _newOwnerClan;
        public CampaignTime TransferDate => _transferDate;
        public bool ByConquest => _byConquest;

        /// <summary>Whether this transfer moved the fief from one kingdom to another.</summary>
        public bool IsCrossKingdom => _previousKingdom != _newKingdom;

        public FiefOwnershipRecord(Kingdom? previousKingdom,
                                   Clan? previousOwnerClan,
                                   Kingdom? newKingdom,
                                   Clan? newOwnerClan,
                                   CampaignTime transferDate,
                                   bool byConquest)
        {
            _previousKingdom = previousKingdom;
            _previousOwnerClan = previousOwnerClan;
            _newKingdom = newKingdom;
            _newOwnerClan = newOwnerClan;
            _transferDate = transferDate;
            _byConquest = byConquest;
        }
    }
}
