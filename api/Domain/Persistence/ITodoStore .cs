﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Persistence
{
    public interface ITodoStore
    {
        /// <summary>
        ///     Create a new <see cref="Todo"/>. Any <see cref="Todo.Tags"/> have each <see cref="Tag.Count"/> incremented on the
        ///     global collection.
        /// </summary>
        /// <param name="todo"></param>
        Task<string> Create(TodoCreateData todo);
        
        /// <summary>
        ///     Retrieve a <see cref="Todo"/> based on its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Todo> Get(string id);
        
        /// <summary>
        ///     Retrieve a full set of tags (ie the global collection) regardless of <see cref="Tenant"/>
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Todo>> GetAll();
        
        /// <summary>
        ///     Update details of a <see cref="Todo"/> that includes checking if the <see cref="Todo.Tags"/> have changed
        ///     and alter the global <see cref="Tag.Count"/> (increment or decrement)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updater"></param>
        Task Update(string id, Action<Todo> updater);
 
        /// <summary>
        ///     Add an existing <see cref="Tag"/> ti a <see cref="Todo"/>. 
        /// </summary>
        /// <param name="id"><see cref="Todo.Id"/> of the todo that has the tags</param>
        /// <param name="tagId"><see cref="Tag.Id"/> of the tag to be added</param>
        /// <param name="add">Callback on the <see cref="Tag.Id"/>. The default action is to increment the <see cref="Tag.Count"/> on the global collection of tags</param>
        Task AddTag(string id, string tagId, Action<string> add = null);
        
        /// <summary>
        ///     Remove a <see cref="Todo"/> from the collection for and decrement <see cref="Tag.Count"/> on the global
        ///     collection
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task Delete(string id);

        /// <summary>
        ///     Remove a <see cref="Tag"/> from a <see cref="Todo"/>. 
        /// </summary>
        /// <param name="id"><see cref="Todo.Id"/> of the todo that has the tags</param>
        /// <param name="tagId"><see cref="Tag.Id"/> of the tag to be deleted</param>
        /// <param name="remove">Callback on the <see cref="Tag.Id"/>. The default action is to decrement the <see cref="Tag.Count"/> on the global collection of tags</param>
        Task DeleteTag(string id, string tagId, Action<string> remove = null);
    }
}