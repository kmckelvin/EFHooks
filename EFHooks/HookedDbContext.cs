using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace EFHooks
{
    /// <summary>
    /// An Entity Framework DbContext that can be hooked into by registering EFHooks.IHook objects.
    /// </summary>
    public abstract class HookedDbContext : DbContext
    {
        protected IList<IPreActionHook> PreHooks;
        protected IList<IPostActionHook> PostHooks;

        public HookedDbContext()
        {
            PreHooks = new List<IPreActionHook>();
            PostHooks = new List<IPostActionHook>();
        }

        public HookedDbContext(IHook[] hooks)
        {
            PreHooks = hooks.OfType<IPreActionHook>().ToList();
            PostHooks = hooks.OfType<IPostActionHook>().ToList();
        }

        public HookedDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            PreHooks = new List<IPreActionHook>();
            PostHooks = new List<IPostActionHook>();
        }

        public HookedDbContext(IHook[] hooks, string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            PreHooks = hooks.OfType<IPreActionHook>().ToList();
            PostHooks = hooks.OfType<IPostActionHook>().ToList();
        }

        /// <summary>
        /// Registers a hook to run before a database action occurs.
        /// </summary>
        /// <param name="hook">The hook to register.</param>
        public void RegisterHook(IPreActionHook hook)
        {
            this.PreHooks.Add(hook);
        }

        /// <summary>
        /// Registers a hook to run after a database action occurs.
        /// </summary>
        /// <param name="hook">The hook to register.</param>
        public void RegisterHook(IPostActionHook hook)
        {
            this.PostHooks.Add(hook);
        }

        public override int SaveChanges()
        {
            bool hasValidationErrors = this.Configuration.ValidateOnSaveEnabled && this.ChangeTracker.Entries().Any(x => x.State != EntityState.Unchanged && !x.GetValidationResult().IsValid);
            bool hasPostHooks = this.PostHooks.Any(); // save this to a local variable since we're checking this again later.

            var modifiedEntries = hasValidationErrors && !hasPostHooks
                                      ? new HookedEntityEntry[0]
                                      : this.ChangeTracker.Entries()
                                            .Where(x => x.State != EntityState.Unchanged && x.State != EntityState.Detached)
                                            .Select(x => new HookedEntityEntry()
                                                             {
                                                                 Entity = x.Entity,
                                                                 PreSaveState = x.State
                                                             })
                                            .ToArray();

            if (!hasValidationErrors)
            {
                foreach (var entityEntry in modifiedEntries)
                {
                    foreach (var hook in PreHooks.Where(x => x.HookStates == entityEntry.PreSaveState))
                    {
                        var metadata = new HookEntityMetadata(entityEntry.PreSaveState, this);
                        hook.HookObject(entityEntry.Entity, metadata);

                        if (metadata.HasStateChanged)
                        {
                            entityEntry.PreSaveState = metadata.State;
                        }
                    }
                }
            }

            int result = base.SaveChanges();

            if (hasPostHooks)
            {
                foreach (var entityEntry in modifiedEntries)
                {
                    foreach (var hook in PostHooks.Where(x => x.HookStates == entityEntry.PreSaveState))
                    {
                        var metadata = new HookEntityMetadata(entityEntry.PreSaveState, this);
                        hook.HookObject(entityEntry.Entity, metadata);
                    }
                }
            }

            return result;
        }
    }
}