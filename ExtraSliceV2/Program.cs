using Amazon.S3;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using MVCAmazonExtra.Helpers;
using MVCAmazonExtra.Models;
using MVCAmazonExtra.Services;
using MVCApiExtraSlice.Services;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options => options.EnableEndpointRouting = false).AddSessionStateTempDataProvider();
//session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
//seguridad
builder.Services.AddAuthentication(options =>
{
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

}).AddCookie();
//habilitar keyVault
builder.Services.AddAzureClients(factory =>
{
    factory.AddSecretClient(builder.Configuration.GetSection("KeyVault"));
});

// Add services to the container.
builder.Services.AddTransient<ServiceRestaurante>();
builder.Services.AddTransient<ServiceCacheAmazon>();
builder.Services.AddTransient<ServiceStorageS3>();
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

string miSecreto = await HelperSecretManager.GetSecret();

//PODEMOS DAR FORMATO A NUESTRO SECRETO
ElasticModel model = JsonConvert.DeserializeObject<ElasticModel>(miSecreto);

//SI ESTAMOS EN UNA APP MAS COMPLEJA DONDE NECESITAMOS RECUPERAR MAS 
//DATOS Y UTILIZARLOS EN DISTINTAS CLASES, AL ESTILO DE IConfiguration
builder.Services.AddSingleton<ElasticModel>(x => model).BuildServiceProvider();

//connection to az keyvault
SecretClient secretClient = builder.Services.BuildServiceProvider().GetService<SecretClient>();

//get blobs connection
//KeyVaultSecret keyVaultSecret = await secretClient.GetSecretAsync("blobs");
//blobs
//string azureKeys = keyVaultSecret.Value; ;
//BlobServiceClient blobServiceClient = new BlobServiceClient(azureKeys);
//builder.Services.AddTransient<BlobServiceClient>(x => blobServiceClient);

//cache redis
//KeyVaultSecret keyVaultCache = await secretClient.GetSecretAsync("cacheredis");
//string cnnCacheRedis = keyVaultCache.Value;
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = cnnCacheRedis;
//});

//REDIS AWS
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = model.Connection_cache;
    //options.InstanceName = "cache-extra-slice";
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseResponseCaching();
app.UseSession();
app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "{controller=Carta}/{action=Index}/{id?}");
});
app.Run();
