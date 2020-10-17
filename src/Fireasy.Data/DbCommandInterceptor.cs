using System.Threading.Tasks;

namespace Fireasy.Data
{
    public class DbCommandInterceptor
    {
        public virtual void OnExecuteNonQuery()
        {

        }

        public virtual async Task OnExecuteNonQueryAsync()
        {

        }

        public virtual void OnExecuteReader()
        {

        }

        public virtual async Task OnExecuteReaderAsync()
        {

        }

        public virtual void OnExecuteScalar()
        {

        }

        public virtual async Task OnExecuteScalarAsync()
        {

        }
    }
}
