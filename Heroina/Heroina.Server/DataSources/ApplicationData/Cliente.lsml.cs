using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.LightSwitch;
namespace LightSwitchApplication
{
    public partial class Cliente
    {
        partial void Nombre_NIT_Compute(ref string result)
        {
            result = RazonSocial + " - " + NIT;
        }
    }
}
