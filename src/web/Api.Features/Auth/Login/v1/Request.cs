namespace Api.Features.Auth.Login.v1;

public class Request
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
}
