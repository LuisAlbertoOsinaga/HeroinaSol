using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.LightSwitch;
namespace LightSwitchApplication
{
    public partial class Dosificacion
    {
        partial void Dosificacion_Created()
        {
            
        }

        partial void NroAutorizacion_Validate(EntityValidationResultsBuilder results)
        {
            if (NroAutorizacion == null)
                return;

            if(NroAutorizacion.Length < 12)
            {
                results.AddPropertyError("El Nro de Autorización debe tener 12 o más caracteres!");
            }
            
            long nro;
            if(!long.TryParse(NroAutorizacion, out nro) || nro <= 0)
            {
                results.AddPropertyError("El Nro de Autorización debe ser un número!");
            }
        }

        partial void NroTramite_Validate(EntityValidationResultsBuilder results)
        {
            if (NroTramite == null)
                return;

            long nro;
            if (!long.TryParse(NroTramite, out nro) || nro <= 0)
            {
                results.AddPropertyError("El Nro de Trámite debe ser un número!");
            }
        }

        partial void FacturaFinal_Validate(EntityValidationResultsBuilder results)
        {
            FacturaFinal = FacturaInicial + CantidadDeFacturas - 1;
        }

        partial void FechaLimiteEmision_Validate(EntityValidationResultsBuilder results)
        {
            if(FechaLimiteEmision < FechaInicial)
            {
                results.AddPropertyError("La Fecha límite de Emisión no puede ser anterior a la Fecha Inicia; de la dosificación!");
            }
        }

        partial void FacturaInicial_Changed()
        {
            FacturaFinal = FacturaInicial + CantidadDeFacturas - 1;
        }

        partial void CantidadDeFacturas_Changed()
        {
            FacturaFinal = FacturaInicial + CantidadDeFacturas - 1;
        }

        partial void FacturaTipo_Validate(EntityValidationResultsBuilder results)
        {
            if (FacturaTipo == null)
                return;

            if (FacturaTipo.Codigo == "TOD")
                results.AddPropertyError("El Tipo de Factura no puede ser 'TODAS'!");
        }
    }
}
