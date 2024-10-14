using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaskUtils.Models;

namespace TaskUtils.Workers
{
    public class FixedRateWorker(IHandler handler, WorkerConfig workerConfig, ILogger logger)
        : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (!IsActive())
                {
                    logger.LogWarning("[FixedRateWorker][{@handler}] worker is deactivated!", handler);
                    return;
                }

                logger.LogInformation("[FixedRateWorker][{@handler}] is Started ....", handler);

                while (!stoppingToken.IsCancellationRequested)
                {
                    DateTime startTime = DateTime.UtcNow;
                    try
                    {
                        using var cts = new CancellationTokenSource();
                        cts.CancelAfter(GetRate());
                        await handler.ProcessAsync(cts.Token);

                        TimeSpan elapsedTime = DateTime.UtcNow - startTime;
                        if (elapsedTime < GetRate())
                        {
                            await Task.Delay(GetRate() - elapsedTime, stoppingToken);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogWarning("[FixedRateWorker][{@handler}] Task was canceled.", handler);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("[FixedRateWorker] Error in the worker with handler: {@handler}, error: {@error}", handler, ex.Message);
                        await Task.Delay(GetRate(), stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    logger.LogInformation("[FixedRateWorker] Stopping the worker with handler: {@handler}", handler);
                }
                else
                {
                    logger.LogError("[FixedRateWorker] Error in the worker that caused it to stop, with handler: {@handler}, error: {@error}", handler, ex.Message);
                }
            }
        }

        private bool IsActive() => workerConfig.IsActive;

        private TimeSpan GetRate()
        {
            return TimeSpan.FromSeconds(workerConfig.RateInSeconds.Value);
        }
    }
}