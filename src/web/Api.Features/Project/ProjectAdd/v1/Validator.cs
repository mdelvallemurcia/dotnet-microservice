using FluentValidation;

namespace Api.Features.Project.ProjectAdd.v1;

public class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotNull();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.CreationDate).GreaterThanOrEqualTo(new DateTime(2000,1,1));
    }
}
