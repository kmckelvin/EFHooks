using System;
using System.Data;
using NUnit.Framework;

namespace EFHooks.Tests.Hooks
{
    public class PostDeleteHookTests
    {
        private class TimestampPostDeleteHook : PostDeleteHook<ITimeStamped>
        {
            public override void Hook(ITimeStamped entity, HookEntityMetadata metadata)
            {
                entity.ModifiedAt = DateTime.Now;
            }
        }

        [Test]
        public void PostDeleteHook_HasModifiedHookState()
        {
            var hook = new TimestampPostDeleteHook();
            Assert.AreEqual(EntityState.Deleted, hook.HookStates);
        }

        [Test]
        public void PostDeleteHook_HookCallsIntoGenericMethod()
        {
            var hook = new TimestampPostDeleteHook();
            var entity = new TimestampedSoftDeletedEntity();
            ((IHook)hook).HookObject(entity, null);

            Assert.AreEqual(entity.ModifiedAt.Value.Date, DateTime.Today);
        }
    }
}