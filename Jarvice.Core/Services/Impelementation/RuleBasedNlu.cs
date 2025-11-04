using Jarvice.Core.Models;
using Jarvice.Core.Services.Interface;
using System.Text.RegularExpressions;

namespace Jarvice.Core.Services.Impelementation
{
    public class RuleBasedNlu : INluEngine
    {
        #region Fields

        private static readonly Regex ReminderRx = new(@"(?i)(یاد|remind).*(ساعت|at)?\s*(\d{1,2}[:٫\.]?\d{0,2})?", RegexOptions.Compiled);
        private static readonly Regex TimeRx = new(@"(?i)چند\s*ساعته|time|ساعت چنده", RegexOptions.Compiled);
        private static readonly Regex OpenRx = new(@"(?i)(باز|open)\s+(?<app>\w+)", RegexOptions.Compiled);

        #endregion

        public IntentResult Predict(string text)
        {
            if (ReminderRx.IsMatch(text))
            {
                var m = ReminderRx.Match(text);
                var t = m.Groups[0].Value ?? "";
                return new IntentResult("reminder.Create", new() { ["Time"] = t });
            }
            else if (TimeRx.IsMatch(text))
            {
                return new IntentResult("time.now", new());
            }

            var om = OpenRx.Match(text);
            if (om.Success)
            {
                return new IntentResult("open_app", new() { ["app"] = om.Groups["app"].Value });
            }

            return new IntentResult("SmallTalk", new());
        }
    }
}