﻿using Hangfire.Logging;
using Hangfire.Mongo.Database;
using Hangfire.Mongo.MongoUtils;
using Hangfire.Server;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Hangfire.Mongo
{
    /// <summary>
    /// Represents Hangfire expiration manager for Mongo database
    /// </summary>
    public class ExpirationManager : IServerComponent
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly MongoStorage _storage;
        private readonly TimeSpan _checkInterval;

        /// <summary>
        /// Constructs expiration manager with one hour checking interval
        /// </summary>
        /// <param name="storage">MongoDB storage</param>
        public ExpirationManager(MongoStorage storage)
            : this(storage, TimeSpan.FromHours(1))
        {
        }

        /// <summary>
        /// Constructs expiration manager with specified checking interval
        /// </summary>
        /// <param name="storage">MongoDB storage</param>
        /// <param name="checkInterval">Checking interval</param>
        public ExpirationManager(MongoStorage storage, TimeSpan checkInterval)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            _storage = storage;
            _checkInterval = checkInterval;
        }

        /// <summary>
        /// Run expiration manager to remove outdated records
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public void Execute(CancellationToken cancellationToken)
        {
            using (HangfireDbContext connection = _storage.CreateAndOpenConnection())
            {
                DateTime now = connection.GetServerTimeUtc();

                List<Task> processedTasks = new List<Task>
                {
                    RemoveExpiredRecord(connection.AggregatedCounter, _ => _.ExpireAt, now),
                    RemoveExpiredRecord(connection.Counter, _ => _.ExpireAt, now),
                    RemoveExpiredRecord(connection.Job, _ => _.ExpireAt, now),
                    RemoveExpiredRecord(connection.List, _ => _.ExpireAt, now),
                    RemoveExpiredRecord(connection.Set, _ => _.ExpireAt, now),
                    RemoveExpiredRecord(connection.Hash, _ => _.ExpireAt, now)
                };

                Task.WaitAll(processedTasks.ToArray());
            }

            cancellationToken.WaitHandle.WaitOne(_checkInterval);
        }

        /// <summary>
        /// Returns text representation of the object
        /// </summary>
        public override string ToString()
        {
            return "Mongo Expiration Manager";
        }

        private static async Task<long> RemoveExpiredRecord<TEntity, TField>(IMongoCollection<TEntity> collection, Expression<Func<TEntity, TField>> expression, TField now)
        {
            Logger.DebugFormat("Removing outdated records from table '{0}'...", collection.CollectionNamespace.CollectionName);

            DeleteResult result = await collection.DeleteManyAsync(Builders<TEntity>.Filter.Lt(expression, now));
            return result.DeletedCount;
        }
    }
}