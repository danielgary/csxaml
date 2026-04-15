using System.Reflection;
using System.Runtime.InteropServices;
using Csxaml.Runtime;
using Microsoft.UI.Xaml;

namespace Csxaml.Runtime.Tests;

internal static class WinUiTestEnvironment
{
    public static void Run(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception) when (GetRelevantException(exception) is COMException)
        {
            Assert.Inconclusive(
                $"WinUI controls are unavailable in this test environment: {GetRelevantException(exception).Message}");
        }
    }

    public static void RunInWindow(Action<Window> action)
    {
        Run(() =>
        {
            var window = new Window();
            try
            {
                action(window);
            }
            finally
            {
                window.Content = null;
                window.Close();
            }
        });
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
