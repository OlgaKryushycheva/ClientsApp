using Microsoft.EntityFrameworkCore;
using ClientsApp.Models;
using ClientsApp.BLL.Interfaces;
using ClientsApp.BLL.Services;
using AutoMapper;
using ClientsApp.BLL;

var builder = WebApplication.CreateBuilder(args);

// --- ϳ��������� ���� ����� ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- ϳ��������� AutoMapper ---
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// --- ��������� ������ ---
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IExecutorService, ExecutorService>();
builder.Services.AddScoped<IClientTaskService, ClientTaskService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// --- �������� ������������ ��� MVC ---
builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- ������������ pipeline ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// --- ������������ �������� ---
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
