Introduction
As you might suspect, logging what is happening in your application is good practice. In a gRPC client it is possible to automatically add logs to your application each time a gRPC endpoint is invoked without polluting your gRPC client. To do this, we will use a gRPC Interceptor to automatically log information on each call. As a reminder, Interceptors are a gRPC concept that allows apps to interact with incoming or outgoing gRPC calls. They offer a way to enrich the request processing pipeline.

Create an Interceptor to intercept calls client-side
To create a custom Interceptor, you will have to use the Interceptor class and inherit from it. To implement your logging, for each type of gRPC service, you will need to override a specific interception method, here they are:

AsyncClientStreamingCall for client streaming calls
AsyncDuplexStreamingCall for duplex (client and server) streaming calls
AsyncServerStreamingCall for server streaming calls
AsyncUnaryCall for unary calls
By adding logging (whatever you want), you can for example add logging execution date or the machine name that is performing the call, display request messages etc etc…

using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace CountryWiki.Web.Interceptors
{
    public class TracerInterceptor : Interceptor
    {
        private readonly ILogger<TracerInterceptor> _logger;

        public TracerInterceptor(ILogger<TracerInterceptor> logger)
        {
            _logger = logger;
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context, 
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
            where TRequest : class
            where TResponse : class
        {
            _logger.LogDebug($"Calling {context.Method.Name} {context.Method.Type} method at {DateTime.UtcNow} UTC from machine {Environment.MachineName}");
            var continuated = continuation(context);

            return continuated;
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context, 
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
            where TRequest : class
            where TResponse : class
        {
            _logger.LogDebug($"Calling {context.Method.Name} {context.Method.Type} method at {DateTime.UtcNow} UTC from machine {Environment.MachineName}");
            var continuated = continuation(context);

            return continuated;
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            TRequest request, 
            ClientInterceptorContext<TRequest, TResponse> context, 
            AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
            where TRequest : class
            where TResponse : class
        {
            _logger.LogDebug($"Calling {context.Method.Name} {context.Method.Type} method. Payload received: {request.GetType()} : {request}");
            _logger.LogDebug($"Calling {context.Method.Name} {context.Method.Type} method at {DateTime.UtcNow} UTC from machine {Environment.MachineName}");
            var continuated = continuation(request, context);

            return continuated;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request, 
            ClientInterceptorContext<TRequest, TResponse> context, 
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
            where TRequest : class
            where TResponse : class
        {
            _logger.LogDebug($"Calling {context.Method.Name} {context.Method.Type} method. Payload received: {request.GetType()} : {request}");
            _logger.LogDebug($"Calling {context.Method.Name} {context.Method.Type} method at {DateTime.UtcNow} UTC from machine {Environment.MachineName}");
            var continuated = continuation(request, context);

            return continuated;
        }
    }
}
view rawTracerInterceptor.cs hosted with ❤ by GitHub
Configure the Interceptor
As you also know, magic does not exist. In this section I will show you how to attach your Interceptor to the gRPC client. First of all, what you need to know is that an Interceptor does not register in the .NET 6 dependency injection system, just like the ILogger interface passed to it. We will have to use the LoggerFactory factory to create the logger and inject it ourselves into the constructor of the Interceptor. Then we will attach everything to the gRPC typed client as follows:

var builder = WebApplication.CreateBuilder(args);

var loggerFactory = LoggerFactory.Create(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Trace);
});

builder.Services.AddGrpcClient<CountryServiceClient>(o =>
    {
        o.Address = new Uri("{gRpcServerBaseUri}");
    })
    .AddInterceptor(() => new TracerInterceptor(loggerFactory.CreateLogger<TracerInterceptor>()));

var app = builder.Build();

...

app.Run();
view rawProgram.cs hosted with ❤ by GitHub
Demo
Based on the logs you have added on your custom Interceptor, it should give something like this:


