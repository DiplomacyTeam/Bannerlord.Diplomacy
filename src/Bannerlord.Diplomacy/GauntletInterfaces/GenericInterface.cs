using HarmonyLib.BUTR.Extensions;

using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.ScreenSystem;

namespace Diplomacy.GauntletInterfaces
{
    public abstract class GenericInterface
    {
        protected bool _isShown = false;

        protected static readonly LoadMovieDelegate? LoadMovieDel =
            AccessTools2.GetDelegate<LoadMovieDelegate>(typeof(GauntletLayer), "LoadMovie");

        protected static readonly ReleaseMovieDelegate? ReleaseMovieDel =
            AccessTools2.GetDelegate<ReleaseMovieDelegate>(typeof(GauntletLayer), "ReleaseMovie");

        protected GauntletLayer _layer = default!;

        protected object? _movie;
        protected ScreenBase _screenBase = default!;
        protected TaleWorlds.Library.ViewModel? _vm;

        protected abstract string MovieName { get; }

        protected bool ShowInterfaceWithCheck()
        {
            if (_isShown)
                return false;
            return _isShown = true;
        }

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
            _isShown = false;
        }

        protected delegate object LoadMovieDelegate(object instance, string movieName, TaleWorlds.Library.ViewModel dataSource);

        protected delegate void ReleaseMovieDelegate(object instance, object movie);
    }
}