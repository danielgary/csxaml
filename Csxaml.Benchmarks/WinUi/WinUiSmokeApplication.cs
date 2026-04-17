using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Csxaml.Benchmarks;

internal sealed class WinUiSmokeApplication : Application
{
    private static TaskCompletionSource<WinUiSmokeReport>? _completionSource;
    private static int _iterations;

    public static WinUiSmokeReport Run(int iterations)
    {
        try
        {
            _iterations = iterations;
            _completionSource = new TaskCompletionSource<WinUiSmokeReport>();
            WinRT.ComWrappersSupport.InitializeComWrappers();
            Start(_ =>
            {
                var synchronizationContext =
                    new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                new WinUiSmokeApplication();
            });
            return _completionSource.Task.GetAwaiter().GetResult();
        }
        catch (Exception exception) when (GetRelevantException(exception) is COMException relevant)
        {
            return WinUiSmokeReport.CreateUnavailable(
                iterations,
                $"WinUI controls are unavailable in this environment: {relevant.Message}");
        }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var window = new Window();
        try
        {
            var runner = new WinUiSmokeScenarioRunner(window, _iterations);
            var report = runner.Run();
            _completionSource?.SetResult(report);
        }
        catch (Exception exception)
        {
            _completionSource?.SetException(exception);
        }
        finally
        {
            window.Content = null;
            window.Close();
            Exit();
        }
    }

    private static Exception GetRelevantException(Exception exception)
    {
        return exception switch
        {
            CsxamlRuntimeException runtimeException when runtimeException.InnerException is not null =>
                GetRelevantException(runtimeException.InnerException),
            TargetInvocationException invocationException when invocationException.InnerException is not null =>
                GetRelevantException(invocationException.InnerException),
            TypeInitializationException initializationException when initializationException.InnerException is not null =>
                GetRelevantException(initializationException.InnerException),
            _ => exception
        };
    }
}
