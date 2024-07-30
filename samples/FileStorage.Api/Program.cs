using FileStorage.Api;
using FileStorage.Extensions;
using FileStorage.MinioRemote;
using FileStorage.NativeLocal;
using FileStorage.TusLocal;
using FileStorage.TusLocal.UrlStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFileStoreage()
	.AddCore(b =>
	{
		b.UseFreeRedisCache(o =>
		{
			o.Configuration = "localhost:6379";
		});
	})
	.AddLocalStore(b =>
	{
		b.AddTusLocalStorage(o =>
		{
			o.MetaDirectoryPath = "../../FileTemp/TusData_Meta";
			o.DirectoryPath = "../../FileTemp/TusData_Temp";
		});

		b.AddNativeLocalStorage(o =>
		{
			o.ConfigDirectoryPath = "../../FileTemp/NativeData_Meta";
			o.TempDirectoryPath = "../../FileTemp/NativeData_Temp";
		});
	})
	.AddRemoteStore(b =>
	{
		b.AddMinioRemoteStorage(o =>
		{
			o.AccessKey = "identityServer_user";
			o.SecretKey = "identityServer_password";
			o.BucketName = "filestorage";
			o.Endpoint = "localhost:9000";
			o.EnableSSL = false;
		});
	});

//添加调度服务
builder.Services.AddSingleton<UploadToMinioJobService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapTusStorageForRedis("/TusStorage");

app.Run();
