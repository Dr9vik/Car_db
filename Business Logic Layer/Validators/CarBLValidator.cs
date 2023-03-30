using Business_Logic_Layer.Common.Models;
using FluentValidation;

namespace Business_Logic_Layer.Validators
{
    public class CarBLCreateValidator : AbstractValidator<CarBLCreate>
    {
        public CarBLCreateValidator()
        {
            RuleFor(x => x.Name).NotNull().Length(1, 50).WithMessage("Количество символов от 1 до 50");
        }
    }
    public class CarBLUpdateValidator : CarBLCreateValidator
    {
    }
}
