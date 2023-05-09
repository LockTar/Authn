using Authn.Sample.BlazorServer.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace Authn.Sample.BlazorServer;
public class Program
{
    public const string AadOpenIdConnectScheme = "OpenIdConnectAad"; // Custom scheme name gives problem

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthentication(Program.AadOpenIdConnectScheme) // Set the default scheme name
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"), Program.AadOpenIdConnectScheme) // Use a custom scheme name 
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddDownstreamApi("DownstreamApi1", builder.Configuration.GetSection("DownstreamApi1"))
            .AddDownstreamApi("DownstreamApi2", builder.Configuration.GetSection("DownstreamApi2"))
                .AddInMemoryTokenCaches();

        builder.Services.AddControllersWithViews()
            .AddMicrosoftIdentityUI();

        builder.Services.AddAuthorization(options =>
        {
            // By default, all incoming requests will be authorized according to the default policy
            options.FallbackPolicy = options.DefaultPolicy;
        });

        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor()
            .AddMicrosoftIdentityConsentHandler();
        builder.Services.AddSingleton<WeatherForecastService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.MapControllers();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        app.Run();
    }
}
