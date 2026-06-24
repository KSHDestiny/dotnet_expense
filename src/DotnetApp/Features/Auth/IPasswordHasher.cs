namespace DotnetApp.Features.Auth;

/// <summary>
/// Hashing abstraction so the service layer doesn't depend on a concrete crypto
/// implementation. Backed by ASP.NET Core's PBKDF2 hasher (see registration).
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);
}
