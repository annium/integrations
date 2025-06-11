using System;
using Annium.AspNetCore.Extensions;
using Annium.Infrastructure.Hosting;
using Annium.Integrations.Social.LinkedIn.Demo;
using Annium.Logging.Microsoft;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServicePack<ServicePack>();
builder.Logging.ConfigureLoggingBridge();
builder.WebHost.UseKestrelDefaults();

// Add services
builder.Services.AddHttpClient();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseExceptionMiddleware();
app.UseSession();
app.UseRouting();
app.UseCorsDefaults();
app.UseRequestLocalization("en", "ru");
app.MapControllers();

await app.RunAsync();
