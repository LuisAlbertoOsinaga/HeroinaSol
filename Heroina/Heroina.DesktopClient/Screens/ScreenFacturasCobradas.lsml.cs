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
    public partial class ScreenFacturasCobradas
    {
        #region Propiedades

        IContentItemProxy boxFactura;
        IContentItemProxy DVFecha;
        IContentItemProxy DVFechaInicial;
        IContentItemProxy DVFechaFinal;
        
        #endregion

        #region Metodos Auxiliares
     
        void BuscarOperaciones()
        {
            if (RangoFechas == "D")
            {
                FechaInicial = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 0, 0, 0);
                FechaFinal = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 23, 59, 59);
            }
            else if (RangoFechas == "S")
            {
                FechaInicial = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 0, 0, 0);
                FechaFinal = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 23, 59, 59);
                while (FechaInicial.DayOfWeek != DayOfWeek.Monday)
                    FechaInicial = FechaInicial.AddDays(-1);
                while (FechaFinal.DayOfWeek != DayOfWeek.Sunday)
                    FechaFinal = FechaFinal.AddDays(1);
            }
            else if (RangoFechas == "M")
            {
                FechaInicial = new DateTime(Fecha.Year, Fecha.Month, 1, 0, 0, 0);
                FechaFinal = FechaInicial.AddMonths(1).AddDays(-1);
                FechaFinal = new DateTime(FechaFinal.Year, FechaFinal.Month, FechaFinal.Day, 23, 59, 59);
            }
            else if (RangoFechas == "A")
            {
                FechaInicial = new DateTime(Fecha.Year, 1, 1, 0, 0, 0);
                FechaFinal = new DateTime(Fecha.Year, 12, 31, 23, 59, 59);
            }
            else if (RangoFechas == "T")
            {
                FechaInicial = new DateTime(2000, 1, 1, 0, 0, 0);
                FechaFinal = new DateTime(3000, 12, 31, 23, 59, 59);
            }
            else    // RangoFechas == "F"
            {
            }

            if(RangoFechasEmision == "A")
            {
                CredFechaInicial = new DateTime(2000, 1, 1, 0, 0, 0);
                CredFechaFinal = new DateTime(Fecha.Year, Fecha.Month, 1, 0, 0, 0);
                CredFechaFinal = CredFechaFinal.AddDays(-1);
                CredFechaFinal = new DateTime(CredFechaFinal.Year, CredFechaFinal.Month, CredFechaFinal.Day, 23, 59, 59);
            }
            else if (RangoFechasEmision == "M")
            {
                CredFechaInicial = new DateTime(Fecha.Year, Fecha.Month, 1, 0, 0, 0);
                CredFechaFinal = CredFechaInicial.AddMonths(1).AddDays(-1);
                CredFechaFinal = new DateTime(CredFechaFinal.Year, CredFechaFinal.Month, CredFechaFinal.Day, 23, 59, 59);
            }
            else    // RangoFechasEmision == "T"
            {
                CredFechaInicial = new DateTime(2000, 1, 1, 0, 0, 0);
                CredFechaFinal = new DateTime(3000, 12, 31, 23, 59, 59);
            }

            OperacionesCobradas.Load();
        }


        void FindControls()
        {
            boxFactura = this.FindControl("FacturaSeleccionada");
            DVFecha = this.FindControl("Fecha");
            DVFechaInicial = this.FindControl("FechaInicial");
            DVFechaFinal = this.FindControl("FechaFinal");
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

        void Inits()
        {
            EmpresaNombre = SoftCliente.NroEmpresas > 1 || Empresa == null ? SoftCliente.ClienteNombre : Empresa.SelectedItem.Nombre;
            RangoFechas = "D";
            RangoFechasEmision = "T";
            CredFechaInicial = new DateTime(2000, 1, 1, 0, 0, 0);
            CredFechaFinal = new DateTime(3000, 12, 31, 23, 59, 59);
        }

        #endregion

        #region Metodos Autogenerados

        partial void ScreenFacturasCobradas_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            FindControls();
            InitDataWorkspace();
            Inits();
        }

        partial void BuscarOperacionesCobradas_Execute()
        {
            BuscarOperaciones();
        }

        partial void OperacionesCobradas_Loaded(bool succeeded)
        {
            Mensaje = OperacionesCobradas.Count > 0 ? string.Empty : "No hay operaciones cobradas en este período!";
            if (OperacionesCobradas.Count > 0)
            {
                CantidadFacturas = OperacionesCobradas.Count;
                Total = OperacionesCobradas.Sum(x => x.Monto);
            }
        }

        partial void OperacionesCobradas_SelectionChanged()
        {
            boxFactura.DisplayName = OperacionesCobradas.SelectedItem != null ? string.Format("Factura Nro: {0} - {1}", Factura.Dosificacion.NroAutorizacion, Factura.Nro) : string.Empty;
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
