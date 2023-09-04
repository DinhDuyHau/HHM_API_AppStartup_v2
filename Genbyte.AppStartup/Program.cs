using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Genbyte.AppStartup;
using Genbyte.Sys.Common;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.AppAuth.Services;

var builder = WebApplication.CreateBuilder(args);

// add services to DI container
{
    var services = builder.Services;
    services.AddCors();
    services.AddControllers();
    services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    //load plugin controllers from appsettings.json
    var appPluginSection = builder.Configuration.GetSection("ControllerPlugin");
    services.Configure<ControllerPlugin[]>(appPluginSection);
    ControllerPlugin[] pluginControllers = appPluginSection.Get<ControllerPlugin[]>();
    string pathRoot = AppDomain.CurrentDomain.BaseDirectory;
    if (pluginControllers != null && pluginControllers.Length > 0)
        foreach (ControllerPlugin plugin_item in pluginControllers)
        {
            try
            {
                string asm_file = pathRoot + (pathRoot.EndsWith("\\") ? "" : "\\") + plugin_item.Namespace + ".dll";
                Assembly pluginAssembly = Assembly.LoadFrom(asm_file);
                services.AddControllers().PartManager.ApplicationParts.Add(new AssemblyPart(pluginAssembly));
            }
            catch (Exception e)
            {
                Logger.Insert("", $"{e.GetType().Name}: Unable to load type {plugin_item.Classname}", e);
            }
        }

    //Swagger utility
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    // configure strongly typed settings object
    services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

    // In-Memory Caching
    builder.Services.AddMemoryCache();

    // configure DI for application services
    services.AddScoped<IUserService, UserService>();

}

var app = builder.Build();

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}*/
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
});

app.UseAuthorization();

// global cors policy
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// custom jwt auth middleware
app.UseMiddleware<JwtAuthentication>();

app.MapControllers();

app.Run();
