using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace EFHooks
{

    /// <summary>
    /// Implements a strongly-typed hook to be run after an load from the database has been performed.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity this hook must watch for.</typeparam>
    public abstract class PostLoadHook<TEntity> : IPostLoadHook
    {
        /// <summary>
        /// Implements the interface.  This causes the hook to only run for objects that are assignable to TEntity.
        /// </summary>
        public void HookObject(object entity, HookEntityMetadata metadata)
        {
            if (typeof(TEntity).IsAssignableFrom(entity.GetType()))
            {
                Hook((TEntity)entity, metadata);
            }
        }

        /// <summary>
        /// The logic to perform per entity after the registered action gets performed.
        /// This gets run once per entity that has been changed.
        /// </summary>
        public abstract void Hook(TEntity entity, HookEntityMetadata metadata);


        /// <summary>
        /// Entity States that this hook must be registered to listen for.
        /// </summary>
        public virtual EntityState HookStates
        {
            get { return EntityState.Unchanged; }
        }
    }
}


