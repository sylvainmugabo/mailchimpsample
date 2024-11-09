
using EmailServe.Models;
using EmailServe.Services;
using Mandrill;
using Mandrill.Extensions.DependencyInjection;
using Mandrill.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<MailChimp>(builder.Configuration.GetSection("MailChimp"));
builder.Services.AddScoped(cfg => cfg.GetService<IOptionsSnapshot<MailChimp>>().Value);
builder.Services.AddScoped<IEmailServeSerivice, EmailServeService>();
builder.Services.AddHttpClient("MailChimp", client =>
{
    client.BaseAddress = new Uri("https://us14.api.mailchimp.com/3.0/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddMandrill(options =>
{
    var option = builder.Configuration.GetSection("Mandrill").Get<EmailProviderOptions>() ?? throw new Exception("There is no mandrill section in config");

    options.ApiKey = option.ApiKey;
});

//builder.Services.Configure<EmailProviderOptions>(builder.Configuration.GetSection("MailChimp"));



var app = builder.Build();

app.MapGet("api/email", async (IEmailServeSerivice emailServeSerivice, IHttpClientFactory httpClientFactory) =>
{
    try
    {
        await emailServeSerivice.SendHelloWorldEmailAsync("mugabosylvain@yahoo.fr", "Mugabo N Sylvain", "sylvain.tabazi@gmail.com");
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("api/mandrill", async ([FromBody] EmailMessage message, IMandrillApi mandrill) =>
{
    var mandrillMessage = new MandrillMessage
    {
        FromEmail = message.From,
        To =
        [
            new MandrillMailAddress(message.To)
        ],
        Subject = message.Subject,
        Html = message.Content
    };

    try
    {
        var result = await mandrill.Messages.SendAsync(mandrillMessage);
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"{ex.Message}");
    }

    return Results.Ok();

});

app.Run();