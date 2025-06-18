using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var service = new UserService();
        var proxy = ProxyFactory.CreateInterfaceProxy<IUserService>(service, new MyAsyncInterceptor());

        await proxy.GetUserAsync("abc"); // Logs with attribute message and userId
        proxy.SayHello("world");         // Logs with attribute message and name
    }
}