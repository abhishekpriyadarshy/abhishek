using Castle.DynamicProxy;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

public class MyAsyncInterceptor : IAsyncInterceptor
{
    public void InterceptSynchronous(IInvocation invocation)
    {
        var logMessage = GetLogMessage(invocation);

        Console.WriteLine($"[Sync] {logMessage}");

        try
        {
            invocation.Proceed();
            Console.WriteLine($"[Sync] Success: {logMessage} Result: {invocation.ReturnValue}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Sync] Error: {logMessage} Exception: {ex.Message}");
            throw;
        }
    }

    public void InterceptAsynchronous(IInvocation invocation)
    {
        var logMessage = GetLogMessage(invocation);

        Console.WriteLine($"[Async] {logMessage}");

        try
        {
            invocation.Proceed();
            var task = (Task)invocation.ReturnValue;
            invocation.ReturnValue = InterceptAsync(task, logMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Async] Error: {logMessage} Exception: {ex.Message}");
            throw;
        }
    }

    public void InterceptAsynchronous<TResult>(IInvocation invocation)
    {
        var logMessage = GetLogMessage(invocation);

        Console.WriteLine($"[Async<TResult>] {logMessage}");

        try
        {
            invocation.Proceed();
            var task = (Task<TResult>)invocation.ReturnValue;
            invocation.ReturnValue = InterceptAsync(task, logMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Async<TResult>] Error: {logMessage} Exception: {ex.Message}");
            throw;
        }
    }

    private async Task InterceptAsync(Task task, string logMessage)
    {
        try
        {
            await task.ConfigureAwait(false);
            Console.WriteLine($"[Async] Success: {logMessage}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Async] Error: {logMessage} Exception: {ex.Message}");
            throw;
        }
    }

    private async Task<TResult> InterceptAsync<TResult>(Task<TResult> task, string logMessage)
    {
        try
        {
            var result = await task.ConfigureAwait(false);
            Console.WriteLine($"[Async<TResult>] Success: {logMessage} Result: {result}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Async<TResult>] Error: {logMessage} Exception: {ex.Message}");
            throw;
        }
    }

    private string GetLogMessage(IInvocation invocation)
    {
        var method = invocation.MethodInvocationTarget ?? invocation.Method;
        var attr = method.GetCustomAttribute<LogMessageAttribute>();
        if (attr != null)
        {
            string paramValue = null;
            if (!string.IsNullOrEmpty(attr.ParameterName))
            {
                var paramIndex = Array.FindIndex(method.GetParameters(), p => p.Name == attr.ParameterName);
                if (paramIndex >= 0)
                {
                    paramValue = invocation.Arguments[paramIndex]?.ToString();
                }
            }
            return $"{attr.Message}" + (paramValue != null ? $" [Parameter: {attr.ParameterName}={paramValue}]" : "");
        }
        return $"Method {method.Name} called";
    }
}