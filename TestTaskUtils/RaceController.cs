namespace WebApplication1;

using Microsoft.AspNetCore.Mvc;
using TaskUtils.TaskCompetition;
using System.Threading.Tasks;

[ApiController]
[Route("[controller]")]
public class RaceController(ILogger<RaceController> logger) : ControllerBase
{
    private readonly ILogger<RaceController> _logger = logger;

    [HttpGet("champion")]
    public async Task<IActionResult> GetChampion()
    {
        var tasks = CreateSampleTasks();
        var result = await Race.StartRacing<string>(RacePosition.Champion, tasks);
        return Ok(result);
    }

    [HttpGet("majority")]
    public async Task<IActionResult> GetMajority()
    {
        var tasks = CreateSampleTasks();
        var result = await Race.StartRacing<string>(RacePosition.Majority, tasks);
        return Ok(result);
    }

    [HttpGet("lanternrouge")]
    public async Task<IActionResult> GetLanternRouge()
    {
        var tasks = CreateSampleTasks();
        var result = await Race.StartRacing<string>(RacePosition.LanternRouge, tasks);
        return Ok(result);
    }

    private CompetitorTask<string>[] CreateSampleTasks()
    {
        return new[]
        {
            new CompetitorTask<string>("Task1", Task.Delay(1000).ContinueWith(_ => "Task1 Result")),
            new CompetitorTask<string>("Task2", Task.Delay(2000).ContinueWith(_ => "Task2 Result")),
            new CompetitorTask<string>("Task3", Task.Delay(3000).ContinueWith(_ => "Task3 Result")),
            new CompetitorTask<string>("Task4", Task.Delay(4000).ContinueWith(_ => "Task4 Result")),
            new CompetitorTask<string>("Task5", Task.Delay(5000).ContinueWith(_ => "Task5 Result"))
        };
    }
}