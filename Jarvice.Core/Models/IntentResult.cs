namespace Jarvice.Core.Models
{
    public sealed record IntentResult(string Intent, Dictionary<string, string> Slots);
}