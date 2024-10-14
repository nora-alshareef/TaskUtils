

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaskUtils.Models;

namespace TaskUtils.Workers
{
    public class FixedDelayWorker(IHandler handler, WorkerConfig workerConfig, ILogger logger)
        : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!IsActive())
            {
                logger.LogWarning("[FixedDelayWorker][{@handler}] worker is deactivated!", handler);
                return;
            }

            logger.LogInformation("[FixedDelayWorker][{@handler}] is Started ....", handler);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        using var cts = new CancellationTokenSource();
                        cts.CancelAfter(GetIterationTimeout());
                        await handler.ProcessAsync(cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogWarning("[FixedDelayWorker][{@handler}] Task was canceled.", handler);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("[FixedRateWorker] Error in the worker with handler: {@handler}, error: {@error}", handler, ex.Message);
                    }

                    await Task.Delay(GetDelayTime(), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    logger.LogInformation("[FixedDelayWorker] Stopping the worker with handler: {@handler}", handler);
                }
                else
                {
                    logger.LogError("[FixedDelayWorker] Error in the worker that cause it to stop, with handler: {@handler}, error: {@error}", handler, ex.Message);
                }
            }
        }

        private TimeSpan GetDelayTime()
        {
            return TimeSpan.FromSeconds(workerConfig.DelayInSeconds.Value);
        }

        private TimeSpan GetErrorDelayTime()
        {
            return TimeSpan.FromSeconds(workerConfig.ErrorDelayInSeconds.Value);
        }

        private bool IsActive() => workerConfig.IsActive;

        private TimeSpan GetIterationTimeout()
        {
            return TimeSpan.FromSeconds(workerConfig.IterationTimeoutInSeconds);
        }
    }
}