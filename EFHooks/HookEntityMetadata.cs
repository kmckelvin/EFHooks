using System.Data;

namespace EFHooks
{
	/// <summary>
	/// Contains entity state, and an indication wether is has been changed.
	/// </summary>
	public class HookEntityMetadata
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="HookEntityMetadata" /> class.
		/// </summary>
		/// <param name="state">The state.</param>
		/// <param name="context">The optional existing context (I believe this is usable for migrations).</param>
		public HookEntityMetadata(EntityState state, HookedDbContext context = null)
		{
			_state = state;
			CurrentContext = context;
		}

		private EntityState _state;
		/// <summary>
		/// Gets or sets the state.
		/// </summary>
		/// <value>
		/// The state.
		/// </value>
		public EntityState State
		{
			get { return this._state; }
			set
			{
				if (_state != value)
				{
					this._state = value;
					HasStateChanged = true;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has state changed.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance has state changed; otherwise, <c>false</c>.
		/// </value>
		public bool HasStateChanged { get; private set; }

		/// <summary>
		/// Container for wrapped context?
		/// </summary>
		/// <value>
		/// The current context.
		/// </value>
		public HookedDbContext CurrentContext { get; private set; }
	}
}