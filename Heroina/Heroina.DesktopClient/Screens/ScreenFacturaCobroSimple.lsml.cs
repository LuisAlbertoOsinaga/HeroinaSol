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
    public partial class ScreenFacturaCobroSimple
    {
        #region Propiedades

        IContentItemProxy DVFecha;
        IContentItemProxy DVFechaInicial;
        IContentItemProxy DVFechaFinal;

        #endregion

        #region Metodos Auxiliares

        void BuscarOperaciones()
        {
            if (RangoFechas == "D")
            {
                OXCFechaIni = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 0, 0, 0);
                OXCFechaFin = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 23, 59, 59);
            }
            else if (RangoFechas == "S")
            {
                OXCFechaIni = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 0, 0, 0);
                OXCFechaFin = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 23, 59, 59);
                while (OXCFechaIni.DayOfWeek != DayOfWeek.Monday)
                    OXCFechaIni = OXCFechaIni.AddDays(-1);
                while (OXCFechaFin.DayOfWeek != DayOfWeek.Sunday)
                    OXCFechaFin = OXCFechaFin.AddDays(1);
            }
            else if (RangoFechas == "M")
            {
                OXCFechaIni = new DateTime(Fecha.Year, Fecha.Month, 1, 0, 0, 0);
                OXCFechaFin = OXCFechaIni.AddMonths(1).AddDays(-1);
                OXCFechaFin = new DateTime(OXCFechaFin.Year, OXCFechaFin.Month, OXCFechaFin.Day, 23, 59, 59);
            }
            else if (RangoFechas == "A")
            {
                OXCFechaIni = new DateTime(Fecha.Year, 1, 1, 0, 0, 0);
                OXCFechaFin = new DateTime(Fecha.Year, 12, 31, 23, 59, 59);
            }
            else if (RangoFechas == "T")
            {
                OXCFechaIni = new DateTime(2000, 1, 1, 0, 0, 0);
                OXCFechaFin = new DateTime(3000, 12, 31, 23, 59, 59);
            }
            else    // RangoFechas == "F"
            {
                OXCFechaIni = FechaInicial;
                OXCFechaFin = FechaFinal;
            }

            OperacionesXCobrar.Load();
        }

        void FindControls()
        {
            DVFecha = this.FindControl("DVFecha");
            DVFechaInicial = this.FindControl("DVFechaInicial");
            DVFechaFinal = this.FindControl("DVFechaFinal");
        }

        void InitDataWorkspace()
        {
            #region Configuracion

            Mensaje = string.Empty;

            string id = ServicesClient.ServicioFacturacion.GetInstalacionId();
            if (string.IsNullOrWhiteSpace(id))
            {
                this.ShowMessageBox("No hay archivo de configuración!", "CashFlow", MessageBoxOption.Ok);
                Close(promptUserToSave: false);
                return;
            }
            Cfg = (from c in DataWorkspace.ApplicationData.Configuracions
                   where c.InstalacionId == id
                   select c).FirstOrDefault();
            if (Cfg == null)
            {
                this.ShowMessageBox("No hay registro de configuración!", "CashFlow", MessageBoxOption.Ok);
                Close(promptUserToSave: false);
                return;
            }

            #endregion

            #region Soft Cliente

            SoftCliente = (from s in DataWorkspace.ApplicationData.SoftClientes
                           where s.SoftProductoId == Cfg.SoftProductoId
                           select s).FirstOrDefault();
            if (SoftCliente == null)
            {
                this.ShowMessageBox("No hay registro de cliente!", "CashFlow", MessageBoxOption.Ok);
                Close(promptUserToSave: false);
                return;
            }

            SoftProducto = Utilidades.SoftProducto(SoftCliente);

            #endregion
        }

        private bool Inits()
        {
            Autoimpresor = (from a in DataWorkspace.ApplicationData.Autoimpresores
                            where a.Nombre == Cfg.Autoimpresor && a.Sucursal.Nombre == Cfg.Sucursal &&
                                  a.Sucursal.Empresa.Nombre == Cfg.Empresa
                            select a).FirstOrDefault();
            if (Autoimpresor == null)
            {
                this.ShowMessageBox("no hay registro de Autoimpresor!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                Close(promptUserToSave: false);
                return false;
            }

            Sucursal = Autoimpresor.Sucursal;
            Empresa = Autoimpresor.Sucursal.Empresa;
            EmpresaNombre = SoftCliente.NroEmpresas > 1 || Empresa == null ? SoftCliente.ClienteNombre : Empresa.Nombre;

            RangoFechas = "D";
            OXCEstadoIni = "V";
            OXCEstadoFin = "V";

            return true;
        }

        #endregion

        #region Metodos Generados

        partial void ScreenFacturaCobroSimple_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            FindControls();
            InitDataWorkspace();
            Inits();
        }

        partial void BuscarOperacioneXCobrar_Execute()
        {
            BuscarOperaciones();
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

        #endregion
    }
}
