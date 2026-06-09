using Microsoft.EntityFrameworkCore;
using biblioteca_api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDataContext>(options =>
    options.UseSqlite("Data Source=biblioteca.db"));

var app = builder.Build();