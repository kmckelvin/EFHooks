using System;
using System.Data;
using NUnit.Framework;

namespace EFHooks.Tests.Hooks
{
    public class PostUpdateHookTests
    {
        private class TimestampPostUpdateHook : PostUpdateHook<ITimeStamped>
        {
            public override void Hook(ITimeStamped entity, HookEntityMetadata metadata)
            {
                entity.ModifiedAt = DateTime.Now;
            }
        }

        [Test]
        public void PostUpdateHook_HasModifiedHookState()
        {
            var hook = new TimestampPostUpdateHook();
            Assert.AreEqual(EntityState.Modified, hook.HookStates);
        }

        [Test]
        public void PostUpdateHook_HookCallsIntoGenericMethod()
        {
            var hook = new TimestampPostUpdateHook();
            var entity = new TimestampedSoftDeletedEntity();
            ((IHook)hook).Hook(entity, null);

            Assert.AreEqual(entity.ModifiedAt.Value.Date, DateTime.Today);
        }
    }
}