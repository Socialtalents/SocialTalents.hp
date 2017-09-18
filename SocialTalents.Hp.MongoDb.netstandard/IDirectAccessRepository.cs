using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.MongoDB
{
    public interface IDirectAccessRepository<T> : IRepository<T>
    {
        IMongoCollection<T> Collection { get; }
    }
}
