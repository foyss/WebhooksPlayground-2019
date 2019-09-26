using Microsoft.AspNet.WebHooks;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FoysalWebhook
{
    public class Filter : IWebHookFilterProvider
    {
        private readonly Collection<WebHookFilter> filters = new Collection<WebHookFilter>
        {
            new WebHookFilter { Name = "created", Description = "Document created." },
            new WebHookFilter { Name = "updated", Description = "Document updated." },
            new WebHookFilter { Name = "deleted", Description = "Document deleted." },
        };

        public Task<Collection<WebHookFilter>> GetFiltersAsync()
        {
            return Task.FromResult(this.filters);
        }
    }
}