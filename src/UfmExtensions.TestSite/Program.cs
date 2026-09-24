WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Machine-specific settings and secrets, gitignored, and loaded last so they override every other file.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

// Show exceptions for Development environment
if (app.Environment.IsDevelopment()|| app.Environment.EnvironmentName=="Local")
{
	app.UseDeveloperExceptionPage();
}


await app.BootUmbracoAsync();


app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
