using Microsoft.EntityFrameworkCore;
using ClientsApp.Models;
using ClientsApp.BLL.Interfaces;
using ClientsApp.BLL.Services;
using AutoMapper;
using ClientsApp.BLL;

var builder = WebApplication.CreateBuilder(args);

// --- Підключення бази даних ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Підключення AutoMapper ---
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// --- Реєстрація сервісів ---
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IExecutorService, ExecutorService>();
builder.Services.AddScoped<IClientTaskService, ClientTaskService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// --- Додаткові налаштування для MVC ---
builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- Налаштування pipeline ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// --- Налаштування маршрутів ---
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
