
using System.Threading.Tasks;

namespace TaskUtils.TaskCompetition
{
    public class CompetitorTask(string name, Task task)
    {
        public string Name { get; } = name;
        public Task Task { get; } = task;
    }

    public class CompetitorTask<T>(string name, Task<T> task)
    {
        public string Name { get; } = name;
        public Task<T> Task { get; } = task;
    }
}