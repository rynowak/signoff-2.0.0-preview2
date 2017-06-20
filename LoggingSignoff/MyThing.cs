using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace LoggingSignoff
{
    internal class MyThing : IOptionsChangeTokenSource<ConsoleLoggerOptions>, IOptionsChangeTokenSource<LoggerFilterOptions>
    {
        private static CancellationChangeToken _token;

        public static CancellationChangeToken Token
        {
            get
            {
                if (_token == null || _token.HasChanged)
                {
                    Source = new CancellationTokenSource();
                    _token = new CancellationChangeToken(Source.Token);
                }

                return _token;
            }
        }

        public static CancellationTokenSource Source { get; private set; }

        public static void Update()
        {
            var old = (Token, Source);
            old.Item2.Cancel();
        }

        public string Name => "Billy";

        public IChangeToken GetChangeToken()
        {
            return Token;
        }
    }
}