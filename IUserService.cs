using System.Threading.Tasks;

public interface IUserService
{
    [LogMessage("Getting user", "userId")]
    Task<string> GetUserAsync(string userId);

    [LogMessage("Synchronous hello", "name")]
    string SayHello(string name);
}