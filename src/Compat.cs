using TaleWorlds.Localization;

namespace Diplomacy
{
    internal static class Compat
    {
        internal static class HintViewModel
        {
            public static TaleWorlds.Core.ViewModelCollection.HintViewModel Create(TextObject text)
            {
#if STABLE
                return new(text.ToString());
#else
                return new(text);
#endif
            }
        }
    }
}
