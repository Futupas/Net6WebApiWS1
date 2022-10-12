using WSWebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<Service1>();


var appsettingsPath = ResolveAppsettingsPath(args);
if (appsettingsPath == null)
{
    Console.WriteLine($"Got default Appsettings file ({Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json")})");
    builder.Configuration.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
}
else
{
    Console.WriteLine($"Got command line Appsettings file ({appsettingsPath})");
    builder.Configuration.AddJsonFile(appsettingsPath, optional: false, reloadOnChange: false);
}

builder.Services.AddLogging(b =>
{
    b.AddConfiguration(builder.Configuration.GetSection("Logging"));
    b.AddFile(o => o.RootPath = AppContext.BaseDirectory);
});
builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    var kestrelSection = context.Configuration.GetSection("Kestrel");
    serverOptions.Configure(kestrelSection);
    //serverOptions.Configure(kestrelSection)
    //    .Endpoint("HTTPS", listenOptions =>
    //    {
    //        // ...
    //    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();







/// <returns>Null if default path is not provided, or path if it is.</returns>
string ResolveAppsettingsPath(string[] args)
{
    if (args == null || args.Length < 2) return null;
    for (int i = 0; i < args.Length; i++)
    {
        var arg = args[i].ToLower();
        if (arg == "appsettings" || arg == "--appsettings" || arg == "conf" || arg == "--conf")
        {
            if (i < args.Length - 1)
            {
                var next = args[i + 1].Trim();
                return next.Length > 0 ? next : null;
            }
        }
    }
    return null;
}
