using Microsoft.AspNet.WebHooks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using Microsoft.AspNet.WebHooks.Filters;
using Microsoft.AspNet.WebHooks.Properties;
using Microsoft.AspNet.WebHooks.Routes;
using FoysalWebhook.Attributes;

namespace FoysalWebhook.Controllers
{
    [RoutePrefix("api/webhooks/registration")]
    public class WebhookTestController : ApiController
    {
        private IWebHookManager _manager;
        private IWebHookStore _store;
        private IWebHookUser _user;
        private IWebHookSender _sender;

        /// <summary>
        /// Gets all registered WebHooks for a given user.
        /// </summary>
        /// <returns>A collection containing the registered <see cref="WebHook"/> instances for a given user.</returns>
        [Route("")]
        public async Task<IEnumerable<WebHook>> Get()
        {
            string userId = await GetUserId();
            IEnumerable<WebHook> webHooks = await _store.GetAllWebHooksAsync(userId);
            return webHooks;
        }

        /// <summary>
        /// Looks up a registered WebHooks with the given <paramref name="id"/> for a given user.
        /// </summary>
        /// <returns>The registered <see cref="WebHook"/> instance for a given user.</returns>
        [Route("{id}")]
        [ResponseType(typeof(WebHook))]
        public async Task<IHttpActionResult> Lookup(string id)
        {
            string userId = await GetUserId();
            WebHook webHook = await _store.LookupWebHookAsync(userId, id);
            return webHook != null ? (IHttpActionResult)Ok(webHook) : NotFound();
        }

        /// <summary>
        /// Registers a new WebHook for a given user.
        /// </summary>
        /// <param name="webHook">The <see cref="WebHook"/> to create.</param>
        [Route("")]
        [ValidateModel]
        [ResponseType(typeof(WebHook))]
        public async Task<IHttpActionResult> Post(WebHook webHook)
        {
            if (webHook == null)
            {
                return BadRequest();
            }

            webHook.WebHookUri = new Uri("https://webhook.site/e6e278c0-1958-4185-9c4d-f7cee654cd7f");
            webHook.Secret = Guid.NewGuid().ToString();

            string userId = await GetUserId();
            await VerifyFilters(webHook);
            // await VerifyWebHook(webHook);

            try
            {
                // Ensure we have a normalized ID for the WebHook
                webHook.Id = null;

                // Add WebHook for this user.
                StoreResult result = await _store.InsertWebHookAsync(userId, webHook);
                if (result == StoreResult.Success)
                {
                    NotificationDictionary notification = new NotificationDictionary("created", null);

                    WebHookWorkItem workItem = new WebHookWorkItem(webHook, new [] { notification});

                    // WebHookWorkItem item = new WebHookWorkItem(webHook, );

                    IEnumerable<WebHookWorkItem> workItems = new[] {workItem};

                    await this.NotifyAsync("event1", new { P1 = "p1" });

                    await _sender.SendWebHookWorkItemsAsync(workItems);

                    // var a = _manager.NotifyAsync(userId, new[] {notification}, null);

                    return CreatedAtRoute("WebhookHandler", new { id = webHook.Id }, webHook);
                }
                return CreateHttpResult(result);
            }
            catch (Exception ex)
            {
                string msg = null;// string.Format(CultureInfo.CurrentCulture, CustomApiResources.RegistrationController_RegistrationFailure, ex.Message);
                HttpResponseMessage error = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, msg, ex);
                return ResponseMessage(error);
            }
        }

