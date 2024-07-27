namespace FileStorage.Models;

public class Result
{
	protected Result(bool isSuccess, Error error)
	{
		if (isSuccess && error != Error.None ||
			!isSuccess && error == Error.None)
			throw new ArgumentException("Invalid error", nameof(error));

		IsSuccess = isSuccess;
		Error = error;
	}

	public bool IsSuccess { get; }
	public bool IsFailure => !IsSuccess;
	public Error Error { get; }

	public static Result<TValue> Create<TValue>(TValue? value, Error error)
		where TValue : class
	{
		return value == null ? Failure<TValue>(error) : Success(value);
	}

	public static Result Success()
	{
		return new Result(true, Error.None);
	}

	public static Result<TValue> Success<TValue>(TValue value)
	{
		return new Result<TValue>(value, true, Error.None);
	}

	public static Result<TValue> From<TValue>(TValue value)
	{
		return Success(value);
	}

	public static Result Failure(Error error)
	{
		return new Result(false, error);
	}

	public static Result<TValue> Failure<TValue>(Error error)
	{
		return new Result<TValue>(default, false, error);
	}

	//用于处理多个Result的返回结果，只要有一个失败就返回这个失败了
	public static Result FirstFailureOrSuccess(params Result[] results)
	{
		foreach (var result in results)
			if (result.IsFailure)
				return result;

		return Success();
	}
}