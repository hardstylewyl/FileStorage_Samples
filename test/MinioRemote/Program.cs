using FileStorage.Extensions;
using FileStorage.MinioRemote;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFileStoreage()
	.AddCore(o =>
	{
		o.UseFreeRedisCache(r =>
		{
			r.Configuration = "localhost:6379";
		});
	})
	.AddRemoteStore(o =>
	{
		//添加minio远程存储
		o.AddMinioRemoteStorage(c =>
		{
			c.AccessKey = "identityServer_user";
			c.SecretKey = "identityServer_password";
			c.BucketName = "filestorage";
			c.Endpoint = "localhost:9000";
			c.EnableSSL = false;
		});
	});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
