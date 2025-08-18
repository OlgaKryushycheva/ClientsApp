using Microsoft.EntityFrameworkCore;
using ClientsApp.Models;
using ClientsApp.BLL.Interfaces;
using ClientsApp.BLL.Services;
using AutoMapper;
using ClientsApp.BLL;

var builder = WebApplication.CreateBuilder(args);

// --- Database connection ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- AutoMapper setup ---
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// --- Service registration ---
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IExecutorService, ExecutorService>();
builder.Services.AddScoped<IClientTaskService, ClientTaskService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IExecutorTaskService, ExecutorTaskService>();

// --- Additional MVC settings ---
builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- Pipeline configuration ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// --- Route configuration ---
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
