using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IPInfo
{ 
    public class Result
    {
        public static Result<TResultType> Success<TResultType>(TResultType value)
        {
            return new Result<TResultType>(true, value, null);
        }

        public static Result<TResultType> Failure<TResultType>(Error error)
        {
            return new Result<TResultType>(false, default(TResultType), error);
        }

        public static Result<TResultType> Of<TResultType>(Func<TResultType> operation)
        {
            try
            {
                var value = operation();
                return Success<TResultType>(value);
            }
            catch (Exception e)
            {
                return Failure<TResultType>(Error.Exception(e.Message));
            }
        }

        public static Result<TResultType> Of<TResultType>(Func<Result<TResultType>> operation)
        {
            try
            {
                return operation();
            }
            catch (Exception e)
            {
                return Failure<TResultType>(Error.Exception(e.Message));
            }
        }

        public async static Task<Result<TResultType>> Of<TResultType>(Func<Task<TResultType>> operation)
        {
            try
            {
                var value = await operation();
                return Success<TResultType>(value);
            }
            catch (Exception e)
            {
                return Failure<TResultType>(Error.Exception(e.Message));
            }
        }

        public async static Task<Result<TResultType>> Of<TResultType>(Func<Task<Result<TResultType>>> operation)
        {
            try
            {
                return await operation();
            }
            catch (Exception e)
            {
                return Failure<TResultType>(Error.Exception(e.Message));
            }
        }
    }

    public class Result<TResultType>
    {
        public Result(bool isSuccess, TResultType value, Error error)
        { 
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public bool IsSuccess { get; private set;}

        public TResultType Value { get; private set; }

        public Error Error { get; private set;}
    }

    public class Error
    {
        public const string EXCEPTION = "500";

        public const string BAD_REQUEST = "400";

        public const string SERVICE_UNAVAILABLE = "503";

        public const string NOT_FOUND = "404";

        #region static utilities

        public static Error Exception(string message) => new Error(EXCEPTION, message);

        public static Error BadRequest(string message = null) => new Error(BAD_REQUEST, message);

        public static Error Unavailable(string message) => new Error(SERVICE_UNAVAILABLE, message);

        public static Error NotFound(string message) => new Error(NOT_FOUND, message);

        #endregion

        public Error(string code, string message)
        { 
            Code = code;
            Message = message;
        }

        public string Code { get; private set; }

        public string Message { get; private set; }
    }
}
