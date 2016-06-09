using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.LightSwitch;
namespace LightSwitchApplication
{
    public partial class Empresa
    {
        partial void ActividadEconomica_Changed()
        {
            if (!string.IsNullOrWhiteSpace(ActividadEconomica))
                ActividadEconomica = ActividadEconomica.Trim().ToUpper();
        }

        partial void Calle_Changed()
        {
            if (!string.IsNullOrWhiteSpace(Calle))
                Calle = Calle.Trim().ToUpper();
        }

        partial void Ciudad_Changed()
        {
            if (!string.IsNullOrWhiteSpace(Ciudad))
                Ciudad = Ciudad.Trim().ToUpper();
        }

        partial void Nombre_Changed()
        {
            if (!string.IsNullOrWhiteSpace(Nombre))
                Nombre = Nombre.Trim().ToUpper(); 
        }

        partial void Pais_Changed()
        {
            if (!string.IsNullOrWhiteSpace(Pais))
                Pais = Pais.Trim().ToUpper();
        }

        partial void Propietario_Changed()
        {
            if (!string.IsNullOrWhiteSpace(Propietario))
                Propietario = Propietario.Trim().ToUpper();
        }

        partial void RazonSocial_Changed()
        {
            if (!string.IsNullOrWhiteSpace(RazonSocial))
                RazonSocial = RazonSocial.Trim().ToUpper();
        }

        partial void Resumen_Compute(ref string result)
        {
            result = EmpresaId + " - " + Nombre;
        }

        partial void Zona_Changed()
        {
            if (!string.IsNullOrWhiteSpace(Zona))
                Zona = Zona.Trim().ToUpper();
        }
    }
}
