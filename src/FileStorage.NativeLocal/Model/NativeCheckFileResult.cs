namespace FileStorage.NativeLocal.Model;

public sealed record NativeCheckFileResult(bool NotExist, List<int>? MissChunkList);
