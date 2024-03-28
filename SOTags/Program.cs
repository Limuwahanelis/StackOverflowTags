using Microsoft.IdentityModel.Tokens;
using SOTags;
using SOTags.Data;
using SOTags.Exceptions;
using SOTags.Interfaces;
using SOTags.Repositories;
using SOTags.Services;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddTransient<StackExchangeService>();
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
APIHelper.SetUP();
using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;
    StackExchangeTagDBService stackExchangeTagDBService = services.GetRequiredService<StackExchangeTagDBService>();
    await stackExchangeTagDBService.ImportTagsFromStackOverflow();
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
