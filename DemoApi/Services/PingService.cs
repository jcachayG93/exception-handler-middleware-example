namespace DemoApi.Services;

/// <summary>
/// Simulates a real application where an unhandled exception can be thrown anywhere.
/// A service that can either return Pong or throw an exception with a custom message.
/// </summary>
public class PingService 
{
    private string? _throwErrorMessage = null;
    
    public void SetupToThrowException(string message)
    {
        _throwErrorMessage = message;
    }
    
    public string Ping()
    {
        if (_throwErrorMessage is not null)
        {
            throw new InvalidOperationException(_throwErrorMessage);
        }
        return "Pong!";
    }
}