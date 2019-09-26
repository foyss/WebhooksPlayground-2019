using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.WebHooks;

namespace FoysalWebhook.Controllers
{
    [RoutePrefix("api/webhooks/handle")]
    public class WebhookHandlerController : WebhookTestController
    {
        public WebhookHandlerController()
        {
                
        }

        [HttpGet]
        [Route("")]
        public HttpResponseMessage CheckWebhookUrl(WebHook webhook)
        {
            var allUrlKeyValues = Request.GetQueryNameValuePairs();
            var parmVal = allUrlKeyValues.FirstOrDefault(x => x.Key == "echo").Value;
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Content = new StringContent(parmVal, System.Text.Encoding.UTF8, "text/plain");
            return resp;
        }

        [HttpGet]
        [Route("{id}", Name = "WebhookHandler")]
        public async Task<IHttpActionResult> HandleWebhook(string id, WebHook webhook)
        {
            return BadRequest("Unable to determine webhook or id");
            return Ok();
        }
    }
}