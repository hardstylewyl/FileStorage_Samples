namespace FileStorage.Models;

public sealed record Error(string Code, string Message)
{
	public static readonly Error None = new(string.Empty, string.Empty);
	public static readonly Error NullValue = new("Error.NullValue", "Null value was provided");

	public static implicit operator Result(Error error)
	{
		return Result.Failure(error);
	}
}