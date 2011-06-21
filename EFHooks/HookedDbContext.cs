using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace EFHooks
{
    public class HookedDbContext : DbContext
    {
        private List<IPreActionHook> _preHooks;

        public HookedDbContext(IEnumerable<IHook> hooks)
        {
            _preHooks = hooks.OfType<IPreActionHook>().ToList();
        }

        protected override System.Data.Entity.Validation.DbEntityValidationResult ValidateEntity(System.Data.Entity.Infrastructure.DbEntityEntry entityEntry, System.Collections.Generic.IDictionary<object, object> items)
        {
            var result = base.ValidateEntity(entityEntry, items);

            //todo - only hook if it has passed validation

            foreach (var hook in _preHooks)
            {
                var metadata = new HookEntityMetadata(entityEntry.State);
                hook.Hook(entityEntry.Entity, metadata);

                if (metadata.HasStateChanged)
                {
                    entityEntry.State = metadata.State;
                }
            }

            return result;
        }
    }
}