using EmergencyCart.Application.AccountContext.UseCases.Employees.Create;
using EmergencyCart.Domain.AccountContext.Entities;
using EmergencyCart.Domain.AccountContext.ValueObjects;
using EmergencyCart.UniTest.AccountContext;
using FluentValidation.TestHelper;
using Shouldly;

namespace EmergencyCart.Test.AccountContext.Entities;

public class CommandUserTest
{
    #region Constants
    public const string FirstNameNotEmpty = "Fistname cannot be empty";
    public const string FirstNameInvalid = "The First Name field must contain only letters and single spaces (no numbers or symbols)";
    public const string LastNameNotEmpty = "Lastname cannot be empty";
    public const string LastNameInvalid = "The Last Name field must contain only letters and single spaces (no numbers or symbols)";
    public const string EmailNotEmpty = "Email cannot be empty";
    public const string EmailInvalid = "Invalid email field. must contain more than 3 characters (ex:name@domain.com)";
    public const string PasswordNotEmpty = "Password cannot be empty";
    public const string PasswordInvalid = "Invalid password field";
    #endregion

    #region ShouldReturnSucess
    [Fact]
    public void ShouldReturnSucess()
    {
        var validate = Validate();
        var command = CommandUser.Build();

        var resultValidation = validate.Validate(command);

        resultValidation.IsValid.ShouldBeTrue();
    }

    #endregion

    #region ShouldReturnFailure
    [Theory]
    [InlineData("", "")]
    [InlineData(" ", " ")]
    [InlineData(null, null)]
    public void ShouldReturnNotEmptyFirstLastName(string? firstName, string? lastName)
    {
        var validate = Validate();
        var command = CommandUser.Build() with { firstName = firstName!, lastName = lastName! };

        var resultValidation = validate.TestValidate(command);

        resultValidation.ShouldSatisfyAllConditions(
             () => resultValidation.IsValid.ShouldBeFalse(),
             () => resultValidation.ShouldHaveValidationErrorFor(x => x.firstName)
                  .WithErrorMessage(FirstNameNotEmpty),
             () => resultValidation.ShouldHaveValidationErrorFor(x => x.lastName)
                 .WithErrorMessage(LastNameNotEmpty));
    }

    [Theory]
    [InlineData("Ronaldo2113", "Da Silva889")]
    [InlineData("Rona#@do", "992910")]
    [InlineData("!$!$!@$@!2", "!!!!!!!!!")]
    public void ShouldReturnPatternFirstLastNameInvalid(string firstName, string lastName)
    {
        var validate = Validate();
        var command = CommandUser.Build() with { firstName = firstName, lastName = lastName };

        var resultValidation = validate.TestValidate(command);

        resultValidation.ShouldSatisfyAllConditions(
             () => resultValidation.IsValid.ShouldBeFalse(),
             () => resultValidation.ShouldHaveValidationErrorFor(x => x.firstName)
                  .WithErrorMessage(FirstNameInvalid),
             () => resultValidation.ShouldHaveValidationErrorFor(x => x.lastName)
                 .WithErrorMessage(LastNameInvalid));


    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ShouldReturnNotEmptyEmail(string? email)
    {
        var validate = Validate();
        var command = CommandUser.Build() with { email = email! };

        var resultValidation = validate.TestValidate(command);

        resultValidation.ShouldSatisfyAllConditions(
             () => resultValidation.IsValid.ShouldBeFalse(),
             () => resultValidation.ShouldHaveValidationErrorFor(x => x.email)
                  .WithErrorMessage(EmailNotEmpty));
    }

    [Theory]
    [InlineData("ro@gmail.com")]
    [InlineData("ronaldogmail.com")]
    [InlineData("ronaldofenomeno@")]
    public void ShouldReturnPatternEmailInvalid(string email)
    {
        var validate = Validate();
        var command = CommandUser.Build() with { email = email };

        var resultValidation = validate.TestValidate(command);

        resultValidation.ShouldSatisfyAllConditions(
             () => resultValidation.IsValid.ShouldBeFalse(),
             () => resultValidation.ShouldHaveValidationErrorFor(x => x.email)
                  .WithErrorMessage(EmailInvalid)
        );
    }


    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ShouldReturnNotEmptyPassword(string? password)
    {
        var validate = Validate();
        var command = CommandUser.Build() with { password = password! };

        var resultValidation = validate.TestValidate(command);

        resultValidation.ShouldSatisfyAllConditions(
             () => resultValidation.IsValid.ShouldBeFalse(),
             () => resultValidation.ShouldHaveValidationErrorFor(x => x.password)
                  .WithErrorMessage(PasswordNotEmpty)
        );
    }


    [Theory]
    [InlineData("semSenha123")]
    [InlineData("senha123@")]
    [InlineData("@")]
    public void ShouldReturnPatternPasswordInvalid(string password)
    {
        var validate = Validate();
        var command = CommandUser.Build() with { password = password };

        var resultValidation = validate.TestValidate(command);

        resultValidation.ShouldSatisfyAllConditions(
             () => resultValidation.IsValid.ShouldBeFalse(),
             () => resultValidation.ShouldHaveValidationErrorFor(x => x.password)
                  .WithErrorMessage(PasswordInvalid)
        );
    }
    #endregion

    #region Factory Method
    private Validator Validate()
    {
        return new Validator();
    }
    #endregion
}
