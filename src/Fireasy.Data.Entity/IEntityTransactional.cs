using System.Data;

namespace Fireasy.Data.Entity
{
    public interface IEntityTransactional
    {
        void BeginTransaction(IsolationLevel level);

        void CommitTransaction();

        void RollbackTransaction();
    }
}
