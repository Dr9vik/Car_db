namespace Business_Logic_Layer.Common.Models.ModelExceptions
{
    public class ValidationME
    {
    }

    public class ValidationME2<T>: ValidationME
    {
        public T Model { get; set; }
        public ValidationME2(T model)
        {
            Model = model;
        }
    }
}
