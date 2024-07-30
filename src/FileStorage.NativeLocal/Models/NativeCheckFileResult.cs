namespace FileStorage.NativeLocal.Models;

public sealed record NativeCheckFileResult(bool NotExist, bool InProgress, List<int>? MissChunkList);
