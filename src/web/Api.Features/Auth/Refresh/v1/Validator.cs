using FluentValidation;

namespace Api.Features.Auth.Refresh.v1;

public class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}
