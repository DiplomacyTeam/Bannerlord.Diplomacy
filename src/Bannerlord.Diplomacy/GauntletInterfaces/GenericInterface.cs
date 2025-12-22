using HarmonyLib.BUTR.Extensions;

using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.ScreenSystem;

namespace Diplomacy.GauntletInterfaces
{
    public abstract class GenericInterface
    {
        protected bool _isShown = false;

        protected GauntletLayer _layer = default!;

        protected GauntletMovieIdentifier? _movie;
        protected ScreenBase _screenBase = default!;
        protected TaleWorlds.Library.ViewModel? _vm;

        protected abstract string MovieName { get; }

        protected bool ShowInterfaceWithCheck()
        {
            if (_isShown)
                return false;
            return _isShown = true;
        }

        protected GauntletMovieIdentifier? LoadMovie()
        {
            return _layer.LoadMovie(MovieName, _vm);
        }

        protected virtual void OnFinalize()
        {
            _screenBase.RemoveLayer(_layer);
            if (_movie is not null && _movie is not null)
                _layer.ReleaseMovie(_movie);
            _layer = null!;
            _movie = null!;
            _vm = null;
            _screenBase = null!;
            _isShown = false;
        }
    }
}