using FileStorage.Extensions;
using FileStorage.TusLocal;
using FileStorage.TusLocal.UrlStorage;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTusLocalStorage(o =>
{
	o.MetaDirectoryPath = "../../TusLocal/meta/";
	o.DirectoryPath = "../../TusLocal/meat/";
});

builder.Services.AddFileStoreage()
	.AddCore(o =>
	{
		o.UseFreeRedisCache("localhost:6379");
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
app.MapTusStorageForRedis();

app.Run();
