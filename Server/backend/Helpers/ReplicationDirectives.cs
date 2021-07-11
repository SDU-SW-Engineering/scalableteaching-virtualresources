using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Helpers
{
    public enum ReplicationDirectives
    {
        SINGLE_MACHINE,
        MULTIPLE_SHARED,
        PER_GROUP,
        PER_USER
    }
}
