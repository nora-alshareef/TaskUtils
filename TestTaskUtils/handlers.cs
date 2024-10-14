
using TaskUtils.Workers;
using TaskUtils.Workers;

namespace WebApplication1;

public class HandlerA(ILogger<IHandler> logger) : IHandler
{
    private static int id = 0;

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        id++;
        // Simulate some work
        logger.LogInformation("[HandlerA]start t:{Id} task at ------------------------------------ {Time}", id, DateTime.Now);
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Delay(3000, cancellationToken);
        logger.LogInformation("still t:{Id} task at ------------------------------------ {Time}", id, DateTime.Now);
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Delay(3000, cancellationToken);
        logger.LogInformation("finish t:{Id} task at ------------------------------------ {Time}", id, DateTime.Now);
    }
}
public class HandlerB(ILogger<IHandler> logger) : IHandler
{
    private static int id = 0;

    // no cancellationToken.ThrowIfCancellationRequested(); ,
    // so the task will not be cancelled or interrupted, it will continue until it finishes
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        id++;
        logger.LogInformation("[HandlerB]start t:{Id} task at ------------------------------------ {Time}", id, DateTime.Now);
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Delay(2000, cancellationToken);
        logger.LogInformation("finish t:{Id} task at ------------------------------------ {Time}", id, DateTime.Now);
    }
}

public class HandlerC(ILogger<IHandler> logger) : IHandler
{
    private static int id = 0;

    // no cancellationToken.ThrowIfCancellationRequested(); ,
    // so the task will not be cancelled or interrupted, it will continue until it finishes

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        id++;
        logger.LogInformation("[HandlerC]start t:{Id} task at ------------------------------------ {Time}", id, DateTime.Now);
        await Task.Delay(1000, cancellationToken);
        throw new Exception("xxxxx");
        logger.LogInformation("[HandlerC] finish t:{Id} task at ------------------------------------ {Time}", id, DateTime.Now);
    }
}