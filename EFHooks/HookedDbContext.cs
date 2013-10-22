using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Common;
using System.Linq;

namespace EFHooks
{
	/// <summary>
	/// An Entity Framework DbContext that can be hooked into by registering EFHooks.IHook objects.
	/// </summary>
	public abstract class HookedDbContext : DbContext
	{
		/// <summary>
		/// The pre-action hooks.
		/// </summary>
		protected IList<IPreActionHook> PreHooks;
		/// <summary>
		/// The post-action hooks.
		/// </summary>
		protected IList<IPostActionHook> PostHooks;

		/// <summary>
		/// Initializes a new instance of the <see cref="HookedDbContext" /> class, initializing empty lists of hooks.
		/// </summary>
		public HookedDbContext()
		{
			PreHooks = new List<IPreActionHook>();
			PostHooks = new List<IPostActionHook>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HookedDbContext" /> class, filling <see cref="PreHooks"/> and <see cref="PostHooks"/>.
		/// </summary>
		/// <param name="hooks">The hooks.</param>
		public HookedDbContext(IHook[] hooks)
		{
			PreHooks = hooks.OfType<IPreActionHook>().ToList();
			PostHooks = hooks.OfType<IPostActionHook>().ToList();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HookedDbContext" /> class, using the specified <paramref name="nameOrConnectionString"/>, initializing empty lists of hooks.
		/// </summary>
		/// <param name="nameOrConnectionString">The name or connection string.</param>
		public HookedDbContext(string nameOrConnectionString)
			: base(nameOrConnectionString)
		{
			PreHooks = new List<IPreActionHook>();
			PostHooks = new List<IPostActionHook>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HookedDbContext" /> class, using the specified <paramref name="nameOrConnectionString"/>, , filling <see cref="PreHooks"/> and <see cref="PostHooks"/>.
		/// </summary>
		/// <param name="hooks">The hooks.</param>
		/// <param name="nameOrConnectionString">The name or connection string.</param>
		public HookedDbContext(IHook[] hooks, string nameOrConnectionString)
			: base(nameOrConnectionString)
		{
			PreHooks = hooks.OfType<IPreActionHook>().ToList();
			PostHooks = hooks.OfType<IPostActionHook>().ToList();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HookedDbContext" /> class using the an existing connection to connect 
		/// to a database. The connection will not be disposed when the context is disposed. (see <see cref="DbContext"/> overloaded constructor)
		/// </summary>
		/// <param name="existingConnection">An existing connection to use for the new context.</param>
		/// <param name="contextOwnsConnection">If set to true the connection is disposed when the context is disposed, otherwise the caller must dispose the connection.</param>
		/// <remarks>Main reason for allowing this, is to enable reusing another database connection. (For instance one that is profiled by Miniprofiler (http://miniprofiler.com/)).</remarks>
		public HookedDbContext(DbConnection existingConnection, bool contextOwnsConnection)
			: base(existingConnection, contextOwnsConnection)
		{
			PreHooks = new List<IPreActionHook>();
			PostHooks = new List<IPostActionHook>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HookedDbContext" /> class using the an existing connection to connect 
		/// to a database. The connection will not be disposed when the context is disposed. (see <see cref="DbContext"/> overloaded constructor)
		/// </summary>
		/// <param name="hooks">The hooks.</param>
		/// <param name="existingConnection">An existing connection to use for the new context.</param>
		/// <param name="contextOwnsConnection">If set to true the connection is disposed when the context is disposed, otherwise the caller must dispose the connection.</param>
		/// <remarks>Main reason for allowing this, is to enable reusing another database connection. (For instance one that is profiled by Miniprofiler (http://miniprofiler.com/)).</remarks>
		public HookedDbContext(IHook[] hooks, DbConnection existingConnection, bool contextOwnsConnection)
			: base(existingConnection, contextOwnsConnection)
		{
			PreHooks = hooks.OfType<IPreActionHook>().ToList();
			PostHooks = hooks.OfType<IPostActionHook>().ToList();
		}
		/// <summary>
		/// Registers a hook to run before a database action occurs.
		/// </summary>
		/// <param name="hook">The hook to register.</param>
		public void RegisterHook(IPreActionHook hook)
		{
			this.PreHooks.Add(hook);
		}

		/// <summary>
		/// Registers a hook to run after a database action occurs.
		/// </summary>
		/// <param name="hook">The hook to register.</param>
		public void RegisterHook(IPostActionHook hook)
		{
			this.PostHooks.Add(hook);
		}

		/// <summary>
		/// Saves all changes made in this context to the underlying database.
		/// </summary>
		/// <returns>
		/// The number of objects written to the underlying database.
		/// </returns>
		public override int SaveChanges()
		{
			var modifiedEntries = this.ChangeTracker.Entries()
											.Where(x => x.State != EntityState.Unchanged && x.State != EntityState.Detached)
											.Select(x => new HookedEntityEntry()
											{
												Entity = x.Entity,
												PreSaveState = x.State
											})
											.ToArray();

			ExecutePreActionHooks(modifiedEntries, false);//Regardless of validation (executing the hook possibly fixes validation errors)

			var hasValidationErrors = this.Configuration.ValidateOnSaveEnabled && this.ChangeTracker.Entries().Any(x => x.State != EntityState.Unchanged && !x.GetValidationResult().IsValid);
			var hasPostHooks = this.PostHooks.Any(); // Save this to a local variable since we're checking this again later.

			if (!hasValidationErrors)
			{
				ExecutePreActionHooks(modifiedEntries, true);
			}

			var result = base.SaveChanges();

			if (hasPostHooks)
			{
				foreach (var entityEntry in modifiedEntries)
				{
					var entry = entityEntry;

					//Obtains hooks that 'listen' to one or more Entity States
					foreach (var hook in PostHooks.Where(x => (x.HookStates & entry.PreSaveState) == entry.PreSaveState))
					{
						var metadata = new HookEntityMetadata(entityEntry.PreSaveState, this);
						hook.HookObject(entityEntry.Entity, metadata);
					}
				}
			}

			return result;
		}
		/// <summary>
		/// Executes the pre action hooks, filtered by <paramref name="requiresValidation"/>.
		/// </summary>
		/// <param name="modifiedEntries">The modified entries to execute hooks for.</param>
		/// <param name="requiresValidation">if set to <c>true</c> executes hooks that require validation, otherwise executes hooks that do NOT require validation.</param>
		private void ExecutePreActionHooks(IEnumerable<HookedEntityEntry> modifiedEntries, bool requiresValidation)
		{
			foreach (var entityEntry in modifiedEntries)
			{
				var entry = entityEntry; //Prevents access to modified closure

				foreach (
					var hook in
						PreHooks.Where(x => (x.HookStates & entry.PreSaveState) == entry.PreSaveState
											&& x.RequiresValidation == requiresValidation))
				{
					var metadata = new HookEntityMetadata(entityEntry.PreSaveState, this);
					hook.HookObject(entityEntry.Entity, metadata);

					if (metadata.HasStateChanged)
					{
						entityEntry.PreSaveState = metadata.State;
					}
				}
			}
		}
	}
}