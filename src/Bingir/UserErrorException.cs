namespace Bingir;

public class UserErrorException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserErrorException"/> class.
    /// </summary>
    /// <param name="message">The error message to display to the user.</param>
    public UserErrorException(string message) : base(message)
    {
        // Empty.
    }
}
