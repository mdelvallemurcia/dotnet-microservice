using FluentValidation;

namespace Api.Features.Auth.Login.v1;

public class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(x => x.UserName).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
