using System.Data;

namespace EFHooks
{
    public abstract class PreDeleteHook<TEntity> : PreActionHook<TEntity>
    {
        public override EntityState HookStates
        {
            get { return EntityState.Deleted; }
        }
    }
}