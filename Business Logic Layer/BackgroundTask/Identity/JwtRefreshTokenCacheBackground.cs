using Business_Logic_Layer.Services.Identity;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Business_Logic_Layer.BackgroundTask.Identity
{
    public class JwtRefreshTokenCacheBackground : BackgroundService
    {
        private readonly JwtAuthManager _jwtAuthManager;

        public JwtRefreshTokenCacheBackground(JwtAuthManager jwtAuthManager)
        {
            _jwtAuthManager = jwtAuthManager;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await BackgroundProcessing(stoppingToken).ConfigureAwait(false);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await base.StopAsync(stoppingToken).ConfigureAwait(false);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            while (true)
            {
                await Task.Delay(60 * 1000, stoppingToken);
                _jwtAuthManager.RemoveExpiredRefreshTokens(DateTimeOffset.Now);
            }
        }
    }
}
