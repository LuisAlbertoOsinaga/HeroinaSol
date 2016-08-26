using System;
using System.Linq;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using Microsoft.LightSwitch;
using Microsoft.LightSwitch.Framework.Client;
using Microsoft.LightSwitch.Presentation;
using Microsoft.LightSwitch.Presentation.Extensions;
using System.Windows;
namespace LightSwitchApplication
{
    public partial class ScreenFacturaCobroSimple
    {
        #region Propiedades

        IContentItemProxy DVFecha;
        IContentItemProxy DVFechaInicial;
        IContentItemProxy DVFechaFinal;

        IContentItemProxy boxFactura;
        IContentItemProxy btnAnularOperacion;
        IContentItemProxy btnCobrarOperacion;

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

        void CobrarOpCredito()
        {

                ClienteOperacion opXCobrar = OperacionesXCobrar.SelectedItem;
                ClienteOperacion opPago = this.DataWorkspace.ApplicationData.ClienteOperacions.AddNew();

                opXCobrar.Estado = "C";
            
                opPago.Cliente = opXCobrar.Cliente;
                opPago.OperacionOrigen = opXCobrar;
                opPago.Contabilizada = false;
                opPago.Estado = "V";    // Vigente
                opPago.NroAutorizacion = opXCobrar.NroAutorizacion;
                opPago.FacturaNro = opXCobrar.FacturaNro;
                opPago.Fecha = FechaCobro;
                opPago.Monto = opXCobrar.Monto;
                opPago.MontoBS = opXCobrar.MontoBS;
                if (Cfg.TipoCambio > 0)
                    opPago.MontoUS = Math.Round(opXCobrar.MontoBS / Cfg.TipoCambio, 2);
                opPago.MedioPagoBS = "DC";
                opPago.MedioPagoUS = "DC";
                opPago.TipoOperacion = "CC";

                Factura.Estado = "P";

                Save();
                OperacionesXCobrar.Load();
        }

        void FindControls()
        {
            DVFecha = this.FindControl("DVFecha");
            DVFechaInicial = this.FindControl("DVFechaInicial");
            DVFechaFinal = this.FindControl("DVFechaFinal");

            boxFactura = this.FindControl("DetalleFactura");
            btnAnularOperacion = this.FindControl("AnularOperacion");
            btnCobrarOperacion = this.FindControl("CobrarOperacion");
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

        partial void AnularOperacion_Execute()
        {
            MessageBoxResult result = this.ShowMessageBox(string.Format("Desea anular la Op.de Crédito {0}, asociada a la Factura {1} - {2} ?", 
                                                                        OperacionesXCobrar.SelectedItem.Id, OperacionesXCobrar.SelectedItem.NroAutorizacion, 
                                                                        OperacionesXCobrar.SelectedItem.FacturaNro), 
                                                            "CONFIRMACIÓN", MessageBoxOption.YesNo);
            if(result == MessageBoxResult.Yes)
            {
                OperacionesXCobrar.SelectedItem.Estado = "A";
                Save();
                OperacionesXCobrar.Load();
            }
        }

        partial void BuscarOperacioneXCobrar_Execute()
        {
            BuscarOperaciones();
        }

        partial void CancelarCredito_Execute()
        {
            this.CloseModalWindow("CobraOperacionCredito");
        }

        partial void CobrarOperacion_Execute()
        {
            this.OpenModalWindow("CobraOperacionCredito");
        }

        partial void CobrarCredito_Execute()
        {
            DateTime fechaTemp = OperacionesXCobrar.SelectedItem.Fecha;
            DateTime fecha = new DateTime(fechaTemp.Year, fechaTemp.Month, fechaTemp.Day);
            
            if(FechaCobro < fecha)
            {
                this.ShowMessageBox("Fecha de cobro no puede ser anterior a fecha de la operación!", "ERROR", MessageBoxOption.Ok);
                return;
            }
            CobrarOpCredito();
            this.CloseModalWindow("CobraOperacionCredito");
        }

        partial void OperacionesXCobrar_Loaded(bool succeeded)
        {
            Mensaje = OperacionesXCobrar.Count > 0 ? string.Empty : "No hay operaciones por cobrar!";
            if(OperacionesXCobrar.Count > 0)
            {
                CantidadFacturas = OperacionesXCobrar.Count;
                Total = OperacionesXCobrar.Sum(x => x.Monto);
            }
        }

        partial void OperacionesXCobrar_SelectionChanged()
        {
            boxFactura.DisplayName = string.Format("Factura Nro: {0} - {1}", Factura.Dosificacion.NroAutorizacion, Factura.Nro);
            btnAnularOperacion.IsVisible = Factura.Estado == "A";
            btnCobrarOperacion.IsVisible = Factura.Estado == "V";
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
