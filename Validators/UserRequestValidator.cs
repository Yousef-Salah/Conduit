using FluentValidation;

namespace Conduit.Validators
{
    public class UserRequestValidator : AbstractValidator<UserRequest>
    {
        public UserRequestValidator()
        {
            RuleFor(user => user.Username).NotEmpty();
            RuleFor(user => user.Username).NotEmpty();
            RuleFor(user => user.Email).NotEmpty().EmailAddress();
            RuleFor(user => user.Password).Length(8, 22);
        }
    }
}
