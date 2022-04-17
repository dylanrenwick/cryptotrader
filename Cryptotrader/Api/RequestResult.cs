namespace Cryptotrader.Api
{
    public class RequestResult<T>
    {
        public T Result { get; private set; }

        public string ErrorMessage { get; private set; }

        public bool IsSuccess { get; private set; }

        public RequestResult<Y> CastTo<Y>(Func<T, Y> convertFunc)
        {
            if (IsSuccess)
            {
                return RequestResult<Y>.Success(convertFunc(Result));
            }
            else
            {
                return RequestResult<Y>.Failure(ErrorMessage);
            }
        }

        public T Unwrap() => Unwrap(new ResultException(ErrorMessage));

        public T Unwrap(Exception ex)
        {
            if (IsSuccess) return Result;
            throw ex;
        }

        public static RequestResult<T> Success(T t)
        {
            return new() {
                Result = t,
                IsSuccess = true,
                ErrorMessage = string.Empty
            };
        }
        public static RequestResult<T> Failure(string message)
        {
            return new() {
                Result = default(T),
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
