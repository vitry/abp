using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Internals;

namespace Volo.Abp.EasyNetQ;

using NonGenericSubscribeDelegate = Func<IPubSub, string, Type, Func<object, Type, CancellationToken, Task>, Action<ISubscriptionConfiguration>, CancellationToken, AwaitableDisposable<ISubscriptionResult>>;

public static class NonGenericPubSubExtensions
{
    private static readonly ConcurrentDictionary<Type, NonGenericSubscribeDelegate> SubscribeDelegates = new();

    //public static Task PublishAsync
    public static AwaitableDisposable<ISubscriptionResult> SubscribeAsync(
        this IPubSub pubSub,
        string subscriptionId,
        Type messageType,
        Func<object, Type, CancellationToken, Task> onMessage,
        Action<ISubscriptionConfiguration> configuration,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(pubSub, nameof(pubSub));

        var subscribeDelegate = SubscribeDelegates.GetOrAdd(messageType, t =>
        {
            var subscribeMethodInfo = typeof(IPubSub).GetMethod("SubscribeAsync");
            if (subscribeMethodInfo == null)
                throw new MissingMethodException(nameof(IPubSub), "SubscribeAsync");

            var genericSubscribeMethodInfo = subscribeMethodInfo.MakeGenericMethod(t);
            var pubSubParameter = Expression.Parameter(typeof(IPubSub), "pubSub");
            var subscriptionIdParameter = Expression.Parameter(typeof(string), "subscriptionId");
            var messageTypeParameter = Expression.Parameter(typeof(Type), "messageType");
            var messageParameter = Expression.Parameter(t, "message");
            var onMessageParameter = Expression.Parameter(typeof(Func<object, Type, CancellationToken, Task>), "onMessage");
            var configureParameter = Expression.Parameter(typeof(Action<ISubscriptionConfiguration>), "configure");
            var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
            var onMessageInvocationExpression = Expression.Lambda(
                Expression.GetFuncType(t, typeof(CancellationToken), typeof(Task)),
                Expression.Invoke(
                    onMessageParameter,
                    Expression.Convert(messageParameter, typeof(object)),
                    Expression.Call(Expression.Convert(messageParameter, typeof(object)), typeof(object).GetMethod("GetType", Array.Empty<Type>()) ?? throw new InvalidOperationException()),
                    cancellationTokenParameter
                ),
                messageParameter,
                cancellationTokenParameter);

            // required method
            var lambda = Expression.Lambda<NonGenericSubscribeDelegate>(
                Expression.Call(
                    pubSubParameter,
                    genericSubscribeMethodInfo,
                    subscriptionIdParameter,
                    onMessageInvocationExpression,
                    configureParameter,
                    cancellationTokenParameter
                ),
                pubSubParameter,
                subscriptionIdParameter,
                messageTypeParameter,
                onMessageParameter,
                configureParameter,
                cancellationTokenParameter
            );
            return lambda.Compile();
        });

        return subscribeDelegate(pubSub, subscriptionId, messageType, onMessage, configuration, cancellationToken);
    }
}