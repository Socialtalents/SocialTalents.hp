using MongoDB.Driver;
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
    public class MongoRepository<TBase, TChild, TId>: IDirectAccessRepository<TBase, TChild> 
        where TChild : TBase
        where TBase: BaseMongoDocument<TId>
        where TId: struct, IEquatable<TId>, IComparable<TId>
    {
        public MongoRepository(IMongoDatabase database, string collectionName = null, MongoCollectionSettings settings = null)
        {
            if (collectionName != null && typeof(TId).TypeIs(typeof(Id<>)))
            {
                collectionName = typeof(TId).GenericTypeArguments.First().Name;
            }
            CollectionName = collectionName ?? typeof(TBase).Name;
            Collection = GetCollection(database, settings);
        }

        public string CollectionName { get; private set; }

        protected IMongoCollection<TBase> GetCollection(IMongoDatabase database, MongoCollectionSettings settings)
        {
            return database.GetCollection<TBase>(CollectionName, settings ?? new MongoCollectionSettings());
        }

        public IMongoCollection<TBase> Collection { get; private set; }

        public virtual void Insert(TBase entity)
        {
            RaiseOnBeforeInsert(entity);
            entity.LastUpdated = DateTime.Now;
            Collection.InsertOne(entity);
            RaiseOnInsert(entity);
        }

        public virtual void Replace(TBase entity)
        {
            RaiseOnBeforeReplace(entity);
            entity.LastUpdated = DateTime.Now;
            var id = entity.Id;
            Collection.ReplaceOne(x => x.Id.Equals(id), entity);
            RaiseOnReplace(entity);
        }

        public virtual void Delete(TBase entity)
        {
            RaiseOnBeforeDelete(entity);
            Collection.DeleteOne(x => x.Id.Equals(entity.Id));
            RaiseOnDelete(entity);
        }

        public virtual void DeleteMany(Expression<Func<TBase, bool>> query)
        {
            RaiseOnBeforeDeleteMany(query);
            Collection.DeleteMany(query);
            RaiseOnDeleteMany(query);
        }

        public IQueryable<TBase> AsQueryable() => Collection.AsQueryable();

        public event EntitySaveDelegate<TBase> OnBeforeInsert;
        public event EntitySaveDelegate<TBase> OnInsert;
        public event EntitySaveDelegate<TBase> OnBeforeReplace;
        public event EntitySaveDelegate<TBase> OnReplace;
        public event EntityDeleteDelegate<TBase> OnBeforeDelete;
        public event EntityDeleteDelegate<TBase> OnDelete;
        public event EntityDeleteManyDelegate<TBase> OnBeforeDeleteMany;
        public event EntityDeleteManyDelegate<TBase> OnDeleteMany;

        protected void RaiseOnBeforeInsert(TBase t) => OnBeforeInsert?.Invoke(t);
        protected void RaiseOnInsert(TBase t) => OnInsert?.Invoke(t);
        protected void RaiseOnBeforeReplace(TBase t) => OnBeforeReplace?.Invoke(t);
        protected void RaiseOnReplace(TBase t) => OnReplace?.Invoke(t);
        protected void RaiseOnBeforeDelete(TBase t) => OnBeforeDelete?.Invoke(t);
        protected void RaiseOnDelete(TBase t) => OnDelete?.Invoke(t);
        protected void RaiseOnBeforeDeleteMany(Expression<Func<TBase, bool>> tester) => OnBeforeDeleteMany?.Invoke(tester);
        protected void RaiseOnDeleteMany(Expression<Func<TBase, bool>> tester) => OnDeleteMany?.Invoke(tester);

        public Expression Expression => Collection.AsQueryable().Expression;

        public Type ElementType => typeof(TBase);

        public IQueryProvider Provider => Collection.AsQueryable().Provider;

        public IEnumerator<TChild> GetEnumerator() {
            var baseEnumerator = Collection.AsQueryable().GetEnumerator();
            return baseEnumerator.Cast<TBase, TChild>();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Mongo repository for <see cref="BaseMongoDocument"/>
    /// </summary>
    /// <typeparam name="T">Document type.</typeparam>
    public class MongoRepository<T>: MongoRepository<T, T, ObjectId>
        where T: BaseMongoDocument
    {
        public MongoRepository(IMongoDatabase database, string collectionName = null, MongoCollectionSettings settings = null):
            base(database, collectionName, settings) { }
    }
}