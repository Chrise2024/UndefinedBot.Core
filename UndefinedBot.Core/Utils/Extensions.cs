namespace UndefinedBot.Core.Utils;

public static class TaskExtensions
{
    public static async Task<T?> InterruptAfter<T>(this Task<T> task, TimeSpan timeout, Action? callbackSuccess = null,
        Action? callbackTimeout = null)
    {
        CancellationTokenSource cts = new();
        try
        {
            T result = await Task.Run(async () => await task, cts.Token).WaitAsync(timeout, cts.Token);
            callbackSuccess?.Invoke();
            return result;
        }
        catch (OperationCanceledException)
        {
            await cts.CancelAsync();
            callbackTimeout?.Invoke();
            return default(T);
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
            await Task.Run(async () => await task, cts.Token).WaitAsync(timeout, cts.Token);
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