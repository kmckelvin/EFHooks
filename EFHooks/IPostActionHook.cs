using System;
using System.Data;

namespace EFHooks
{
	/// <summary>
	/// A hook that is executed after an action.
	/// </summary>
    public interface IPostActionHook : IHook
    {
		/// <summary>
		/// Gets the entity state(s) to listen for.
		/// </summary>
		/// <remarks>The entity state being <see cref="FlagsAttribute"/>, it allows this hook to listen to multiple states.</remarks>
        EntityState HookStates { get; }
    }
}