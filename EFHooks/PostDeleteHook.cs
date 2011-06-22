using System.Data;

namespace EFHooks
{
    /// <summary>
    /// Implements a hook that will run after an entity gets deleted from the database.
    /// </summary>
    public abstract class PostDeleteHook<TEntity> : PostActionHook<TEntity>
    {
        public override EntityState HookStates
        {
            get { return EntityState.Deleted; }
        }
    }
}