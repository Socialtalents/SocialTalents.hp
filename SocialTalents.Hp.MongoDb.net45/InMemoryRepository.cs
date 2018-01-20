using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.MongoDB
{
    public class InMemoryRepository<T> : IRepository<T> where T : BaseMongoDocument
    {
        private readonly List<T> _entities;

        public InMemoryRepository(List<T> entities = null)
        {
            _entities = entities ?? new List<T>();
        }

        public void Insert(T entity)
        {
            RaiseOnBeforeInsert(entity);
            entity.LastUpdated = DateTime.Now;

            if (entity.Id == ObjectId.Empty)
            {
                entity.Id = ObjectId.GenerateNewId();
            }
            _entities.Add(entity);
            RaiseOnInsert(entity);
        }

        public void Replace(T entity)
        {
            RaiseOnBeforeReplace(entity);
            entity.LastUpdated = DateTime.Now;
            DeleteIfFound(entity);
            _entities.Add(entity);
            RaiseOnReplace(entity);
        }

        private void DeleteIfFound(T Entity)
        {
            var existingItem = _entities.FirstOrDefault(e => e.Id == Entity.Id);
            if (existingItem != null)
                _entities.Remove(existingItem);
        }

        public void Delete(T entity)
        {
            RaiseOnBeforeDelete(entity);
            DeleteIfFound(entity);
            RaiseOnDelete(entity);
        }

        public void DeleteMany(Expression<Func<T, bool>> query)
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

        public Expression Expression
        {
            get { return _entities.AsQueryable().Expression; }
        }

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public IQueryProvider Provider { get { return _entities.AsQueryable().Provider; } }

        public IEnumerator<T> GetEnumerator()
        {
            return _entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
