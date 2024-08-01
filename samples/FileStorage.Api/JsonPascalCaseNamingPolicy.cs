using System.Text.Json;

namespace FileStorage.Api;

//大驼峰
public class JsonPascalCaseNamingPolicy : JsonNamingPolicy
{
	public static readonly JsonPascalCaseNamingPolicy Instance = new();

	public override string ConvertName(string name1)
	{
		return string.IsNullOrEmpty(name1) || !char.IsUpper(name1[0])
			? name1
			: string.Create<string>(name1.Length, name1, ((chars, name2) =>
			{
				name2.CopyTo(chars);
				FixCasing(chars);
			}));
	}

	private static void FixCasing(Span<char> chars)
	{
		for (int index = 0; index < chars.Length && (index != 1 || char.IsUpper(chars[index])); ++index)
		{
			bool flag = index + 1 < chars.Length;
			if (index > 0 & flag && !char.IsUpper(chars[index + 1]))
			{
				if (chars[index + 1] != ' ')
					break;
				chars[index] = char.ToUpperInvariant(chars[index]);
				break;
			}

			chars[index] = char.ToUpperInvariant(chars[index]);
		}
	}
}
