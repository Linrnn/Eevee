using System;

namespace Eevee.Diagnosis
{
    public interface IExceptionBuilder
    {
        Exception Build(Type exception, string paramName, string message);
    }
}