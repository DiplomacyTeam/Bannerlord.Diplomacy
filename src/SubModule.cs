using Bannerlord.ButterLib.Common.Extensions;

using Diplomacy.CampaignEventBehaviors;

using HarmonyLib;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Serilog.Events;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Diplomacy
{
    public sealed class SubModule : MBSubModuleBase
    {
        public static readonly int VersionMajor = 0;
        public static readonly int VersionMinor = 1;
        public static readonly int VersionPatch = 0;
        public static readonly string Version = $"d{VersionMajor}.{VersionMinor}.{VersionPatch}";

        public static readonly string Name = typeof(SubModule).Namespace;
        public static readonly string DisplayName = Name;
        public static readonly string HarmonyDomain = "com.zijistark.bannerlord." + Name.ToLower();

        internal static readonly Color SignatureTextColor = Color.FromUint(0x00F16D26); // Orange

        internal static SubModule Instance { get; set; } = default!;

        private static ILogger Log { get; set; } = default!;

        private bool _hasLoaded;

        internal SubModule() => Instance = this;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            this.AddSerilogLoggerProvider($"{Name}.log", new[] { $"{Name}.*" }, config => config.MinimumLevel.Is(LogEventLevel.Verbose));
            InitializeLog<SubModule>();

            new Harmony(HarmonyDomain).PatchAll();
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
            Log.LogInformation($"Unloaded {Name}!");
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            if (!_hasLoaded)
            {
                _hasLoaded = true;

                InitializeLog<SubModule>();
                Log.LogInformation($"Loaded {Name}!");

                InformationManager.DisplayMessage(new InformationMessage($"Loaded {DisplayName} {Version}", SignatureTextColor));
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (game.GameType is Campaign)
            {
                Events.Instance = new Events();
                var gameStarter = (CampaignGameStarter)gameStarterObject;

                gameStarter.AddBehavior(new DiplomaticAgreementBehavior());
                gameStarter.AddBehavior(new CooldownBehavior());
                gameStarter.AddBehavior(new MessengerBehavior());

                if (Settings.Instance!.EnableWarExhaustion)
                    gameStarter.AddBehavior(new WarExhaustionBehavior());

                if (Settings.Instance!.EnableFiefFirstRight)
                    gameStarter.AddBehavior(new KeepFiefAfterSiegeBehavior());

                gameStarter.AddBehavior(new AllianceBehavior());
                gameStarter.AddBehavior(new MaintainInfluenceBehavior());
                gameStarter.AddBehavior(new ExpansionismBehavior());
            }
        }

        public override void OnGameEnd(Game game) => Events.Instance = default!;

        private void InitializeLog<T>()
        {
            var provider = this.GetServiceProvider() ?? this.GetTempServiceProvider();
            Log = provider.GetRequiredService<ILogger<T>>() ?? NullLogger<T>.Instance;
        }
    }
}
