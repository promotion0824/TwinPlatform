
using OntologyGraphTool.Models;
using OntologyGraphTool.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<MappedOntology>();
builder.Services.AddSingleton<WillowOntology>();
builder.Services.AddSingleton<MappingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

var env = app.Environment;

// In production
// if (!env.IsDevelopment())
// {
//     // This serves the ClientApp/dist folder, see above
//     app.UseSpaStaticFiles();
// }

// app.UseSpa(spa =>
// {
//     spa.Options.SourcePath = "ClientApp";
//     if (env.IsDevelopment())
//     {
//         spa.Options.DevServerPort = 5174;
//         //the npm script must start with "echo Starting the development server &&" for this to work
//         spa.UseReactDevelopmentServer(npmScript: "run dev");
//     }
// });

app.MapFallbackToFile("index.html");

app.Run();
