﻿using System.Threading.Tasks;
using Api.Web;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Persistence;
using Domain.Representation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Toolkit;
using Toolkit.Representation.Forms;
using Toolkit.Representation.LinkedRepresentation;

namespace Api.Controllers
{
    [Route("todo")]
    [Authorize]
    public class TodoController : Controller
    {
        private readonly ITagStore _tagStore;
        private readonly ITodoStore _todoStore;

        public TodoController(ITodoStore todoStore, ITagStore tagStore)
        {
            _todoStore = todoStore;
            _tagStore = tagStore;
        }

        [HttpGet("", Name = TodoUriFactory.SelfRouteName)]
        public async Task<FeedRepresentation> GetAll()
        {
            return (await _todoStore
                    .GetAll())
                .ToFeedRepresentation(Url);
        }

        [HttpGet("form/create", Name = TodoUriFactory.CreateFormRouteName)]
        public CreateFormRepresentation GetCreateForm()
        {
            return Url.ToTodoCreateFormRepresentation();
        }


        [HttpPost]
        public async Task<CreatedResult> Create([FromBody] TodoCreateDataRepresentation todo)
        {
            return (await _todoStore.Create(todo
                    .ThrowInvalidDataExceptionIfNull("Invalid todo create data")
                    .FromRepresentation(Url)))
                .MakeTodoUri(Url)
                .MakeCreated();
        }

        [HttpGet("{id}", Name = TodoUriFactory.TodoRouteName)]
        public async Task<TodoRepresentation> GetById(string id)
        {
            var todo = await _todoStore
                .Get(id);

            return todo
                .ThrowObjectNotFoundExceptionIfNull("todo not found")
                .ToRepresentation(Url);
        }

        [HttpPut("{id}", Name = TodoUriFactory.TodoRouteName)]
        public async Task<IActionResult> Update(string id, [FromBody] TodoRepresentation item)
        {
            await _todoStore.Update(id,
                todo =>
                {
                    todo.Name = item.Name
                        .ThrowInvalidDataExceptionIfNullOrWhiteSpace("A todo must have a name");

                    todo.State = item.State;
                    todo.Due = item.Due;
                });
            return NoContent();
        }

        /// <summary>
        ///     A public stateless edit form that is fully cacheable.
        /// </summary>
        [HttpGet("form/edit", Name = TodoUriFactory.EditFormRouteName)]
        public FormRepresentation GetEditForm()
        {
            return Url.ToTodoEditFormRepresentation();
        }

        [HttpDelete("{id}", Name = TodoUriFactory.TodoRouteName)]
        public async Task<IActionResult> Delete(string id)
        {
            await _todoStore.Delete(id);
            return NoContent();
        }

        ////////////////////////////////////////////////
        /// 
        //  The tags on the todo collection
        //  ===============================

        [HttpGet("{id}/tag/", Name = TagUriFactory.TodoTagsRouteName)]
        public async Task<FeedRepresentation> GetTodoTags(string id)
        {
            var todo = await _todoStore.Get(id);

            return (await _tagStore.Get(todo.Tags))
                .ToFeedRepresentation(id, Url);
        }

        [HttpPost("{id}/tag/", Name = TagUriFactory.TodoTagCreateRouteName)]
        public async Task<CreatedResult> CreateTag([FromBody] TagCreateDataRepresentation tag, string id)
        {
            var tagId = await _tagStore.Create(tag
                .ThrowInvalidDataExceptionIfNull("Invalid tag create data")
                .FromRepresentation());

            await _todoStore.AddTag(id, tagId);

            return tagId
                .MakeTodoTagUri(id, Url)
                .MakeCreated();
        }

        [HttpGet("{id}/tag/form/create", Name = TagUriFactory.CreateFormRouteName)]
        [AllowAnonymous]
        public CreateFormRepresentation GetCreateForm(string id)
        {
            return id.ToTagCreateFormRepresentation(Url);
        }

        [HttpGet("{id}/tag/{tagId}", Name = TagUriFactory.TodoTagRouteName)]
        public async Task<TagRepresentation> Get(string id, string tagId)
        {
            var todo = await _todoStore.Get(id)
                .ThrowInvalidDataExceptionIfNull($"Todo not found '{id}'");

            todo.Tags
                .ThrowObjectNotFoundExceptionIfNull()
                .ThrowObjectNotFoundExceptionIf(tags => tags.Contains(tagId), $"Tag not found '{tagId}'");

            return (await _tagStore
                    .Get(tagId))
                .ToTodoRepresentation(id, Url);
        }

        [HttpDelete("{id}/tag/{tagId}", Name = TagUriFactory.TodoTagRouteName)]
        public async Task<IActionResult> DeleteTag(string id, string tagId)
        {
            await _todoStore.DeleteTag(id, tagId);
            return NoContent();
        }
    }
}