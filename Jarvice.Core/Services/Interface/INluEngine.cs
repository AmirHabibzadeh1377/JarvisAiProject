using Jarvice.Core.Models;

namespace Jarvice.Core.Services.Interface
{
    public interface INluEngine
    {
        IntentResult Predict(string text);
    }
}