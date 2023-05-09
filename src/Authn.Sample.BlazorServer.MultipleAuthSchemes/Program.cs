using Authn.Sample.BlazorServer.MultipleAuthSchemes.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System.Security.Claims;

namespace Authn.Sample.BlazorServer.MultipleAuthSchemes;
public class Program
{
    public const string B2cOpenIdConnectScheme = "OpenIdConnectB2c";
    public const string AadOpenIdConnectScheme = "OpenIdConnectAad";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // As described here: https://github.com/AzureAD/microsoft-identity-web/wiki/multiple-authentication-schemes#cookie-schemes
        var authenticationBuilder = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);

        authenticationBuilder.AddCookie(options =>
        {
            ////options.ExpireTimeSpan = new TimeSpan(7, 0, 0, 0);

            // Default login path to customers because for employees we get a hidden login page
            options.LoginPath = "/MicrosoftIdentity/Account/SignIn/" + B2cOpenIdConnectScheme;

            options.Events = new CookieAuthenticationEvents()
            {
                OnSigningIn = async context =>
                {
                    var claimsIdentity = context.Principal.Identity as ClaimsIdentity;

                    var authScheme = context.Properties.Items.Where(k => k.Key == ".AuthScheme").Single();
                    var authSchemeClaim = new Claim(authScheme.Key, authScheme.Value);
                    claimsIdentity.AddClaim(authSchemeClaim);

                    await Task.CompletedTask;
                }
            };
        });

        builder.Services.AddAuthentication()
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"), Program.AadOpenIdConnectScheme, null)
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddDownstreamApi("DownstreamApi1", builder.Configuration.GetSection("DownstreamApi1"))
            .AddDownstreamApi("DownstreamApi2", builder.Configuration.GetSection("DownstreamApi2"))
                .AddInMemoryTokenCaches();

        builder.Services.AddAuthentication()
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAdB2C"), Program.B2cOpenIdConnectScheme, null)
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddDownstreamApi("DownstreamApi1", builder.Configuration.GetSection("DownstreamApi1"))
            .AddDownstreamApi("DownstreamApi2", builder.Configuration.GetSection("DownstreamApi2"))
                .AddInMemoryTokenCaches();

        builder.Services.AddControllersWithViews()
            .AddMicrosoftIdentityUI();

        builder.Services.AddAuthorization(options =>
        {
            // By default, all incoming requests will be authorized according to the default policy
            //options.FallbackPolicy = options.DefaultPolicy;
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

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        app.Run();
    }
}
