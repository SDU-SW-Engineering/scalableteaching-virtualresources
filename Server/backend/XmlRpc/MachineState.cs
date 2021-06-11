using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.XmlRpc
{
    public enum MachineState
    {
        INIT,
        PENDING,
        HOLD,
        ACTIVE,
        STOPPED,
        SUSPENDED,
        DONE,
        FAILED,
        POWEROFF,
        UNDEPLOYED,
        CLONING,
        CLONING_FALIURE
    }
}
