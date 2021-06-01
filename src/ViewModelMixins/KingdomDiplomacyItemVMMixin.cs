using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using Diplomacy.ViewModel;
using TaleWorlds.Library;

namespace Diplomacy.ViewModelMixins
{
    [ViewModelMixin]
    internal sealed class KingdomTruceItemVMMixin : BaseViewModelMixin<KingdomTruceItemVMExtensionVM>
    {
        public KingdomTruceItemVMMixin(KingdomTruceItemVMExtensionVM vm) : base(vm)
        {
            IsWarItem = false;
        }

        [DataSourceProperty]
        public bool IsWarItem { get; }
    }

    [ViewModelMixin]
    internal sealed class KingdomWarItemVMMixin : BaseViewModelMixin<KingdomWarItemVMExtensionVM>
    {
        public KingdomWarItemVMMixin(KingdomWarItemVMExtensionVM vm) : base(vm)
        {
            IsWarItem = true;
        }

        [DataSourceProperty]
        public bool IsWarItem { get; }
    }
}
