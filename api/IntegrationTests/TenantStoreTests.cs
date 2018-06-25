﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.NoSQL;
using Xunit;

namespace IntegrationTests
{
    /// <summary>
    ///     NOTE TO SELF: this test could be refactored to get the setup/teardown as a base class for other tests
    /// </summary>
    public class TenantStoreTests : IDisposable
    {
        public TenantStoreTests()
        {
            dbProvider = DynamoDbServerTestUtils.CreateDatabase();
        }

        public void Dispose()
        {
            dbProvider.Dispose();
        }

        private readonly DynamoDbServerTestUtils.DisposableDatabase dbProvider;

        private readonly Func<DynamoDbServerTestUtils.DisposableDatabase, Task<TenantStore>> MakeStore = async dbProvider =>
        {
            await TableNameConstants
                .Tenant
                .CreateTable(dbProvider.Client);

            return new TenantStore(dbProvider.Context);
        };

        private readonly Func<TenantStore, Task<string>> Create = async store =>
        {
            var tenantCreateData = new TenantCreateData
            {
                Code = "test.rewire.nz",
                Name = "baba",
                Description = "new one"
            };
            return await store.Create(tenantCreateData);
        };

        [Fact]
        public async Task LoadTenant()
        {
            var tenantStore = await MakeStore(dbProvider);
            var id = await Create(tenantStore);
            var tenant = await tenantStore.Get(id);

            Assert.Equal("test.rewire.nz", tenant.Code);
            Assert.Equal("baba", tenant.Name);
            Assert.Equal("new one", tenant.Description);

            await dbProvider.Context.DeleteAsync<Tenant>(id);
        }

        [Fact]
        public async Task UserRemoveUser()
        {
            var tenantStore = await MakeStore(dbProvider);
            var id = await Create(tenantStore);
            var tenant = await tenantStore.Get(id);

            // default is an empty list of users
            Assert.Empty(tenant.User ?? new List<string>());

            // add a user
            var userId = Guid.NewGuid().ToString();
            await tenantStore.AddUser(id, userId);
            tenant = await tenantStore.Get(id);
            Assert.Contains(userId, tenant.User);

            // remove a user
            await tenantStore.RemoveUser(id, userId);
            tenant = await tenantStore.Get(id);
            Assert.DoesNotContain(userId, tenant.User ?? new List<string>());

            await dbProvider.Context.DeleteAsync<Tenant>(id);
        }
    }
}