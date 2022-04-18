namespace Cryptotrader.Api
{
    public class Result<T>
    {
        public T Value { get; private set; }

        public string ErrorMessage { get; private set; }

        public bool IsSuccess { get; private set; }

        public Result<Y> CastTo<Y>(Func<T, Y> convertFunc)
        {
            if (IsSuccess)
            {
                return Result<Y>.Success(convertFunc(Value));
            }
            else
            {
                return Result<Y>.Failure(ErrorMessage);
            }
        }

        public T Unwrap() => Unwrap(new ResultException(ErrorMessage));

        public T Unwrap(Exception ex)
        {
            if (IsSuccess) return Value;
            throw ex;
        }

        public static Result<T> Success(T t)
        {
            return new() {
                Value = t,
                IsSuccess = true,
                ErrorMessage = string.Empty
            };
        }
        public static Result<T> Failure(string message)
        {
            return new() {
                Value = default(T),
                IsSuccess = false,
                ErrorMessage = message
            };
        }
    }

    public class ResultException : Exception
    {
        public ResultException(): base() { }
        public ResultException(string message) : base(message) { }
        public ResultException(string message, Exception inner) : base(message, inner) { }
    }
}
