using System.Linq.Expressions;
using System.Reflection;
using Csxaml.ControlMetadata;

namespace Csxaml.Runtime;

internal sealed class ExternalEventBinder
{
    private readonly EventInfo _eventInfo;
    private readonly Func<Delegate, Delegate> _handlerFactory;

    private ExternalEventBinder(
        string exposedName,
        Type handlerType,
        EventInfo eventInfo,
        Func<Delegate, Delegate> handlerFactory)
    {
        ExposedName = exposedName;
        HandlerType = handlerType;
        _eventInfo = eventInfo;
        _handlerFactory = handlerFactory;
    }

    public string ExposedName { get; }

    public Type HandlerType { get; }

    public Action Bind(object element, Delegate handler)
    {
        var delegateHandler = _handlerFactory(handler);
        _eventInfo.AddEventHandler(element, delegateHandler);
        return () => _eventInfo.RemoveEventHandler(element, delegateHandler);
    }

    public static ExternalEventBinder Create(Type controlType, EventMetadata metadata)
    {
        var eventName = metadata.ClrEventName ??
            throw new InvalidOperationException(
                $"External event '{metadata.ExposedName}' is missing a CLR event name.");
        var eventInfo = controlType.GetEvent(eventName, BindingFlags.Instance | BindingFlags.Public) ??
            throw new InvalidOperationException(
                $"External control '{controlType.FullName}' is missing event '{eventName}'.");

        var handlerType = ResolveHandlerType(metadata, eventInfo);
        return new ExternalEventBinder(
            metadata.ExposedName,
            handlerType,
            eventInfo,
            BuildHandlerFactory(eventInfo, metadata.BindingKind, handlerType));
    }

    private static Type ResolveHandlerType(EventMetadata metadata, EventInfo eventInfo)
    {
        if (metadata.BindingKind == EventBindingKind.Direct &&
            string.Equals(metadata.HandlerTypeName, typeof(Action).FullName, StringComparison.Ordinal))
        {
            return typeof(Action);
        }

        if (metadata.BindingKind == EventBindingKind.EventArgs &&
            TryGetEventArgsType(eventInfo, out var eventArgsType))
        {
            return typeof(Action<>).MakeGenericType(eventArgsType);
        }

        throw new InvalidOperationException(
            $"External event '{metadata.ExposedName}' requires unsupported binding metadata.");
    }

    private static Func<Delegate, Delegate> BuildHandlerFactory(
        EventInfo eventInfo,
        EventBindingKind bindingKind,
        Type handlerType)
    {
        var eventHandlerType = eventInfo.EventHandlerType ??
            throw new InvalidOperationException(
                $"External event '{eventInfo.Name}' does not declare a handler type.");
        var invoke = eventHandlerType.GetMethod("Invoke") ??
            throw new InvalidOperationException(
                $"External event '{eventInfo.Name}' handler type is missing Invoke().");
        if (invoke.ReturnType != typeof(void) ||
            invoke.GetParameters().Any(parameter => parameter.ParameterType.IsByRef))
        {
            throw new InvalidOperationException(
                $"External event '{eventInfo.Name}' is not compatible with Action-based binding.");
        }

        var eventParameters = invoke.GetParameters()
            .Select(parameter => Expression.Parameter(parameter.ParameterType, parameter.Name))
            .ToArray();
        var handlerParameter = Expression.Parameter(typeof(Delegate), "handler");
        var body = bindingKind == EventBindingKind.Direct
            ? BuildDirectBody(handlerParameter)
            : BuildEventArgsBody(handlerParameter, handlerType, eventParameters);
        var typedHandler = Expression.Lambda(eventHandlerType, body, eventParameters);
        var castHandler = Expression.Convert(typedHandler, typeof(Delegate));
        return Expression.Lambda<Func<Delegate, Delegate>>(castHandler, handlerParameter).Compile();
    }

    private static Expression BuildDirectBody(Expression handlerParameter)
    {
        var action = Expression.Convert(handlerParameter, typeof(Action));
        return Expression.Call(action, typeof(Action).GetMethod(nameof(Action.Invoke))!);
    }

    private static Expression BuildEventArgsBody(
        Expression handlerParameter,
        Type handlerType,
        IReadOnlyList<ParameterExpression> eventParameters)
    {
        var action = Expression.Convert(handlerParameter, handlerType);
        var invoke = handlerType.GetMethod(nameof(Action.Invoke))!;
        return Expression.Call(action, invoke, eventParameters[1]);
    }

    private static bool TryGetEventArgsType(EventInfo eventInfo, out Type eventArgsType)
    {
        var parameters = eventInfo.EventHandlerType
            ?.GetMethod("Invoke")
            ?.GetParameters();
        if (parameters is { Length: 2 })
        {
            eventArgsType = parameters[1].ParameterType;
            return true;
        }

        eventArgsType = null!;
        return false;
    }
}
