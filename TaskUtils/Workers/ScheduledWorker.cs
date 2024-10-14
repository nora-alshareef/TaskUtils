using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskUtils.Models;

namespace TaskUtils.Workers
{
    public class ScheduledWorker : BackgroundService
    {
        private readonly IHandler handler;
        private readonly WorkerConfig workerConfig;
        private readonly ILogger logger;
        private readonly List<TimeSpan> _executionTimes;

        public ScheduledWorker(IHandler handler, WorkerConfig workerConfig, ILogger logger)
        {
            this.handler = handler;
            this.workerConfig = workerConfig;
            this.logger = logger;
            this._executionTimes = workerConfig.SpecifiedHours
                .Select(TimeSpan.Parse)
                .OrderBy(t => t)
                .ToList();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!IsActive())
            {
                logger.LogWarning("[ScheduledWorker][{@handler}] worker is deactivated!", handler);
                return;
            }

            logger.LogInformation("[ScheduledWorker][{@handler}] is Started ....", handler);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        DateTime now = DateTime.Now;
                        DateTime nextRun = GetNextExecutionTime(now);
                        TimeSpan delay = nextRun - now;

                        if (delay > TimeSpan.Zero)
                        {
                            logger.LogInformation("[ScheduledWorker][{HandlerName}] next run scheduled at: {NextRun}", workerConfig.HandlerName, nextRun);
                            await Task.Delay(delay, stoppingToken);
                        }

                        logger.LogInformation("[ScheduledWorker][{HandlerName}] is running at: {Time}", workerConfig.HandlerName, DateTimeOffset.Now);
                        await handler.ProcessAsync(stoppingToken);
                        await Task.Delay(1000, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("[ScheduledWorker] Error in the worker with handler: {@handler}, error: {@error}", handler, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    logger.LogInformation("[ScheduledWorker] Stopping the worker with handler: {@handler}", handler);
                }
                else
                {
                    logger.LogError("[ScheduledWorker] Error in the worker with handler: {@handler}, error: {@error}", handler, ex.Message);
                }
            }
        }

        private DateTime GetNextExecutionTime(DateTime fromTime)
        {
            TimeSpan currentTimeOfDay = fromTime.TimeOfDay;
            TimeSpan nextTimeSpan = _executionTimes.FirstOrDefault(t => t > currentTimeOfDay);

            if (nextTimeSpan != default)
            {
                return fromTime.Date.Add(nextTimeSpan);
            }
            else
            {
                return fromTime.Date.AddDays(1).Add(_executionTimes[0]);
            }
        }

        private bool IsActive() => workerConfig.IsActive;
    }
}