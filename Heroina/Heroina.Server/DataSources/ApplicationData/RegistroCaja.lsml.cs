using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.LightSwitch;
namespace LightSwitchApplication
{
    public partial class RegistroCaja
    {
        partial void ReporteId_Compute(ref string result)
        {
            result = Autoimpresor.Nombre + " - " + Fecha.ToString("dd/MM/yyyy") + " - " + Turno.Nombre + " - " + Usuario; 
        }
    }
}
