namespace TaskUtils.Workers;

public interface IHandler
{
    Task ProcessAsync(CancellationToken cancellationToken);
}