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
    public partial class ScreenFacturasCobroAnulacion
    {
        #region Propiedades

        IContentItemProxy DVFecha;
        IContentItemProxy DVFechaInicial;
        IContentItemProxy DVFechaFinal;

        #endregion

        partial void ScreenFacturasCobroAnulacion_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            try
            {
                Mensaje = string.Empty;

                // Configuracion
                string id = ServicesClient.ServicioFacturacion.GetInstalacionId();
                if (string.IsNullOrWhiteSpace(id))
                {
                    Mensaje = "No hay archivo de configuración!";
                    return;
                }
                Cfg = (from c in DataWorkspace.ApplicationData.Configuracions
                       where c.InstalacionId == id
                       select c).FirstOrDefault();
                if (Cfg == null)
                {
                    Mensaje = "No hay registro de configuración!";
                    return;
                }

                // Soft Cliente
                SoftCliente = (from s in DataWorkspace.ApplicationData.SoftClientes
                               where s.SoftProductoId == Cfg.SoftProductoId
                               select s).FirstOrDefault();
                if (SoftCliente == null)
                {
                    Mensaje = "No hay registro de cliente!";
                    return;
                }

                IContentItemProxy comboEmpresas = this.FindControl("ComboEmpresas");
                comboEmpresas.IsVisible = false;
                if (SoftCliente.NroEmpresas > 1)
                {
                    comboEmpresas.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.Empresas", System.Windows.Data.BindingMode.OneWay);
                    comboEmpresas.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.Empresas.SelectedItem", System.Windows.Data.BindingMode.TwoWay);
                    comboEmpresas.IsEnabled = Application.Current.User.HasPermission(Permissions.CambiarEmpresa);
                    comboEmpresas.IsVisible = true;
                }

                DVFecha = this.FindControl("Fecha");
                DVFechaInicial = this.FindControl("FechaInicial");
                DVFechaFinal = this.FindControl("FechaFinal");

                RangoFechas = "D";
                Fecha = Cfg.FechaHoy.GetValueOrDefault();
                FechaInicial = Cfg.FechaHoy;
                FechaFinal = Cfg.FechaHoy;
            }
            catch (Exception ex)
            {
                this.ShowMessageBox("Error en Inicialización Workspace: " + ex.Message);
            }
        }

        partial void RangoFechas_Changed()
        {
            if (RangoFechas == "D" || RangoFechas == "S" || RangoFechas == "M" || RangoFechas == "A")
            {
                DVFecha.IsVisible = true;
                DVFechaInicial.IsVisible = false;
                DVFechaFinal.IsVisible = false;
            }
            else if (RangoFechas == "F")
            {
                DVFecha.IsVisible = false;
                DVFechaInicial.IsVisible = true;
                DVFechaFinal.IsVisible = true;
            }
            else    // RangoFechas == "T"
            {
                DVFecha.IsVisible = false;
                DVFechaInicial.IsVisible = false;
                DVFechaFinal.IsVisible = false;
            }
        }

        partial void Buscar_Execute()
        {
            if (RangoFechas == "D")
            {
                COFechaIni = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 0, 0, 0);
                COFechaFin = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 23, 59, 59);
            }
            else if (RangoFechas == "S")
            {
                COFechaIni = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 0, 0, 0);
                COFechaFin = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 23, 59, 59);
                while (COFechaIni.DayOfWeek != DayOfWeek.Monday)
                    COFechaIni = COFechaIni.AddDays(-1);
                while (COFechaFin.DayOfWeek != DayOfWeek.Sunday)
                    COFechaFin = COFechaFin.AddDays(1);
            }
            else if (RangoFechas == "M")
            {
                COFechaIni = new DateTime(Fecha.Year, Fecha.Month, 1, 0, 0, 0);
                COFechaFin = COFechaIni.AddMonths(1).AddDays(-1);
                COFechaFin = new DateTime(COFechaFin.Year, COFechaFin.Month, COFechaFin.Day, 23, 59, 59);
            }
            else if (RangoFechas == "A")
            {
                COFechaIni = new DateTime(Fecha.Year, 1, 1, 0, 0, 0);
                COFechaFin = new DateTime(Fecha.Year, 12, 31, 23, 59, 59);
            }
            else if (RangoFechas == "T")
            {
                COFechaIni = new DateTime(2000, 1, 1, 0, 0, 0);
                COFechaFin = new DateTime(3000, 12, 31, 23, 59, 59);
            }
            else    // RangoFechas == "F"
            {
                COFechaIni = FechaInicial.GetValueOrDefault();
                COFechaFin = FechaFinal.GetValueOrDefault();
            }

            Mensaje = ClienteOperaciones.Count == 0 ? "No hay facturas cobradas en este rango de fechas!" : string.Empty;
        }

        partial void Anular_Execute()
        {
            if (ClienteOperaciones.SelectedItem == null)
                return;

            ClienteOperacion opCobrada = ClienteOperaciones.SelectedItem;
            ClienteOperacion opCobro = opCobrada.OperacionesRelacionadas.FirstOrDefault();

            if (opCobro != null)
            {
                opCobro.Estado = "C";
                opCobrada.OperacionesRelacionadas.Remove(opCobro);
            }
            
            opCobrada.Estado = "V";
            opCobrada.MedioPagoBS = "EF";
            opCobrada.MedioPagoUS = "EF";
            opCobrada.MontoBS = opCobrada.Monto;
            opCobrada.MontoUS = 0M;

            this.Save();
            ClienteOperaciones.Load();
        }
    }
}
