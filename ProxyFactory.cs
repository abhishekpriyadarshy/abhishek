using Castle.DynamicProxy;
using System;
using System.Threading.Tasks;

public static class ProxyFactory
{
    private static readonly ProxyGenerator _generator = new ProxyGenerator();

    public static T CreateClassProxy<T>(IAsyncInterceptor interceptor, params object[] constructorArgs) where T : class
    {
        return _generator.CreateClassProxy<T>(new AsyncInterceptorAdapter(interceptor), constructorArgs);
    }

    public static T CreateInterfaceProxy<T>(T target, IAsyncInterceptor interceptor) where T : class
    {
        return _generator.CreateInterfaceProxyWithTarget<T>(target, new AsyncInterceptorAdapter(interceptor));
    }

    private class AsyncInterceptorAdapter : IInterceptor
    {
        private readonly IAsyncInterceptor _asyncInterceptor;
        public AsyncInterceptorAdapter(IAsyncInterceptor asyncInterceptor) => _asyncInterceptor = asyncInterceptor;

        public void Intercept(IInvocation invocation)
        {
            if (IsAsyncMethod(invocation.Method))
            {
                var returnType = invocation.Method.ReturnType;
                if (returnType == typeof(Task))
                {
                    _asyncInterceptor.InterceptAsynchronous(invocation);
                }
                else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var method = typeof(IAsyncInterceptor)
                        .GetMethod(nameof(IAsyncInterceptor.InterceptAsynchronous))
                        .MakeGenericMethod(returnType.GetGenericArguments()[0]);
                    method.Invoke(_asyncInterceptor, new object[] { invocation });
                }
                else
                {
                    _asyncInterceptor.InterceptSynchronous(invocation);
                }
            }
            else
            {
                _asyncInterceptor.InterceptSynchronous(invocation);
            }
        }

        private bool IsAsyncMethod(System.Reflection.MethodInfo method)
        {
            var returnType = method.ReturnType;
            return returnType == typeof(Task) ||
                   (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>));
        }
    }
}