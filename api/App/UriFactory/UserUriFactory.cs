﻿using Microsoft.AspNetCore.Mvc;

namespace App.UriFactory
{
    public static class UserUriFactory
    {
        public const string UserRouteName = "UserRouteName";
        public const string UserMeName = "UserMeName";
        public const string CreateFormRouteName = "UserCreateForm";
        public const string EditFormRouteName = "UserEditForm";
        public const string UserTodosRouteName = "UserNamedTodoCollectionRouteName";
        public const string UserTenantsRouteName = "UserTenantsRouteName";
        public const string UserTenantRouteName = "UserTenantRouteName";
        public const string UserTenantTodoListRouteName = "UserTenantTodoListRouteName";

        public static string MakeUserUri(this string id, IUrlHelper url)
        {
            return url.Link(UserRouteName, new {id = id});
        }

        public static string MakeUserCreateFormUri(this IUrlHelper url)
        {
            return url.Link(CreateFormRouteName, new { });
        }

        public static string MakeUserEditFormUri(this IUrlHelper url)
        {
            return url.Link(EditFormRouteName, new { });
        }

        public static string MakeUserMeUri(this IUrlHelper url)
        {
            return url.Link(UserMeName, new { });
        }

        public static string MakeUserTodosUri(this string id, IUrlHelper url)
        {
            return url.Link(UserTodosRouteName, new {id = id});
        }

        public static string MakeUserTenantsUri(this string id, IUrlHelper url)
        {
            return url.Link(UserTenantsRouteName, new {id = id});
        }

        public static string MakeUserTenantUri(this string id, string tenantId, IUrlHelper url)
        {
            return url.Link(UserTenantRouteName, new {id = id, tenantId = tenantId});
        }
        
        public static string MakeUserTenantTodoListUri(this string id, string tenantId, IUrlHelper url)
        {
            return url.Link(UserTenantTodoListRouteName, new {id = id, tenantId = tenantId});
        }
        
       
    }
}