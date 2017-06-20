using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace LoggingSignoff.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger _logger;
        public IndexModel(ILogger<IndexModel> logger, IOptionsMonitor<ConsoleLoggerOptions> options)
        {
            _logger = logger;
        }

        public void OnGet(string trigger)
        {
            _logger.LogDebug($"DEBUG YO DEBUG Enabled={_logger.IsEnabled(LogLevel.Debug)}");

            _logger.LogInformation($"INFO YO DEBUG Enabled={_logger.IsEnabled(LogLevel.Debug)}");

            _logger.LogCritical($"CRIT YO DEBUG Enabled={_logger.IsEnabled(LogLevel.Debug)}");

            if (trigger != null)
            {
                MyThing.Update();
            }
        }
    }
}
