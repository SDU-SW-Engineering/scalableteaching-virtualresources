using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScalableTeaching.OpenNebula.Containers
{
    class FieldNotFoundException : Exception
    {
        public FieldNotFoundException(string message) : base(message)
        {
        }

        public FieldNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }        
    }
}
