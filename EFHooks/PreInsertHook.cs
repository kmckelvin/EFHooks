using System.Data;

namespace EFHooks
{
    /// <summary>
    /// Implements a hook that will run before an entity gets inserted into the database.
    /// </summary>
    public abstract class PreInsertHook<TEntity> : PreActionHook<TEntity>
    {
        public override EntityState HookStates
        {
            get { return EntityState.Added; }
        }
    }
}