using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.MongoDB
{
    public interface IDirectAccessRepository<TBase, out TChild> : IRepositoryEx<TBase, TChild> where TChild : TBase
    {
        IMongoCollection<TBase> Collection { get; }
    }
}