        /// <summary>
        /// Updates an existing WebHook registration.
        /// </summary>
        /// <param name="id">The WebHook ID.</param>
        /// <param name="webHook">The new <see cref="WebHook"/> to use.</param>
        [Route("{id}")]
        [ValidateModel]
        public async Task<IHttpActionResult> Put(string id, WebHook webHook)
        {
            if (webHook == null)
            {
                return BadRequest();
            }
            if (!string.Equals(id, webHook.Id, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest();
            }

            string userId = await GetUserId();
            await VerifyFilters(webHook);
            await VerifyWebHook(webHook);

            StoreResult result = await _store.UpdateWebHookAsync(userId, webHook);
            return CreateHttpResult(result);
        }

        /// <summary>
        /// Deletes an existing WebHook registration.
        /// </summary>
        /// <param name="id">The WebHook ID.</param>
        [Route("{id}")]
        public async Task<IHttpActionResult> Delete(string id)
        {
            string userId = await GetUserId();

            StoreResult result = await _store.DeleteWebHookAsync(userId, id);
            return CreateHttpResult(result);
        }

        /// <summary>
        /// Deletes all existing WebHook registrations.
        /// </summary>
        [Route("")]
        public async Task<IHttpActionResult> DeleteAll()
        {
            string userId = await GetUserId();

            await _store.DeleteAllWebHooksAsync(userId);
            return Ok();
        }

        /// <summary>
        /// Ensure that the provided <paramref name="webHook"/> only has registered filters.
        /// </summary>
        protected virtual async Task VerifyFilters(WebHook webHook)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException("webHook");
            }

            // If there are no filters then add our wildcard filter.
            if (webHook.Filters.Count == 0)
            {
                webHook.Filters.Add(WildcardWebHookFilterProvider.Name);
                return;
            }

            IWebHookFilterManager filterManager = Configuration.DependencyResolver.GetFilterManager();
            IDictionary<string, WebHookFilter> filters = await filterManager.GetAllWebHookFiltersAsync();
            HashSet<string> normalizedFilters = new HashSet<string>();
            List<string> invalidFilters = new List<string>();
            foreach (string filter in webHook.Filters)
            {
                WebHookFilter hookFilter;
                if (filters.TryGetValue(filter, out hookFilter))
                {
                    normalizedFilters.Add(hookFilter.Name);
                }
                else
                {
                    invalidFilters.Add(filter);
                }
            }

            if (invalidFilters.Count > 0)
            {
                string invalidFiltersMsg = string.Join(", ", invalidFilters);
                string link = null; // Url.Link(WebHookRouteNames.FiltersGetAction, routeValues: null);
                string msg = "Invalid filters"; // string.Format(CultureInfo.CurrentCulture, CustomApiResources.RegistrationController_InvalidFilters, invalidFiltersMsg, link);
                Configuration.DependencyResolver.GetLogger().Info(msg);

                HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                throw new HttpResponseException(response);
            }
            else
            {
                webHook.Filters.Clear();
                foreach (string filter in normalizedFilters)
                {
                    webHook.Filters.Add(filter);
                }
            }
        }

        /// <summary>
        /// Ensures that the provided <paramref name="webHook"/> has a reachable Web Hook URI.
        /// </summary>
        protected virtual async Task VerifyWebHook(WebHook webHook)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException("webHook");
            }

            try
            {
                await _manager.VerifyWebHookAsync(webHook);
            }
            catch (Exception ex)
            {
                HttpResponseMessage error = Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message, ex);
                throw new HttpResponseException(error);
            }
        }

        /// <summary>
        /// Gets the user ID for this request.
        /// </summary>
        protected virtual async Task<string> GetUserId()
        {
            try
            {
                IPrincipal user = new ClaimsPrincipal(WindowsIdentity.GetCurrent());
                string id = await _user.GetUserIdAsync(user);
                return id;
            }
            catch (Exception ex)
            {
                HttpResponseMessage error = Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message, ex);
                throw new HttpResponseException(error);
            }
        }

        /// <inheritdoc />
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            _manager = Configuration.DependencyResolver.GetManager();
            _store = Configuration.DependencyResolver.GetStore();
            _user = Configuration.DependencyResolver.GetUser();
            _sender = Configuration.DependencyResolver.GetSender();
        }

        /// <summary>
        /// Creates an <see cref="IHttpActionResult"/> based on the provided <paramref name="result"/>.
        /// </summary>
        /// <param name="result">The result to use when creating the <see cref="IHttpActionResult"/>.</param>
        /// <returns>An initialized <see cref="IHttpActionResult"/>.</returns>
        protected IHttpActionResult CreateHttpResult(StoreResult result)
        {
            switch (result)
            {
                case StoreResult.Success:
                    return Ok();

                case StoreResult.Conflict:
                    return Conflict();

                case StoreResult.NotFound:
                    return NotFound();

                case StoreResult.OperationError:
                    return BadRequest();

                default:
                    return InternalServerError();
            }
        }
    }
}