using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Internals;

namespace Volo.Abp.EventBus.EasyNetQ.Volo.Abp.EventBus.EasyNetQ;

//using NonGenericPublishDelegate = Func<IPubSub, object, Type, Action<IPublishConfiguration>, CancellationToken, Task>;
using NonGenericSubscribeDelegate = Func<IPubSub, string, Type, Func<object, Type, CancellationToken, Task>, Action<ISubscriptionConfiguration>, CancellationToken, AwaitableDisposable<SubscriptionResult>>;

public static class NonGenericPubSubExtensions
{
    //private static readonly ConcurrentDictionary<Type, NonGenericPublishDelegate> PublishDelegates = new ();
    private static readonly ConcurrentDictionary<Type, NonGenericSubscribeDelegate> SubscribeDelegates = new();

    //public static Task PublishAsync(
    //    this IPubSub pubSub, 
    //    object message, 
    //    Type messageType, 
    //    Action<IPublishConfiguration> configure, 
    //    CancellationToken cancellationToken = default)
    //{
    //    Check.NotNull(pubSub, nameof(pubSub));

    //    var publishDelegate = PublishDelegates.GetOrAdd(messageType, t =>
    //    {
    //        var publishMethodInfo = typeof(IPubSub).GetMethod("PublishAsync");
    //        if (publishMethodInfo == null)
    //            throw new MissingMethodException(nameof(IPubSub), "PublishAsync");

    //        var genericPublishMethodInfo = publishMethodInfo.MakeGenericMethod(t);
    //        ParameterExpression pubSubParameter = Expression.Parameter(typeof(IPubSub), "pubSub");
    //        ParameterExpression messageParameter = Expression.Parameter(typeof(object), "message");
    //        ParameterExpression messageTypeParameter = Expression.Parameter(typeof(Type), "messageType");
    //        ParameterExpression configureParameter = Expression.Parameter(typeof(Action<IPublishConfiguration>), "configure");
    //        ParameterExpression cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
    //        var genericPublishMethodCallExpression = Expression.Call(
    //            pubSubParameter,
    //            genericPublishMethodInfo,
    //            Expression.Convert(messageParameter, t),
    //            configureParameter,
    //            cancellationTokenParameter);
    //        var lambda = Expression.Lambda<NonGenericPublishDelegate>(
    //            genericPublishMethodCallExpression,
    //            pubSubParameter,
    //            messageParameter,
    //            messageTypeParameter,
    //            configureParameter,
    //            cancellationTokenParameter);
    //        return lambda.Compile();
    //    });
    //    return publishDelegate(pubSub, message, messageType, configure, cancellationToken);
    //}

    // todo-kai-test: remove messagetype parameter | use messagetype directly
    public static AwaitableDisposable<SubscriptionResult> SubscribeAsync(
        this IPubSub pubSub,
        string subscriptionId,
        Type messageType,
        Func<object, Type, CancellationToken, Task> onMessage,
        Action<ISubscriptionConfiguration> configuration,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(pubSub, nameof(pubSub));

        NonGenericSubscribeDelegate subscribeDelegate = SubscribeDelegates.GetOrAdd(messageType, t =>
        {
            var subscribeMethodInfo = typeof(IPubSub).GetMethod("SubscribeAsync");
            if (subscribeMethodInfo == null)
                throw new MissingMethodException(nameof(IPubSub), "SubscribeAsync");

            var genericSubscribeMethodInfo = subscribeMethodInfo.MakeGenericMethod(t);
            ParameterExpression pubSubParameter = Expression.Parameter(typeof(IPubSub), "pubSub");
            ParameterExpression subscriptionIdParameter = Expression.Parameter(typeof(string), "subscriptionId");
            ParameterExpression messageTypeParameter = Expression.Parameter(typeof(Type), "messageType");
            ParameterExpression messageParameter = Expression.Parameter(t, "message");
            ParameterExpression onMessageParameter = Expression.Parameter(typeof(Func<object, Type, CancellationToken, Task>), "onMessage");
            ParameterExpression configureParameter = Expression.Parameter(typeof(Action<ISubscriptionConfiguration>), "configure");
            ParameterExpression cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
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