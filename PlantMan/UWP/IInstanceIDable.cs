using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantMan
{
    // This interface basically just specifies, "if you put IInstanceIDable on your class
    // declaration, then your object must have a public property of type int called InstanceID,
    // which has at least a public Get accessor."
    interface IInstanceIDable
    {
        int InstanceID
        {
            get;
        }
    }
}
