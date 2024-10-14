using System.ComponentModel.DataAnnotations;

namespace TaskUtils.Models
{
    public class WorkerConfig : IValidatableObject
    {
        public required string HandlerName { get; set; }

        public required WorkerType WorkerType { get; set; }

        public bool IsActive { get; set; }

        public int? DelayInSeconds { get; set; }

        public int? ErrorDelayInSeconds { get; set; }

        public int IterationTimeoutInSeconds { get; init; } = 3600;

        public double? RateInSeconds { get; set; }

        public List<string>? SpecifiedHours { get; init; } = new List<string>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            switch (WorkerType)
            {
                case WorkerType.FixedDelay:
                    if (!DelayInSeconds.HasValue || DelayInSeconds.Value <= 0)
                        yield return new ValidationResult("DelayInSeconds is required and must be greater than 0 for FixedDelayWorker.", new[] { nameof(DelayInSeconds) });

                    if (!ErrorDelayInSeconds.HasValue || ErrorDelayInSeconds.Value <= 0)
                        yield return new ValidationResult("ErrorDelayInSeconds is required and must be greater than 0 for FixedDelayWorker.", new[] { nameof(ErrorDelayInSeconds) });

                    if (IterationTimeoutInSeconds <= 0)
                        yield return new ValidationResult("IterationTimeoutInSeconds is required and must be greater than 0 for FixedDelayWorker.", new[] { nameof(IterationTimeoutInSeconds) });
                    break;

                case WorkerType.FixedRate:
                    if (!RateInSeconds.HasValue || RateInSeconds.Value <= 0)
                        yield return new ValidationResult("RateInSeconds is required and must be greater than 0 for FixedRateWorker.", new[] { nameof(RateInSeconds) });
                    break;

                case WorkerType.Scheduled:
                    if (SpecifiedHours == null || SpecifiedHours.Count == 0)
                        yield return new ValidationResult("SpecifiedHours is required and must contain at least one value for ScheduledWorker.", new[] { nameof(SpecifiedHours) });
                    break;

                default:
                    yield return new ValidationResult("Invalid WorkerType specified.", new[] { nameof(WorkerType) });
                    break;
            }
        }
    }
}