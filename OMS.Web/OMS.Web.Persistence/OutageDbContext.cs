using System.Data.Entity;

namespace OMS.Web.Persistence
{
    public class OutageDbContext : DbContext
    {
        public OutageDbContext(): base("name=OutageDbConnection")
        {
        }
    }
}
