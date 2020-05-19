using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.Alliance
{
    class AllianceFormedEvent
    {
        public AllianceFormedEvent(Kingdom kingdom, Kingdom otherKingdom)
        {
            this.Kingdom = kingdom;
            this.OtherKingdom = otherKingdom;
        }

        public Kingdom Kingdom { get; }
        public Kingdom OtherKingdom { get; }
    }
}
