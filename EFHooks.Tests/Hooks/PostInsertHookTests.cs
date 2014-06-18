using System;
using System.Data.Entity;
using NUnit.Framework;

namespace EFHooks.Tests.Hooks
{
    public class PostInsertHookTests
    {
        private class TimestampPostInsertHook : PostInsertHook<ITimeStamped>
        {
            public override void Hook(ITimeStamped entity, HookEntityMetadata metadata)
            {
                entity.CreatedAt = DateTime.Now;
            }
        }

        [Test]
        public void PostInsertHook_HasAddedHookState()
        {
            var hook = new TimestampPostInsertHook();
            Assert.AreEqual(EntityState.Added, hook.HookStates);
        }

        [Test]
        public void PostInsertHook_InterfaceHookCallsIntoGenericMethod()
        {
            var hook = new TimestampPostInsertHook();
            var entity = new TimestampedSoftDeletedEntity();

            ((IHook)hook).HookObject(entity, null);
            Assert.AreEqual(DateTime.Today, entity.CreatedAt.Date);
        }
    }
}