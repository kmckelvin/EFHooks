using System.Data;

namespace EFHooks
{
    public abstract class PostDeleteHook<TEntity> : PostActionHook<TEntity>
    {
        public override EntityState HookStates
        {
            get { return EntityState.Deleted; }
        }
    }
}