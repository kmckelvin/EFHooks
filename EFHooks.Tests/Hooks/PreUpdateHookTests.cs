using System;
using System.Data.Entity;
using NUnit.Framework;

namespace EFHooks.Tests.Hooks
{
    public class PreUpdateHookTests
    {
        private class TimestampPreUpdateHook : PreUpdateHook<ITimeStamped>
        {
			public override bool RequiresValidation
			{
				get { return false; }
			}

            public override void Hook(ITimeStamped entity, HookEntityMetadata metadata)
            {
                entity.ModifiedAt = DateTime.Now;
            }
        }

        [Test]
        public void PreUpdateHook_HasModifiedHookState()
        {
            var hook = new TimestampPreUpdateHook();
            Assert.AreEqual(EntityState.Modified, hook.HookStates);
        }

        [Test]
        public void PreUpdateHook_HookCallsIntoGenericMethod()
        {
            var hook = new TimestampPreUpdateHook();
            var entity = new TimestampedSoftDeletedEntity();
            ((IHook) hook).HookObject(entity, null);

            Assert.AreEqual(entity.ModifiedAt.Value.Date, DateTime.Today);
        }
    }
}