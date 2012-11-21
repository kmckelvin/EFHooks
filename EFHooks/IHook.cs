namespace EFHooks
{
	/// <summary>
	/// A 'hook' usable for calling at certain point in an entities life cycle.
	/// </summary>
    public interface IHook
    {
		/// <summary>
		/// Executes the logic in the hook.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="metadata">The metadata.</param>
        void HookObject(object entity, HookEntityMetadata metadata);
    }
}