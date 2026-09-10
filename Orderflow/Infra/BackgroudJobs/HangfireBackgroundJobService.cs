using System.Linq.Expressions;
using APP.Common.Interfaces;
using Hangfire;

namespace Infra.BackgroudJobs;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    public void Enqueue<T>(Expression<Action<T>> methodCall)
    {
        BackgroundJob.Enqueue(methodCall);
    }

    public void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
    {
        BackgroundJob.Schedule(methodCall, delay);
    }
}
