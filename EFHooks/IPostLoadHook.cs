using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFHooks
{
    /// <summary>
    /// A hook that is executed after an entity has been loaded.
    /// </summary>
    public interface IPostLoadHook : IHook
    {
    }
}
