using TaleWorlds.Localization;

namespace Diplomacy
{
    internal static class Compat
    {
        internal static class HintViewModel
        {
            public static TaleWorlds.Core.ViewModelCollection.Information.HintViewModel Create(TextObject text)
            {
                return new(text);
            }
        }
    }
}