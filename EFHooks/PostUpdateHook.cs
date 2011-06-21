using System.Data;

namespace EFHooks
{
    public abstract class PostUpdateHook<TEntity> : PostActionHook<TEntity>
    {
        public override EntityState HookStates
        {
            get { return EntityState.Modified; }
        }
    }
}