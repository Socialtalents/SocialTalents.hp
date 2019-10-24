﻿using SocialTalents.Hp.MongoDB.Exceptions;
using System.Linq;

namespace SocialTalents.Hp.MongoDB
{
    /// <summary>
    /// In-memory repository for <see cref="TypedIdMongoDocument{T}"/>
    /// </summary>
    /// <typeparam name="T">Document type.</typeparam>
    public class InMemoryTypedIdRepository<T>: InMemoryRepository<T, Id<T>>
        where T: TypedIdMongoDocument<T>
    {
        public InMemoryTypedIdRepository(bool allowOptimisticLock = true) : base()
        {
            AllowOptimistickLock = allowOptimisticLock;
        }

        /// <summary>
        /// Shows if optimistic lock is engaged
        /// </summary>
        public bool AllowOptimistickLock { get; private set; }

        public override void Replace(T entity)
        {
            if (AllowOptimistickLock)
            {
                var id = entity.Id;
                var lastUpdated = entity.LastUpdated.ToUniversalTime();

                var existingItem = this.FirstOrDefault(e => e.Id == id && e.LastUpdated.ToUniversalTime() == lastUpdated);

                if (existingItem == null)
                {
                    var count = this.Count(x => x.Id == id);

                    if (count > 0)
                    {
                        throw new OptimistickLockException(this.ElementType.FullName);
                    }
                }
            }

            base.Replace(entity);
        }
    }
}