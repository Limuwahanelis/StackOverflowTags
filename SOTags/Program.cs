using Microsoft.IdentityModel.Tokens;
using SOTags;
using SOTags.Data;
using SOTags.Services;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddScoped<StackExchangeService>();
builder.Services.AddDbContext<SOTagsDBContext>();
builder.Services.AddTransient<TagDBService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();
using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;
    StackExchangeService stackExchangeService = services.GetRequiredService<StackExchangeService>();
    using (var context = new SOTagsDBContext())
    {
        APIHelper.SetUP();
        if(context.Tags.IsNullOrEmpty()) await stackExchangeService.ImportTagsToDB("https://api.stackexchange.com/2.3/tags?page=1&order=desc&sort=popular&site=stackoverflow&filter=!bMsg5CXCu9jto8", context);

    }
}
//("https://api.stackexchange.com/2.3/tags?page=1&order=desc&sort=popular&site=stackoverflow&filter=!bMsg5CXCu9jto8");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
