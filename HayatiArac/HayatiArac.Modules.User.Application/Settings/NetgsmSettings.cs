namespace HayatiArac.Modules.User.Application.Settings;

public sealed class NetgsmSettings
{
    public const string SectionName = "Netgsm";
    public string ApiKey { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
}
