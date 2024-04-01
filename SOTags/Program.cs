using Microsoft.IdentityModel.Tokens;
using SOTags;
using SOTags.Data;
using SOTags.Exceptions;
using SOTags.Interfaces;
using SOTags.Repositories;
using SOTags.Services;
using Serilog;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

//builder.Logging.ClearProviders();
// Add services to the container.
//builder.Services.AddHttpClient().ConfigureHttpClientDefaults(s =>
//{
//    s.ConfigurePrimaryHttpMessageHandler(handler =>
//    new HttpClientHandler
//    {
//        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
//        UseCookies = false,
//        AllowAutoRedirect = false,
//        UseDefaultCredentials = true,
//    });
//});
builder.Services.AddHttpClient<IStackExchangeService,StackExchangeService>().ConfigurePrimaryHttpMessageHandler(handler =>
    new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        UseCookies = false,
        AllowAutoRedirect = false,
        UseDefaultCredentials = true,
    });


builder.Services.AddTransient<IStackExchangeService,StackExchangeService>();
builder.Services.AddDbContext<SOTagsDBContext>();
builder.Services.AddTransient<PagedTagDBService>();
builder.Services.AddTransient<StackExchangeTagDBService>();
builder.Services.AddScoped<ITagsRepository,TagsRepository>();

builder.Services.AddExceptionHandler<StackExchangeServerExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();
using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;
    StackExchangeTagDBService stackExchangeTagDBService = services.GetRequiredService<StackExchangeTagDBService>();
    try
    {
        await stackExchangeTagDBService.ImportTagsFromStackOverflow();
    }
    catch (StackExchangeServerCouldNotBeReachedException e) 
    {
        Log.Logger.Fatal($"An problem occured when reaching Stack Exchange server. Message from server: {e.StackExchangeSetverMessage}\n" +
                $"Managed to {e.OperationMessage}");
        //throw;
    }
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseExceptionHandler();

app.MapControllers();

app.Run();
