namespace UndefinedBot.Core.Utils;

public static class TaskExtensions
{
    public static async Task<T?> InterruptAfter<T>(this Task<T> task, TimeSpan timeout, Action? callbackSuccess = null,
        Action? callbackTimeout = null) where T : notnull
    {
        CancellationTokenSource cts = new(timeout);
        try
        {
            T result = await task.WaitAsync(timeout, cts.Token);
            callbackSuccess?.Invoke();
            cts.Dispose();
            return result;
        }
        catch (OperationCanceledException)
        {
            await cts.CancelAsync();
            callbackTimeout?.Invoke();
            cts.Dispose();
            return default;
        }
        finally
        {
            cts.Dispose();
        }
    }

    public static async Task InterruptAfter(this Task task, TimeSpan timeout, Action? callbackSuccess = null,
        Action? callbackTimeout = null)
    {
        CancellationTokenSource cts = new();
        try
        {
            await task.WaitAsync(timeout, cts.Token);
            callbackSuccess?.Invoke();
        }
        catch
        {
            await cts.CancelAsync();
            callbackTimeout?.Invoke();
        }
        finally
        {
            cts.Dispose();
        }
    }
}