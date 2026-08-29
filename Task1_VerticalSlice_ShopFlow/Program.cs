using GetProduct;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<GetProductHandler>();



builder.Services.AddDbContext<AppDbcontext>(op =>
op.UseSqlServer(builder.Configuration.GetConnectionString("cs")));
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapApplicationEndpoin();

app.Run();
