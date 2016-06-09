using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.LightSwitch;
namespace LightSwitchApplication
{
    public partial class ResumenCaja
    {
        partial void RecibidoXFacturacion_Compute(ref decimal result)
        {
            result = FacturadoContado;
        }

        partial void FacturadoTotal_Compute(ref decimal result)
        {
            result = FacturadoContado + FacturadoXCobrar + FacturadoConAnticipos;
        }

        partial void RecibidoTotal_Compute(ref decimal result)
        {
            result = RecibidoXFacturacion + RecibidoXCobranzas + RecibidoXAnticipos;
        }
    }
}
