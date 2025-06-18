using System.Threading.Tasks;

public class UserService : IUserService
{
    public async Task<string> GetUserAsync(string userId)
    {
        await Task.Delay(100);
        return $"User: {userId}";
    }

    public string SayHello(string name) => $"Hello, {name}";
}