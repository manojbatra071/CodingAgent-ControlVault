using System;
using System.Threading;

namespace InvoiceRiskMonitor
{
    public static class RetryPolicy
    {
        public static T Execute<T>(Func<T> operation, int retryCount, int retryDelayMs, Action<string> log)
        {
            Exception lastException = null;
            int attempts = Math.Max(1, retryCount);

            for (int attempt = 1; attempt <= attempts; attempt++)
            {
                try
                {
                    return operation();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    log($"Attempt {attempt} of {attempts} failed: {ex.Message}");

                    if (attempt < attempts)
                    {
                        Thread.Sleep(Math.Max(0, retryDelayMs));
                    }
                }
            }

            throw new InvalidOperationException("Retryable operation failed after all attempts.", lastException);
        }

        public static void Execute(Action operation, int retryCount, int retryDelayMs, Action<string> log)
        {
            Execute(
                () =>
                {
                    operation();
                    return true;
                },
                retryCount,
                retryDelayMs,
                log);
        }
    }
}
