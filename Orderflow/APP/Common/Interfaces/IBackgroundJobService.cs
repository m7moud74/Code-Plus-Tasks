using System.Linq.Expressions;

namespace APP.Common.Interfaces;

public interface IBackgroundJobService
{
    void Enqueue<T>(Expression<Action<T>> methodCall);
    void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);
}
