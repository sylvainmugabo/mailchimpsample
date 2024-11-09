namespace EmailServe.Models;

public record EmailMessage(string From, string To, string Subject, string Content);
