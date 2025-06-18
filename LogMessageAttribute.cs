using System;

[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class LogMessageAttribute : Attribute
{
    public string Message { get; }
    public string ParameterName { get; }

    public LogMessageAttribute(string message, string parameterName = null)
    {
        Message = message;
        ParameterName = parameterName;
    }
}