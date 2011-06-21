using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace EFHooks
{
    public class HookedDbContext : DbContext
    {
        protected IList<IPreActionHook> PreHooks;

        public HookedDbContext()
        {
            PreHooks = new List<IPreActionHook>();
        }

        public HookedDbContext(IEnumerable<IHook> hooks)
        {
            PreHooks = hooks.OfType<IPreActionHook>().ToList();
        }

        public override int SaveChanges()
        {
            bool hasValidationErrors = this.ChangeTracker.Entries().Any(x => !x.GetValidationResult().IsValid);
            if (!hasValidationErrors)
            {
                var modifiedEntries =
                    this.ChangeTracker.Entries()
                                      .Where(x => x.State != EntityState.Unchanged && x.State != EntityState.Detached)
                                      .ToArray();

                foreach (var entityEntry in modifiedEntries)
                {
                    foreach (var hook in PreHooks)
                    {
                        var metadata = new HookEntityMetadata(entityEntry.State);
                        hook.Hook(entityEntry.Entity, metadata);

                        if (metadata.HasStateChanged)
                        {
                            entityEntry.State = metadata.State;
                        }
                    }
                }
            }

            return base.SaveChanges();
        }
    }
}