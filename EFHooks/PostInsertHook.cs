using System.Data;

namespace EFHooks
{
    public abstract class PostInsertHook<TEntity> : PostActionHook<TEntity>
    {
        public override EntityState HookStates
        {
            get { return EntityState.Added; }
        }
    }
}