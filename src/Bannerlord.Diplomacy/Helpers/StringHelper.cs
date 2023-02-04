using TaleWorlds.Localization;

namespace Diplomacy.Helpers
{
    internal static class StringHelper
    {
        private static readonly TextObject _TPlus = new("{=eTw2aNV5}+");

        internal static string GetPlusPrefixed(float value)
        {
            return $"{(value >= 0.0005f ? _TPlus.ToString() : string.Empty)}{value:0.##}";
        }
    }
}