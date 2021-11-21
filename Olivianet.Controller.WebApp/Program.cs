using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Olivianet.Controller.WebApp;
using Olivianet.Controller.WebApp.Data;
using Olivianet.Controller.WebApp.Helpers;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
string databaseType = builder.Configuration["AppSettings:DatabaseType"];

builder.Services.AddControllersWithViews ();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    switch (databaseType)
    {
        case Constants.SqliteDatabase:
            options.UseSqlite(connectionString);
            break;
        case Constants.PostgresDatabase:
            options.UseNpgsql(connectionString);
            break;
        case Constants.SqlServerDatabase:
            options.UseSqlServer(connectionString);
            break;
    }

    options.UseOpenIddict();
});

builder.Services.AddQuartz(options =>
{
    options.UseMicrosoftDependencyInjectionJobFactory();
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
});
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

builder.Services.AddOpenIddict()
        // Register the OpenIddict core components.
        .AddCore(options =>
        {
            // Configure OpenIddict to use the Entity Framework Core stores and models.
            // Note: call ReplaceDefaultEntities() to replace the default entities.
            options.UseEntityFrameworkCore()
                   .UseDbContext<ApplicationDbContext>();
        })
        // Register the OpenIddict server components.
        .AddServer(options =>
        {
            // Enable the token endpoint.
            options.SetTokenEndpointUris("/connect/token");

            // Enable the client credentials flow.
            options.AllowClientCredentialsFlow();

            // Register the signing and encryption credentials.
            // TODO: Switch this to some generated certificate
            options.AddDevelopmentEncryptionCertificate()
                  .AddDevelopmentSigningCertificate();

            // Register the ASP.NET Core host and configure the ASP.NET Core options.
            options.UseAspNetCore()
                  .EnableTokenEndpointPassthrough();
        })
        // Register the OpenIddict validation components.
        .AddValidation(options =>
        {
            // Import the configuration from the local OpenIddict server instance.
            options.UseLocalServer();

            // Register the ASP.NET Core host.
            options.UseAspNetCore();
        });

        // Register the worker responsible of seeding the database with the sample clients.
        // Note: in a real world application, this step should be part of a setup script.
        builder.Services.AddHostedService<Worker>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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

app.MapRazorPages();

app.UseEndpoints(options =>
{
    options.MapControllers();
    options.MapDefaultControllerRoute();
});

app.UseWelcomePage();

app.Run();
