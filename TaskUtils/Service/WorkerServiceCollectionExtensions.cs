using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskUtils.Models;
using TaskUtils.Workers;

namespace TaskUtils.Service;

  public static class WorkerServiceCollectionExtensions
  {
    public static IServiceCollection AddWorkerService(
      this IServiceCollection services,
      IConfiguration configuration,
      params IHandler[] handlers)
    {
      List<WorkerConfig> workerConfigs = configuration.GetSection("workers").Get<List<WorkerConfig>>();
      if (workerConfigs == null || workerConfigs.Count == 0)
        throw new InvalidOperationException("No worker configurations found in the provided configuration section.");
      return services.AddWorkerService(workerConfigs, handlers);
    }

    public static IServiceCollection AddWorkerService(
      this IServiceCollection services,
      WorkerConfig configuration,
      params IHandler[] handlers)
    {
      IServiceCollection services1 = services;
      List<WorkerConfig> workerConfigs = new List<WorkerConfig>();
      workerConfigs.Add(configuration);
      IHandler[] handlerArray = handlers;
      return services1.AddWorkerService(workerConfigs, handlerArray);
    }

    public static IServiceCollection AddWorkerService(
      this IServiceCollection services,
      List<WorkerConfig> workerConfigs,
      params IHandler[] handlers)
    {
      ILogger<WorkerConfig> requiredService = services.BuildServiceProvider().GetRequiredService<ILogger<WorkerConfig>>();
      WorkerServiceCollectionExtensions.ValidateWorkers(workerConfigs, (ILogger) requiredService);
      WorkerServiceCollectionExtensions.ValidateHandlers(handlers, workerConfigs, (ILogger) requiredService);
      foreach (IHandler handler in handlers)
        services.AddSingleton(handler.GetType(), (object) handler);
      WorkerServiceCollectionExtensions.RegisterWorkers(services, handlers, workerConfigs, (ILogger) requiredService);
      return services;
    }

    private static void ValidateWorkers(List<WorkerConfig> workerConfigs, ILogger logger)
    {
      if (workerConfigs == null || workerConfigs.Count == 0)
        throw new InvalidOperationException("No worker configurations found in the provided configuration section.");
      foreach (WorkerConfig workerConfig in workerConfigs)
      {
        ValidationContext validationContext = new ValidationContext((object) workerConfig);
        List<ValidationResult> validationResultList = new List<ValidationResult>();
        if (!Validator.TryValidateObject((object) workerConfig, validationContext, (ICollection<ValidationResult>) validationResultList, true))
        {
          foreach (ValidationResult validationResult in validationResultList)
            logger.LogError("Configuration error for {HandlerName}: {error}", (object) workerConfig.HandlerName, (object) validationResult.ErrorMessage);
        }
      }
    }

    private static void ValidateHandlers(
      IHandler[] handlers,
      List<WorkerConfig> workerConfigs,
      ILogger logger)
    {
      HashSet<string> handlerNamesInConfigs = workerConfigs.Select<WorkerConfig, string>((Func<WorkerConfig, string>) (w => w.HandlerName)).ToHashSet<string>();
      List<IHandler> list = ((IEnumerable<IHandler>) handlers).Where<IHandler>((Func<IHandler, bool>) (h => !handlerNamesInConfigs.Contains(h.GetType().Name))).ToList<IHandler>();
      if (list.Any<IHandler>())
      {
        foreach (IHandler handler in list)
          logger.LogWarning("Handler {HandlerName} has no associated worker, please check configuration.", (object) handler.GetType().Name);
      }
      foreach (string str in handlerNamesInConfigs.Except<string>(((IEnumerable<IHandler>) handlers).Select<IHandler, string>((Func<IHandler, string>) (h => h.GetType().Name))).ToList<string>())
        logger.LogWarning("Handler {HandlerName} is configured in appsettings but has no matching implementation.", (object) str);
    }

    private static void RegisterWorkers(
      IServiceCollection services,
      IHandler[] handlers,
      List<WorkerConfig> workerConfigs,
      ILogger logger)
    {
      foreach (WorkerConfig workerConfig in workerConfigs)
      {
        WorkerConfig config = workerConfig;
        IHandler handler = ((IEnumerable<IHandler>) handlers).FirstOrDefault<IHandler>((Func<IHandler, bool>) (h => h.GetType().Name == config.HandlerName));
        if (handler != null)
        {
          switch (config.WorkerType)
          {
            case WorkerType.FixedDelay:
              services.AddHostedService<FixedDelayWorker>((Func<IServiceProvider, FixedDelayWorker>) (provider => new FixedDelayWorker(handler, config, (ILogger) provider.GetRequiredService<ILogger<FixedDelayWorker>>())));
              break;
            case WorkerType.FixedRate:
              services.AddHostedService<FixedRateWorker>((Func<IServiceProvider, FixedRateWorker>) (provider => new FixedRateWorker(handler, config, (ILogger) provider.GetRequiredService<ILogger<FixedRateWorker>>())));
              break;
            case WorkerType.Scheduled:
              services.AddHostedService<ScheduledWorker>((Func<IServiceProvider, ScheduledWorker>) (provider => new ScheduledWorker(handler, config, (ILogger) provider.GetRequiredService<ILogger<ScheduledWorker>>())));
              break;
            default:
              DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 1);
              interpolatedStringHandler.AppendLiteral("Worker type ");
              interpolatedStringHandler.AppendFormatted<WorkerType>(config.WorkerType);
              interpolatedStringHandler.AppendLiteral(" is not supported.");
              throw new NotSupportedException(interpolatedStringHandler.ToStringAndClear());
          }
        }
      }
    }
  }