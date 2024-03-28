using Microsoft.IdentityModel.Tokens;
using SOTags;
using SOTags.Data;
using SOTags.Interfaces;
using SOTags.Repositories;
using SOTags.Services;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddTransient<StackExchangeService>();
builder.Services.AddDbContext<SOTagsDBContext>();
builder.Services.AddTransient<PagedTagDBService>();
builder.Services.AddScoped<ITagsRepository,TagsRepository>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();
APIHelper.SetUP();
// populate database if empty
using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;
    StackExchangeService stackExchangeService = services.GetRequiredService<StackExchangeService>();
    //using (var context = new SOTagsDBContext())
    //{
        //context.Database.EnsureCreated();
        //if(context.Tags.IsNullOrEmpty()) 
        await stackExchangeService.ImportTagsFromStackOverflow();

    //}
}
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
