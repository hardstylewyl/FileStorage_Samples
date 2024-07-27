namespace FileStorage.NativeLocal.Models;

public sealed record NativeCheckFileResult(bool NotExist, List<int>? MissChunkList);
