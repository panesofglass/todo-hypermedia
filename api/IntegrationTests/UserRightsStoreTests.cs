using System;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.NoSQL;
using Xunit;

namespace IntegrationTests
{
    public class UserRightsStoreTests : BaseProvider
    {
        private readonly Func<DynamoDbServerTestUtils.DisposableDatabase, Task<UserRightsStore>> MakeStore =
            async dbProvider =>
            {
                await TableNameConstants
                    .UserRight
                    .CreateTable(dbProvider.Client);

                return new UserRightsStore(dbProvider.Context);
            };

        [Fact]
        public async Task LoadUserRight()
        {
            var store = await MakeStore(DbProvider);

            var userId = IdGenerator.New();
            var resourceId = IdGenerator.New();

            var id = await store.Create(userId, resourceId, RightType.Todo, Permission.Get);

            var userRights = await store.Get(userId, resourceId);
            Assert.NotNull(userRights);

            Assert.Equal(Permission.Get, userRights.Rights);
            Assert.Equal(RightType.Todo, userRights.Type);
            await DbProvider.Context.DeleteAsync<UserRight>(id);
        }

        [Fact]
        public async Task CreateUserRightWithNoInheritance()
        {
            var store = await MakeStore(DbProvider);

            // seed the tenant owner
            var ownerId = IdGenerator.New();
            var tenantUserId = IdGenerator.New();
            
            // seed the top level tenant resource
            var resourceId = IdGenerator.New();

            await store.CreateInherit(RightType.Tenant, ownerId, resourceId, RightType.Tenant, Permission.FullCreatorOwner);
            await store.CreateInherit(RightType.Tenant, ownerId, resourceId, RightType.Tenant, Permission.FullCreatorOwner);
        }
    }
}