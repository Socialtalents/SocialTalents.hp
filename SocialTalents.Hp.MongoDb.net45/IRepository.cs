using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.MongoDB
{
    public interface IRepository<T> :IQueryable<T>, IEnumerable<T>
    {
        void Insert(T entity);
        void Replace(T entity);
        void Delete(T entity);
        void DeleteMany(Expression<Func<T, bool>> query);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event EntitySaveDelegate<T> OnBeforeInsert;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event EntitySaveDelegate<T> OnInsert;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event EntitySaveDelegate<T> OnBeforeReplace;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event EntitySaveDelegate<T> OnReplace;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event EntityDeleteDelegate<T> OnBeforeDelete;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event EntityDeleteDelegate<T> OnDelete;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event EntityDeleteManyDelegate<T> OnBeforeDeleteMany;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event EntityDeleteManyDelegate<T> OnDeleteMany;

    }

    public delegate void EntitySaveDelegate<T>(T t);
    public delegate void EntityDeleteDelegate<T>(T t);
    public delegate void EntityDeleteManyDelegate<T>(Expression<Func<T,bool>> t);
}
