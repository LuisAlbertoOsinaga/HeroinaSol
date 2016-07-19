using System;
using System.Linq;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using Microsoft.LightSwitch;
using Microsoft.LightSwitch.Framework.Client;
using Microsoft.LightSwitch.Presentation;
using Microsoft.LightSwitch.Presentation.Extensions;
namespace LightSwitchApplication
{
    public partial class Application
    {
        partial void GridEmpresas_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageDefiniciones);  
            #endif
        }

        partial void GridSucursales_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageDefiniciones);
            #endif
        }

        partial void GridAutoimpresores_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageDefiniciones);
            #endif
        }

        partial void GridFacturaTipos_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageDefiniciones);
            #endif
        }

        partial void GridFacturaTipoDetalles_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageDefiniciones);
            #endif
        }

        partial void GridSoftClientes_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageConfiguracion) || 
                        Application.Current.User.HasPermission(Permissions.ManageSoftClientes);
            #endif
        }

        partial void GridConfiguracion_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageConfiguracion);
            #endif
        }

        partial void ScreenFacturasXAutoimpresor_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";       
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageReportes) ||  
                        Application.Current.User.HasPermission(Permissions.VerReportesFacturacion);
            #endif
        }

        partial void ScreenFacturasXSucursal_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageReportes) ||   
                        Application.Current.User.HasPermission(Permissions.VerReportesFacturacion);
            #endif
        }

        partial void ScreenFacturasXFechas_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageReportes) ||   
                        Application.Current.User.HasPermission(Permissions.VerReportesFacturacion);
            #endif
        }

        partial void ScreenFacturasXDosificacion_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageReportes) ||   
                        Application.Current.User.HasPermission(Permissions.VerReportesFacturacion);
            #endif
        }

        partial void ScreenLibroVentas_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageReportes) ||   
                        Application.Current.User.HasPermission(Permissions.VerReportesFacturacion);
            #endif
        }

        partial void ScreenFechaHoy_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.CambiarFecha);
            #endif
        }

        partial void ScreenFacturacion_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageCajas) ||  
                        Application.Current.User.HasPermission(Permissions.EmitirFacturas);
            #endif
        }

        partial void GridDosificaciones_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageDosificaciones);
            #endif
        }

        partial void GridClientes_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageCajas) ||  
                        Application.Current.User.HasPermission(Permissions.EmitirFacturas);
            #endif
        }

        partial void ScreenProcesos_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageProcesos);
            #endif
        }

        partial void ScreenClientesOperaciones_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageReportes) ||   
                        Application.Current.User.HasPermission(Permissions.ManageClientesOperaciones);
            #endif
        }

        partial void ScreenAnticiposReporte_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageSupervision) ||  
                        Application.Current.User.HasPermission(Permissions.ManageReporteAnticipos);
            #endif
        }

        partial void ScreenFacturasCobroAnulacion_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageSupervision) ||  
                        Application.Current.User.HasPermission(Permissions.ManageFacturaCobroAnulacion);
            #endif
        }

        partial void ScreenAnticipos_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageCajas) ||  
                Application.Current.User.HasPermission(Permissions.ManageAnticipos);
            #endif
        }

        partial void ScreenFacturasPorCobrar_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageReportes);
            #endif
        }

        partial void ScreenAperturaCaja_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageCajas) || 
                        Application.Current.User.HasPermission(Permissions.ManageAperturaCaja);
            #endif
        }

        partial void ScreenFacturaCobro_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageCajas) ||  
                Application.Current.User.HasPermission(Permissions.ManageFacturasPagos);
            #endif
        }

        partial void GridTurnos_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageDefiniciones);
            #endif
        }

        partial void ScreenHabilitarCaja_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageSupervision) || 
                Application.Current.User.HasPermission(Permissions.HabilitarCaja);
            #endif
        }

        partial void ScreenCierreCaja_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageCajas) ||  
                        Application.Current.User.HasPermission(Permissions.ManageAperturaCaja);
            #endif
        }

        partial void ScreenReaperturaCaja_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageSupervision) ||  
                        Application.Current.User.HasPermission(Permissions.ManageReaperturaCaja);
            #endif
        }

        partial void ScreenMedioPagos_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageDefiniciones);
            #endif
        }

        partial void ScreenActividadesEconomicas_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageDefiniciones) ||
                        Application.Current.User.HasPermission(Permissions.ManageDosificaciones);
            #endif
        }

        partial void ScreenFacturaAnulacion_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageCajas);
            #endif
        }

        partial void ScreenCausasAnulaciones_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageSupervision);
            #endif
        }

        partial void ScreenFacturaAnulador_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageSupervision);
            #endif
        }

        partial void ScreenFacturaPorNumero_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageReportes);
            #endif
        }

        partial void ScreenSupervisionCaja_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageSupervision);
            #endif
        }

        partial void ScreenResumenCajaDiario_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageReportes);
            #endif
        }

        partial void ScreenCerrarCaja_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageSupervision);
            #endif
        }

        partial void ScreenRolMails_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageDefiniciones);
            #endif
        }

        partial void ScreenParametros_CanRun(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageDefiniciones);
            #endif
        }
    }
}
