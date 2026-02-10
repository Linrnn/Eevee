using System;

namespace Eevee.Diagnosis
{
    public sealed class ExceptionBuilder : IExceptionBuilder
    {
        public Exception Build(Type exception, string paramName, string message) => exception switch
        {
            _ when exception == typeof(ArgumentException) => new ArgumentException(message, paramName),
            _ when exception == typeof(ArgumentNullException) => new ArgumentNullException(paramName, message),
            _ when exception == typeof(ArgumentOutOfRangeException) => new ArgumentOutOfRangeException(paramName, message),
            _ when exception == typeof(IndexOutOfRangeException) => new IndexOutOfRangeException(message),
            _ when exception == typeof(InvalidOperationException) => new InvalidOperationException(message),
            _ when exception == typeof(NullReferenceException) => new NullReferenceException(message),
            _ => new Exception(message),
        };
    }
}