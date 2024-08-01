using FileStorage.Api;
using FileStorage.Extensions;
using FileStorage.MinioRemote;
using FileStorage.NativeLocal;
using FileStorage.TusLocal;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
	.AddJsonOptions(c =>
	{
		//大驼峰命名
		c.JsonSerializerOptions.PropertyNamingPolicy = JsonPascalCaseNamingPolicy.Instance;
	});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o =>
{
	o.AddDefaultPolicy(p =>
	{
		p.WithOrigins(["https://localhost:3000"])
			//搭配tus-js-client.js库所需要头信息 否则cors访问不到响应header信息
			.WithExposedHeaders("Location", "Upload-Offset", "Upload-Length")
			.SetIsOriginAllowedToAllowWildcardSubdomains()
			.AllowCredentials()
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});

builder.Services.AddDbContext<AppDbContext>(o =>
{
	o.UseFileStorage();
	o.UseSqlite("Data Source = ../../FileTemp/filestorage.db");
});

builder.Services.AddFileStoreage()
	.AddCore(b =>
	{
		b.UseDbContext<AppDbContext>();

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
app.UseCors();

app.UseAuthorization();

app.MapControllers();

//app.MapTusStorageForRedis("/TusStorage");

app.Run();
