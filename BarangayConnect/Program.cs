using BarangayConnect.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<SqliteConnectionFactory>();
builder.Services.AddSingleton<PortalRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Portal/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Portal}/{action=Index}/{id?}")
    .WithStaticAssets();

await app.Services.GetRequiredService<PortalRepository>().InitializeAsync();

app.Run();
