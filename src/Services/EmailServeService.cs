using EmailServe.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace EmailServe.Services;

public class EmailServeService : IEmailServeSerivice
{
    private readonly HttpClient _httpClient;

    public EmailServeService(IHttpClientFactory httpClientFactory, MailChimp mailChimpOptions)
    {
        _httpClient = httpClientFactory.CreateClient("MailChimp");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"any:{mailChimpOptions.ApiKey}")));

    }
    public async Task<string> CreateCampaignAsync(string recipientId, string subject, string fromName, string replyTo)
    {
        var campaignData = new
        {
            type = "regular",
            recipients = new { list_id = "9f8747a409" },
            settings = new
            {
                subject_line = subject,
                from_name = fromName,
                reply_to = replyTo,
                template_id = 10032306
            }
        };

        var content = new StringContent(JsonConvert.SerializeObject(campaignData), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("campaigns", content);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            dynamic jsonResponse = JsonConvert.DeserializeObject(result);
            return jsonResponse.id;
        }
        else
        {
            Console.WriteLine($"Error creating campaign: {response.StatusCode}");
            return null;
        }
    }

    public async Task SendCampaignAsync(string campaignId)
    {
        HttpResponseMessage response = await _httpClient.PostAsync($"campaigns/{campaignId}/actions/send", null);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Campaign sent successfully.");
        }
        else
        {
            Console.WriteLine($"Error sending campaign: {response.StatusCode}");
        }
    }

    public async Task SendHelloWorldEmailAsync(string listId, string fromName, string replyTo)
    {
        string subject = "Hello World";

        // Step 1: Create the campaign
        string campaignId = await CreateCampaignAsync(listId, subject, fromName, replyTo);

        if (campaignId == null)
        {
            Console.WriteLine("Failed to create campaign.");
            return;
        }

        // Step 2: Set campaign content
        //await SetCampaignContentAsync(campaignId, htmlContent);

        // Step 3: Send the campaign
        await SendCampaignAsync(campaignId);
    }

    public async Task SetCampaignContentAsync(string campaignId, string htmlContent)
    {
        var contentData = new
        {
            html = htmlContent
        };

        var content = new StringContent(JsonConvert.SerializeObject(contentData), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PutAsync($"campaigns/{campaignId}/content", content);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error setting campaign content: {response.StatusCode}");
        }
    }
}

public interface IEmailServeSerivice
{
    //Task SendSimpleEmailAsync(string fromEmail, string toEmail, string toName, string message);
    Task<string> CreateCampaignAsync(string recipientId, string subject, string fromName, string replyTo);
    Task SetCampaignContentAsync(string campaignId, string htmlContent);
    Task SendCampaignAsync(string campaignId);
    Task SendHelloWorldEmailAsync(string listId, string fromName, string replyTo);
}
