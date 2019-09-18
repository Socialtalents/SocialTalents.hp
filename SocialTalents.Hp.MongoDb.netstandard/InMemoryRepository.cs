using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;

namespace SocialTalents.Hp.MongoDB
{
    /// <summary>
    /// Base class for in-memory repositories for <see cref="BaseMongoDocument{TId}"/>
    /// </summary>
    /// <typeparam name="T">Document type.</typeparam>
    /// <typeparam name="TId">Document id type.</typeparam>
    public abstract class InMemoryRepository<T, TId>: IRepository<T>
        where T: BaseMongoDocument<TId>
        where TId: struct, IEquatable<TId>, IComparable<TId>
    {
        private readonly List<T> _entities;

        public InMemoryRepository(List<T> entities = null)
        {
            _entities = entities ?? new List<T>();
        }

        public virtual void Insert(T entity)
        {
            RaiseOnBeforeInsert(entity);
            entity.LastUpdated = DateTime.Now;
            if (entity.Id.Equals(default(TId)))
                entity.Id = entity.GenerateNewId();

            _entities.Add(entity);
            RaiseOnInsert(entity);
        }

        public virtual void Replace(T entity)
        {
            RaiseOnBeforeReplace(entity);
            entity.LastUpdated = DateTime.Now;
            if (DeleteIfFound(entity))
            {
                _entities.Add(entity);
            }
            RaiseOnReplace(entity);
        }

        private bool DeleteIfFound(T entity)
        {
            var existingItem = _entities.FirstOrDefault(e => e.Id.Equals(entity.Id));
            if (existingItem != null)
            {
                _entities.Remove(existingItem);
                return true;
            }
            return false;
        }

        public virtual void Delete(T entity)
        {
            RaiseOnBeforeDelete(entity);
            DeleteIfFound(entity);
            RaiseOnDelete(entity);
        }

        public virtual void DeleteMany(Expression<Func<T, bool>> query)
        {
            RaiseOnBeforeDeleteMany(query);
            _entities.RemoveAll(query.Compile().Invoke);
            RaiseOnDeleteMany(query);

            var data = _entities.Where(query.Compile()).ToArray();
            foreach (var e in data)
            {
                RaiseOnBeforeDelete(e);
                _entities.Remove(e);
                RaiseOnDelete(e);
            }
        }

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

        public Type ElementType => typeof(T);
        public Expression Expression => _entities.AsQueryable().Expression;
        public IQueryProvider Provider => _entities.AsQueryable().Provider;

        public IEnumerator<T> GetEnumerator() => _entities.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// In-memory repository for <see cref="BaseMongoDocument"/>
    /// </summary>
    /// <typeparam name="T">Document type.</typeparam>
    public class InMemoryRepository<T>: InMemoryRepository<T, ObjectId>
        where T: BaseMongoDocument<ObjectId> { }
}