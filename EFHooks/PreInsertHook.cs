using System.Data;

namespace EFHooks
{
	/// <summary>
	/// Implements a hook that will run before an entity gets inserted into the database.
	/// </summary>
	public abstract class PreInsertHook<TEntity> : PreActionHook<TEntity>
	{
		/// <summary>
		/// Returns <see cref="EntityState.Added"/> as the hookstate to listen for.
		/// </summary>
		public override EntityState HookStates
		{
			get { return EntityState.Added; }
		}
	}
}