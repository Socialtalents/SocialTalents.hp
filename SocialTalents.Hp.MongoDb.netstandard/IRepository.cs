using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.MongoDB
{
    public interface IRepositoryEx<TBase, out TChild>: IQueryable<TChild>, IEnumerable<TChild> where TChild : TBase
    {
        void Insert(TBase entity);
        void Replace(TBase entity);
        void Delete(TBase entity);
        void DeleteMany(Expression<Func<TBase, bool>> query);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event EntitySaveDelegate<TBase> OnBeforeInsert;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event EntitySaveDelegate<TBase> OnInsert;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event EntitySaveDelegate<TBase> OnBeforeReplace;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event EntitySaveDelegate<TBase> OnReplace;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event EntityDeleteDelegate<TBase> OnBeforeDelete;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event EntityDeleteDelegate<TBase> OnDelete;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event EntityDeleteManyDelegate<TBase> OnBeforeDeleteMany;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event EntityDeleteManyDelegate<TBase> OnDeleteMany;

    }

    public interface IRepository<TBase> : IRepositoryEx<TBase, TBase>, IQueryable<TBase>, IEnumerable<TBase> { }

    public delegate void EntitySaveDelegate<T>(T t);
    public delegate void EntityDeleteDelegate<T>(T t);
    public delegate void EntityDeleteManyDelegate<T>(Expression<Func<T,bool>> t);
}
