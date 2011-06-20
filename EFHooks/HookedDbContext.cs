using System.Data.Entity;

namespace EFHooks
{
    public class HookedDbContext : DbContext
    {
        protected override System.Data.Entity.Validation.DbEntityValidationResult ValidateEntity(System.Data.Entity.Infrastructure.DbEntityEntry entityEntry, System.Collections.Generic.IDictionary<object, object> items)
        {
            return base.ValidateEntity(entityEntry, items);
        }
    }
}