﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain.Models;
using Domain.Persistence;
using NLog;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public class TodoStore : ITodoStore
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        private readonly IDynamoDBContext _context;
        private readonly UserRightStore _userRightStore;
        private readonly string _userId;

        public TodoStore(IDynamoDBContext context, string userId)
            : this(context, new UserRightStore(context), userId)
        {
        }

        public TodoStore(IDynamoDBContext context, UserRightStore userRightStore, string userId)
        {
            _context = context;
            _userRightStore = userRightStore;
            _userId = userId;
        }

        public async Task<string> Create(TodoCreateData todo)
        {
            var id = IdGenerator.New();
            var create = new Todo
            {
                Id = id,
                Name = todo.Name,
                State = todo.State,
                Due = todo.Due,
                CreatedAt = DateTime.UtcNow,
                // TODO: validation/cross checking of tag references
                Tags = todo.Tags
            };

            await _context.SaveAsync(create);

            return id;
        }

        public Task<string> Create(string userId, string contextId, Permission contextRights, TodoCreateData todo)
        {
            throw new NotImplementedException();
        }

        public async Task<string> Create(
            string todoCollectionId /* in this case is a userId */,
            TodoCreateData data,
            Permission callerRights,
            IDictionary<RightType, Permission> callerCollectionRights)
        {
            var todoId = await Create(data);

            await _userRightStore.CreateRights(
                _userId,
                todoId,
                RightType.Todo.MakeCreateRights(callerRights, callerCollectionRights),
                new InheritForm
                {
                    Type = RightType.UserTodoCollection,
                    ResourceId = todoCollectionId,
                    InheritedTypes = new List<RightType>
                    {
                        RightType.Todo,
                        RightType.TodoCommentCollection,
                        RightType.TodoTagCollection
                    }
                });

            return todoId;
        }

        public async Task<Todo> Get(string id)
        {
            return await _context.WhereById<Todo>(id);
        }

        public async Task<Todo> GetByIdAndTag(string id, string tagId)
        {
            return (await _context.Where<Todo>(new List<ScanCondition>
                {
                    new ScanCondition(HashKeyConstants.Default, ScanOperator.Equal, id),
                    new ScanCondition(nameof(Todo.Tags), ScanOperator.Contains, tagId)
                }))
                .FirstOrDefault();
        }

        public async Task<IEnumerable<Todo>> GetAll()
        {
            return await _context.Where<Todo>();
        }

        public async Task<IEnumerable<Todo>> GetByUser()
        {
            // TODO: implement by user
            return await GetAll();
        }


        public async Task Update(string id, Action<Todo> updater)
        {
            var todo = await Get(id)
                .ThrowObjectNotFoundExceptionIfNull();

            updater(todo);

            // no messing with the ID allowed
            todo.Id = id;
            // no messing with the update time allowed
            todo.UpdatedAt = DateTime.UtcNow;

            // if tags have been removed, it looks like you can't hand
            // though an empty list but rather need to null it.
            // TODO: check this is true
            todo.Tags = !todo.Tags.IsNullOrEmpty() ? todo.Tags : null;

            await _context.SaveAsync(todo);
        }

        /// <summary>
        ///     Add tags to the todo. You are not able to add duplicates.
        /// </summary>
        public async Task AddTag(string id, string tagId, Action<string> add = null)
        {
            await Update(id, todo =>
            {
                todo.Tags = todo.Tags ?? new List<string>();

                // do not allow duplicates
                if (!todo.Tags.Contains(tagId))
                {
                    todo.Tags.Add(tagId);
                }
            });
        }

        public async Task Delete(string id)
        {
            var todo = await Get(id)
                .ThrowObjectNotFoundExceptionIfNull();


            await _context.DeleteAsync(todo);
        }

        public async Task DeleteTag(string id, string tagId, Action<string> remove = null)
        {
            await Update(id, todo => todo.Tags?.RemoveAll(tag => tag == tagId));
        }

        public async Task<IEnumerable<Todo>> GetByTag(string tagId)
        {
            return await _context.Where<Todo>(new ScanCondition(nameof(Todo.Tags), ScanOperator.Contains, tagId));
        }
    }
}