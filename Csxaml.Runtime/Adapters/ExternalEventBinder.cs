using System.Linq.Expressions;
using System.Reflection;
using Csxaml.ControlMetadata;

namespace Csxaml.Runtime;

internal sealed class ExternalEventBinder
{
    private readonly EventInfo _eventInfo;
    private readonly Func<Action, Delegate> _handlerFactory;

    private ExternalEventBinder(
        string exposedName,
        EventInfo eventInfo,
        Func<Action, Delegate> handlerFactory)
    {
        ExposedName = exposedName;
        _eventInfo = eventInfo;
        _handlerFactory = handlerFactory;
    }

    public string ExposedName { get; }

    public Action Bind(object element, Action handler)
    {
        var delegateHandler = _handlerFactory(handler);
        _eventInfo.AddEventHandler(element, delegateHandler);
        return () => _eventInfo.RemoveEventHandler(element, delegateHandler);
    }

    public static ExternalEventBinder Create(Type controlType, EventMetadata metadata)
    {
        if (metadata.BindingKind != EventBindingKind.Direct ||
            !string.Equals(metadata.HandlerTypeName, typeof(Action).FullName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"External event '{metadata.ExposedName}' requires unsupported binding metadata.");
        }

        var eventName = metadata.ClrEventName ??
            throw new InvalidOperationException(
                $"External event '{metadata.ExposedName}' is missing a CLR event name.");
        var eventInfo = controlType.GetEvent(eventName, BindingFlags.Instance | BindingFlags.Public) ??
            throw new InvalidOperationException(
                $"External control '{controlType.FullName}' is missing event '{eventName}'.");
        return new ExternalEventBinder(
            metadata.ExposedName,
            eventInfo,
            BuildHandlerFactory(eventInfo));
    }

    private static Func<Action, Delegate> BuildHandlerFactory(EventInfo eventInfo)
    {
        var handlerType = eventInfo.EventHandlerType ??
            throw new InvalidOperationException(
                $"External event '{eventInfo.Name}' does not declare a handler type.");
        var invoke = handlerType.GetMethod("Invoke") ??
            throw new InvalidOperationException(
                $"External event '{eventInfo.Name}' handler type is missing Invoke().");
        if (invoke.ReturnType != typeof(void) ||
            invoke.GetParameters().Any(parameter => parameter.ParameterType.IsByRef))
        {
            throw new InvalidOperationException(
                $"External event '{eventInfo.Name}' is not compatible with Action-based binding.");
        }

        var actionParameter = Expression.Parameter(typeof(Action), "action");
        var eventParameters = invoke.GetParameters()
            .Select(parameter => Expression.Parameter(parameter.ParameterType, parameter.Name))
            .ToArray();
        var body = Expression.Call(actionParameter, typeof(Action).GetMethod(nameof(Action.Invoke))!);
        var typedHandler = Expression.Lambda(handlerType, body, eventParameters);
        var castHandler = Expression.Convert(typedHandler, typeof(Delegate));
        return Expression.Lambda<Func<Action, Delegate>>(castHandler, actionParameter).Compile();
    }
}
