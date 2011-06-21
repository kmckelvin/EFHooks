using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace EFHooks
{
    public class HookedDbContext : DbContext
    {
        private IPreActionHook[] _preHooks;
        private bool _hooksEnabled = false;

        public HookedDbContext(IEnumerable<IHook> hooks)
        {
            _preHooks = hooks.OfType<IPreActionHook>().ToArray();
        }

        protected override System.Data.Entity.Validation.DbEntityValidationResult ValidateEntity(System.Data.Entity.Infrastructure.DbEntityEntry entityEntry, System.Collections.Generic.IDictionary<object, object> items)
        {
            var result = base.ValidateEntity(entityEntry, items);

            if (_hooksEnabled && result.IsValid)
            {
                foreach (var hook in _preHooks)
                {
                    var metadata = new HookEntityMetadata(entityEntry.State);
                    hook.Hook(entityEntry.Entity, metadata);

                    if (metadata.HasStateChanged)
                    {
                        entityEntry.State = metadata.State;
                    }
                }
            }

            return result;
        }

        public override int SaveChanges()
        {
            try
            {
                _hooksEnabled = true;
                return base.SaveChanges();
            }
            finally
            {
                _hooksEnabled = false;
            }
        }
    }
}