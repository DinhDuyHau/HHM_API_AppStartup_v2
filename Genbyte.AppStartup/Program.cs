using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Genbyte.AppStartup;
using Genbyte.Sys.Common;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.AppAuth.Services;

var builder = WebApplication.CreateBuilder(args);
string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// add services to DI container
{
    var services = builder.Services;

    //add CORS for client request
    var corsSetting = builder.Configuration.GetSection("CorsSetting");
    services.Configure<string[]>(corsSetting);
    string[] cors_settings = corsSetting.Get<string[]>();
    services.AddCors(options =>
    {
        options.AddPolicy(name: MyAllowSpecificOrigins,
                          policy =>
                          {
                              policy.WithOrigins(cors_settings)
                                .AllowAnyHeader()
                                .AllowCredentials()
                                .AllowAnyMethod();
                          });
    });

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
    services.Configure<Security>(builder.Configuration.GetSection("Security"));
    services.Configure<SmtpConfig>(builder.Configuration.GetSection("SmtpConfig"));

    // In-Memory Caching
    builder.Services.AddMemoryCache();

    //for session 
    var sessionSettingConfig = builder.Configuration.GetSection("SessionSetting");
    services.Configure<SessionSetting>(sessionSettingConfig);
    SessionSetting sesion_settings = sessionSettingConfig.Get<SessionSetting>();
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.Cookie.Name = sesion_settings.Name;
        options.IdleTimeout = new TimeSpan(0, sesion_settings.ExpireMinutes, 0);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    // configure DI for application services
    services.AddScoped<ITokenUtils, JwtUtils>();
    services.AddScoped<IUserService, UserService>();

    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

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

app.UseSession();

// global cors policy
//app.UseCors(x => x
//    .AllowAnyOrigin()
//    .AllowAnyMethod()
//    .AllowAnyHeader());
app.UseCors(MyAllowSpecificOrigins);

// custom jwt auth middleware
app.UseMiddleware<JwtAuthentication>();

Startup.Services = app.Services;

app.MapControllers();

app.Run();
