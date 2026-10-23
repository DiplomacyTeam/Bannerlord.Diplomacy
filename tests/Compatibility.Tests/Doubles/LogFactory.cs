using Microsoft.Extensions.Logging;

namespace Diplomacy
{
    internal static class LogFactory
    {
        public static List<string> Warnings { get; } = new();
        public static ILogger Get<T>() => new TestLogger();

        private sealed class TestLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => throw new NotSupportedException();
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter) => Warnings.Add(formatter(state, exception));
        }
    }
}
