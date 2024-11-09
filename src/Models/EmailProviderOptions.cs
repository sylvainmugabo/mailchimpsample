namespace EmailServe.Models;

public record EmailProviderOptions(string ApiKey);
public class MailChimp
{
    public required string ApiKey { get; set; }
}


