using System.Data;

namespace EFHooks
{
    /// <summary>
    /// Implements a hook that will run before an entity gets updated in the database.
    /// </summary>
    public abstract class PreUpdateHook<TEntity> : PreActionHook<TEntity>
    {
        public override EntityState HookStates
        {
            get { return EntityState.Modified; }
        }
    }
}