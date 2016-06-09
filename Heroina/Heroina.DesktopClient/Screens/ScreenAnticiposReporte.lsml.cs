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
    public partial class ScreenAnticiposReporte
    {
        #region Propiedades

        IContentItemProxy DVFecha;
        IContentItemProxy DVFechaInicial;
        IContentItemProxy DVFechaFinal;

        #endregion

        partial void ScreenAnticiposReporte_InitializeDataWorkspace(List<IDataService> saveChangesTo)
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

            DVFecha = this.FindControl("DVFecha");
            DVFechaInicial = this.FindControl("DVFechaInicial");
            DVFechaFinal = this.FindControl("DVFechaFinal");

            RangoFechas = "D";
            Fecha = Cfg.FechaHoy.GetValueOrDefault();
            FechaInicial = Cfg.FechaHoy.GetValueOrDefault();
            FechaFinal = Cfg.FechaHoy.GetValueOrDefault();

            IContentItemProxy btnAnular = this.FindControl("Anular");
            #if DEBUG
            btnAnular.IsVisible = Application.Current.User.Name == "TestUser";
            #else
            btnAnular.IsVisible = Application.Current.User.HasPermission(Permissions.ManageAnticiposAnulacion);
            #endif
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
                COFechaIni = FechaInicial;
                COFechaFin = FechaFinal;
            }
        }

        partial void Anular_Execute()
        {
            if (ClienteOperaciones.SelectedItem == null)
                return;

            this.OpenModalWindow("ModalAnticipo");
        }

        partial void CancelarAnulacion_Execute()
        {
            this.CloseModalWindow("ModalAnticipo");
        }

        partial void ConfirmarAnulacion_Execute()
        {
            this.CloseModalWindow("ModalAnticipo");
            ClienteOperaciones.SelectedItem.Estado = "A";
            this.Save();
            ClienteOperaciones.Load();
        }
    }
}
