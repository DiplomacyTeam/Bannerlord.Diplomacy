using HarmonyLib;
using HarmonyLib.BUTR.Extensions;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;

namespace Diplomacy.GauntletInterfaces
{
    public abstract class GenericInterface
    {
        protected static readonly LoadMovieDelegate? LoadMovieDel =
            AccessTools2.GetDelegateObjectInstance<LoadMovieDelegate>(AccessTools.Method(typeof(GauntletLayer), "LoadMovie"));

        protected static readonly ReleaseMovieDelegate? ReleaseMovieDel =
            AccessTools2.GetDelegateObjectInstance<ReleaseMovieDelegate>(AccessTools.Method(typeof(GauntletLayer), "ReleaseMovie"));

        protected GauntletLayer _layer = default!;

        protected object? _movie;
        protected ScreenBase _screenBase = default!;
        protected TaleWorlds.Library.ViewModel? _vm;

        protected abstract string MovieName { get; }

        protected object? LoadMovie()
        {
            return LoadMovieDel?.Invoke(_layer, MovieName, _vm!);
        }

        protected virtual void OnFinalize()
        {
            _screenBase.RemoveLayer(_layer);
            if (_movie is not null && ReleaseMovieDel is not null)
                ReleaseMovieDel(_layer, _movie);
            _layer = null!;
            _movie = null!;
            _vm = null;
            _screenBase = null!;
        }

        protected delegate object LoadMovieDelegate(object instance, string movieName, TaleWorlds.Library.ViewModel dataSource);

        protected delegate void ReleaseMovieDelegate(object instance, object movie);
    }
}