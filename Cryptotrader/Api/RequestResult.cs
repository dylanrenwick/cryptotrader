namespace Cryptotrader.Api
{
    public class RequestResult<T>
    {
        public T Result { get; private set; }

        public string ErrorMessage { get; private set; }

        public bool IsSuccess { get; private set; }

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
}
