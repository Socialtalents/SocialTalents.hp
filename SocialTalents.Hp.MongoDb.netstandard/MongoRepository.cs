﻿using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;

namespace SocialTalents.Hp.MongoDB
{
    /// <summary>
    /// Base class for Mongo repositories for <see cref="BaseMongoDocument{TId}"/>
    /// </summary>
    /// <typeparam name="T">Document type.</typeparam>
    /// <typeparam name="TId">Document id type.</typeparam>
    public abstract class MongoRepository<T, TId>: IDirectAccessRepository<T>
        where T: BaseMongoDocument<TId>
        where TId: struct, IEquatable<TId>, IComparable<TId>
    {
        public MongoRepository(IMongoDatabase database, string collectionName = null, MongoCollectionSettings settings = null)
        {
            CollectionName = collectionName ?? typeof(T).Name;
            Collection = GetCollection(database, settings);
        }

        public string CollectionName { get; private set; }

        protected IMongoCollection<T> GetCollection(IMongoDatabase database, MongoCollectionSettings settings)
        {
            return database.GetCollection<T>(CollectionName, settings ?? new MongoCollectionSettings());
        }

        public IMongoCollection<T> Collection { get; private set; }

        public virtual void Insert(T entity)
        {
            RaiseOnBeforeInsert(entity);
            entity.LastUpdated = DateTime.UtcNow;
            Collection.InsertOne(entity);
            RaiseOnInsert(entity);
        }

        public virtual void Replace(T entity)
        {
            RaiseOnBeforeReplace(entity);
            entity.LastUpdated = DateTime.UtcNow;
            var id = entity.Id;
            Collection.ReplaceOne(x => x.Id.Equals(id), entity);
            RaiseOnReplace(entity);
        }

        public virtual void Delete(T entity)
        {
            RaiseOnBeforeDelete(entity);
            Collection.DeleteOne(x => x.Id.Equals(entity.Id));
            RaiseOnDelete(entity);
        }

        public virtual void DeleteMany(Expression<Func<T, bool>> query)
        {
            RaiseOnBeforeDeleteMany(query);
            Collection.DeleteMany(query);
            RaiseOnDeleteMany(query);
        }

        public IQueryable<T> AsQueryable() => Collection.AsQueryable();

        public event EntitySaveDelegate<T> OnBeforeInsert;
        public event EntitySaveDelegate<T> OnInsert;
        public event EntitySaveDelegate<T> OnBeforeReplace;
        public event EntitySaveDelegate<T> OnReplace;
        public event EntityDeleteDelegate<T> OnBeforeDelete;
        public event EntityDeleteDelegate<T> OnDelete;
        public event EntityDeleteManyDelegate<T> OnBeforeDeleteMany;
        public event EntityDeleteManyDelegate<T> OnDeleteMany;

        protected void RaiseOnBeforeInsert(T t) => OnBeforeInsert?.Invoke(t);
        protected void RaiseOnInsert(T t) => OnInsert?.Invoke(t);
        protected void RaiseOnBeforeReplace(T t) => OnBeforeReplace?.Invoke(t);
        protected void RaiseOnReplace(T t) => OnReplace?.Invoke(t);
        protected void RaiseOnBeforeDelete(T t) => OnBeforeDelete?.Invoke(t);
        protected void RaiseOnDelete(T t) => OnDelete?.Invoke(t);
        protected void RaiseOnBeforeDeleteMany(Expression<Func<T, bool>> tester) => OnBeforeDeleteMany?.Invoke(tester);
        protected void RaiseOnDeleteMany(Expression<Func<T, bool>> tester) => OnDeleteMany?.Invoke(tester);

        public Expression Expression => Collection.AsQueryable().Expression;

        public Type ElementType => typeof(T);

        public IQueryProvider Provider => Collection.AsQueryable().Provider;

        public IEnumerator<T> GetEnumerator() => Collection.AsQueryable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Mongo repository for <see cref="BaseMongoDocument"/>
    /// </summary>
    /// <typeparam name="T">Document type.</typeparam>
    public class MongoRepository<T>: MongoRepository<T, ObjectId>
        where T: BaseMongoDocument
    {
        public MongoRepository(IMongoDatabase database, string collectionName = null, MongoCollectionSettings settings = null):
            base(database, collectionName, settings) { }
    }
}