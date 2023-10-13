using CansolveApplication.CansolveApplicationServises;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IcansolveServises, CansolveServises>();
builder.Services.Configure<MongoSocket>(builder.Configuration.GetSection("MongoObject"));
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CansolvDataAPI", Version = "v1@ank" });
    // Configure XML comments for your controllers and models here
    // c.IncludeXmlComments("your-xml-comments-file.xml");
});
builder.Services.AddMvc().AddXmlSerializerFormatters();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();



app.MapControllers();

app.Run();
