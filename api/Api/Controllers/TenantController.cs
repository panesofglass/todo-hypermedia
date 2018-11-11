﻿using System.Threading.Tasks;
using Api.Authorisation;
using Api.Web;
using App;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SemanticLink;
using SemanticLink.AspNetCore;
using SemanticLink.Form;
using Toolkit;

namespace Api.Controllers
{
    [Route("tenant")]
    public class TenantController : Controller
    {
        private ILogger<TenantController> Log { get; }
        private readonly ITenantStore _tenantStore;
        private readonly IUserStore _userStore;

        public TenantController(ITenantStore tenantStore, IUserStore userStore, ILogger<TenantController> log)
        {
            Log = log;
            _tenantStore = tenantStore;
            _userStore = userStore;
        }

        /// <summary>
        ///   The basic information about a tenant. 
        /// </summary>
        /// <remarks>
        ///    There is a small level of disclosure here because we want authenticated users
        ///     to find this tenant and then be able to register against it.
        /// </remarks>
        [HttpGet("{id}", Name = TenantUriFactory.TenantRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
        [Authorise(RightType.Tenant, Permission.Get)]
        public async Task<TenantRepresentation> Get(string id)
        {
            return (await _tenantStore
                    .Get(id))
                .ThrowObjectNotFoundExceptionIfNull("Invalid tenant")
                .ToRepresentation(Url);
        }

        [HttpGet("form/create", Name = TenantUriFactory.CreateFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        public CreateFormRepresentation TenantCreateForm()
        {
            return Url.ToTenantCreateFormRepresentation();
        }

        [HttpGet("form/edit", Name = TenantUriFactory.EditFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        public EditFormRepresentation TenantEditForm()
        {
            return Url.ToTenantEditFormRepresentation();
        }

  
        ///////////////////////////////////////
        //
        //  User collection
        //  ===============

        /// <summary>
        ///     User collection on a tenant
        /// </summary>
        /// <remarks>
        ///    Authenticated users will get back a list of users. Authenticated but unregistered
        ///    users get back an empty collection. This allows us to parent the creation of new users off
        ///    the tenant and allow users who are unknown but authenticated to register themselves.
        /// </remarks>
        [HttpGet("{id}/user/", Name = TenantUriFactory.TenantUsersRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(NoCache = true)]
        [AuthoriseTenantUserCollection(Permission.Get)]
        public async Task<FeedRepresentation> GetUsers(string id)
        {
            return (await _tenantStore.GetUsersByTenant(id))
                .ToRepresentation(id, Url);
        }

        /// <summary>
        ///     Create a user (if doesn't exist) and register for a tenant. 
        /// </summary>
        [HttpPost("{id}/user/")]
        [AuthoriseTenantUserCollection(Permission.Post)]
        public async Task<IActionResult> RegisterUser([FromBody] UserCreateDataRepresentation data, string id)
        {
            (await _tenantStore.Get(id))
                .ThrowObjectNotFoundExceptionIfNull("Invalid tenant");

            var user = await _userStore.GetByExternalId(data.ExternalId);

            // Create the user if it doesn't already exist
            if (user.IsNull() || user.Id.IsNullOrWhitespace())
            {
                // make the user
                var userId = await _userStore.Create(
                    User.GetId(),
                    id,
                    data.FromRepresentation(),
                    Permission.FullControl,
                    CallerCollectionRights.User);

                // and stick it on the tenant
                await _tenantStore.IncludeUser(id, userId, Permission.Get, CallerCollectionRights.Tenant);

                Log.DebugFormat("New user {0} registered on tenant {1}", userId, id);

                // now, we have the identity user, link this into the new user
                return userId
                    .MakeUserUri(Url)
                    .MakeCreated();
            }

            // 409 if already on the tenant
            (await _tenantStore.IsRegisteredOnTenant(id, user.Id))
                .ThrowInvalidOperationExceptionIf(exists => exists, "User already exists on tenant");

            // 202 if now included
            await _tenantStore.IncludeUser(id, user.Id, Permission.Get, CallerCollectionRights.Tenant);

            Log.DebugFormat("Registered existing user {0} on tenant {1}", user.Id, id);

            return user.Id
                .MakeUserUri(Url)
                .MakeAccepted();
        }
    }
}