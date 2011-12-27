using System.Data;

namespace EFHooks
{
    public class HookEntityMetadata
    {
        public HookEntityMetadata(EntityState state, HookedDbContext context = null)
        {
            _state = state;
            CurrentContext = context;
        }

        private EntityState _state;
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

        public bool HasStateChanged { get; private set; }

        public HookedDbContext CurrentContext { get; private set; }
    }
}