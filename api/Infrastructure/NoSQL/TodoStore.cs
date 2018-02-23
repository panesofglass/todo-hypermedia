﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Domain.Models;
using Domain.Persistence;

namespace Infrastructure.Db
{
    public class TodoStore : ITodoStore
    {
        private readonly IAmazonDynamoDB _client;
        private readonly IDynamoDBContext _context;

        public const string TableName = "Todo";
        private const string HashKey = "Id";

        public TodoStore(IAmazonDynamoDB client, IDynamoDBContext context)
        {
            _client = client;
            _context = context;
        }

        public async Task<TableDescription> BuildOrDescribeTable()
        {
            var request = new CreateTableRequest(
                tableName: TableName,
                keySchema: new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = HashKey,
                        KeyType = KeyType.HASH
                    }
                },
                attributeDefinitions: new List<AttributeDefinition>
                {
                    new AttributeDefinition()
                    {
                        AttributeName = HashKey,
                        AttributeType = ScalarAttributeType.S
                    }
                },
                provisionedThroughput: new ProvisionedThroughput
                {
                    ReadCapacityUnits = 1,
                    WriteCapacityUnits = 1
                }
            );
            Console.WriteLine("Sending request to build table...");
            try
            {
                var result = await _client.CreateTableAsync(request);
                Console.WriteLine("Table created.");
                return result.TableDescription;
            }
            catch (ResourceInUseException)
            {
                // Table already created, just describe it
                Console.WriteLine("Table already exists. Fetching description...");
                var result = await _client.DescribeTableAsync(TableName);
                Console.WriteLine($"Using table: {result.Table.TableName} ");
                return result.Table;
            }
        }

        public async Task<string> Create(TodoCreateData todo)
        {
            var id = Guid.NewGuid().ToString();

            var create = new Todo
            {
                Id = id,
                Name = todo.Name,
                Completed = todo.Completed,
                Due = todo.Due,
                CreatedAt = DateTime.UtcNow
            };

            await _context.SaveAsync<Todo>(create);

            return id;
        }

        public async Task<Todo> GetById(string id)
        {
            List<ScanCondition> conditions =
                new List<ScanCondition> {new ScanCondition(HashKey, ScanOperator.Equal, id)};
            var allDocs = await _context.ScanAsync<Todo>(conditions).GetRemainingAsync();
            return allDocs.FirstOrDefault();
        }
    }
}