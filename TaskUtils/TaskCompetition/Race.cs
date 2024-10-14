

using Microsoft.Extensions.Logging;
using TaskUtils.Models;

namespace TaskUtils.TaskCompetition
{
    public static class Race
    {
        private static readonly ILogger logger = StaticLogger.GetLogger();

        public static async Task<object> StartRacing<T>(RacePosition position, CompetitorTask[] competitorTasks)
        {
            logger.LogInformation($"[JobUtils][Race] Starting the race for position: {position}");

            switch (position)
            {
                case RacePosition.Champion:
                    var championTask = await Task.WhenAny(competitorTasks.Select(ct => ct.Task));
                    var champion = competitorTasks.First(ct => ct.Task == championTask);
                    logger.LogInformation($"[JobUtils][Race] The Champion Task '{champion.Name}' has crossed the finish line!");
                    break;

                case RacePosition.Majority:
                    int majority = (competitorTasks.Length + 1) / 2;
                    var results = new List<T>();
                    for (int i = 0; i < majority; i++)
                    {
                        var completedTask = await Task.WhenAny(competitorTasks.Select(ct => ct.Task));
                        var completedCompetitor = competitorTasks.First(ct => ct.Task == completedTask);
                        logger.LogInformation($"[JobUtils][Race] Task '{completedCompetitor.Name}' completed.");
                        competitorTasks = competitorTasks.Where(ct => ct.Task != completedTask).ToArray();
                    }
                    logger.LogInformation("[JobUtils][Race] Majority of the tasks have completed! Majority tasks can proceed.");
                    return results;

                case RacePosition.LanternRouge:
                    await Task.WhenAll(competitorTasks.Select(ct => ct.Task));
                    logger.LogInformation("[JobUtils][Race] The Lantern Rouge has finished the race!");
                    break;
            }

            return null;
        }

        public static async Task<object> StartRacing<T>(RacePosition position, CompetitorTask<T>[] competitorTasks)
        {
            logger.LogInformation($"[JobUtils][Race] Starting the race for position: {position}");

            switch (position)
            {
                case RacePosition.Champion:
                    var championTask = await Task.WhenAny(competitorTasks.Select(ct => ct.Task));
                    var champion = competitorTasks.First(ct => ct.Task == championTask);
                    logger.LogInformation($"[JobUtils][Race] The Champion Task '{champion.Name}' has crossed the finish line!");
                    return await championTask;

                case RacePosition.Majority:
                    int majority = (competitorTasks.Length + 1) / 2;
                    var results = new List<T>();
                    for (int i = 0; i < majority; i++)
                    {
                        var completedTask = await Task.WhenAny(competitorTasks.Select(ct => ct.Task));
                        var completedCompetitor = competitorTasks.First(ct => ct.Task == completedTask);
                        logger.LogInformation($"[JobUtils][Race] Task '{completedCompetitor.Name}' completed.");
                        results.Add(await completedTask);
                        competitorTasks = competitorTasks.Where(ct => ct.Task != completedTask).ToArray();
                    }
                    logger.LogInformation("[JobUtils][Race] Half of the tasks have completed! Majority tasks can proceed.");
                    return results;

                case RacePosition.LanternRouge:
                    await Task.WhenAll(competitorTasks.Select(ct => ct.Task));
                    logger.LogInformation("[JobUtils][Race] The Lantern Rouge has finished the race!");
                    return competitorTasks.Select(ct => ct.Task.Result).ToList();

                default:
                    return null;
            }
        }
    }
}