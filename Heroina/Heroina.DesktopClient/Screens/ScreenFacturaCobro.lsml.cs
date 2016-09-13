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
using System.Windows.Controls;
using System.Windows.Media;
namespace LightSwitchApplication
{
    public partial class ScreenFacturaCobro
    {
        #region Propiedades

        IContentItemProxy DVFecha;
        IContentItemProxy DVFechaInicial;
        IContentItemProxy DVFechaFinal;

        IContentItemProxy btnFactura;

        IContentItemProxy groupCaja;
        IContentItemProxy groupEmpresas;
        IContentItemProxy groupModosPago;

        #endregion

        #region Métodos Auxiliares

        void Bindings()
        {
            groupCaja.ControlAvailable += GroupCaja_ControlAvailable;
        }

        void BuscarOperaciones()
        {
            if (RangoFechas == "D")
            {
                CredFechaIni = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 0, 0, 0);
                CredFechaFin = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 23, 59, 59);
            }
            else if (RangoFechas == "S")
            {
                CredFechaIni = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 0, 0, 0);
                CredFechaFin = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 23, 59, 59);
                while (CredFechaIni.DayOfWeek != DayOfWeek.Monday)
                    CredFechaIni = CredFechaIni.AddDays(-1);
                while (CredFechaFin.DayOfWeek != DayOfWeek.Sunday)
                    CredFechaFin = CredFechaFin.AddDays(1);
            }
            else if (RangoFechas == "M")
            {
                CredFechaIni = new DateTime(Fecha.Year, Fecha.Month, 1, 0, 0, 0);
                CredFechaFin = CredFechaIni.AddMonths(1).AddDays(-1);
                CredFechaFin = new DateTime(CredFechaFin.Year, CredFechaFin.Month, CredFechaFin.Day, 23, 59, 59);
            }
            else if (RangoFechas == "A")
            {
                CredFechaIni = new DateTime(Fecha.Year, 1, 1, 0, 0, 0);
                CredFechaFin = new DateTime(Fecha.Year, 12, 31, 23, 59, 59);
            }
            else if (RangoFechas == "T")
            {
                CredFechaIni = new DateTime(2000, 1, 1, 0, 0, 0);
                CredFechaFin = new DateTime(3000, 12, 31, 23, 59, 59);
            }
            else    // RangoFechas == "F"
            {
                CredFechaIni = FechaInicial;
                CredFechaFin = FechaFinal;
            }
        }

        void CreaOperacionPago()
        {
            PagoContado_Execute();
        }

        void Enablings()
        {
        }

        void FindControls()
        {
            btnFactura = this.FindControl("ShowFactura");
            groupCaja = this.FindControl("GroupCaja");
            groupEmpresas = this.FindControl("GroupEmpresas");
            groupModosPago = this.FindControl("GroupModosPago");
        }

        private void GroupCaja_ControlAvailable(object sender, ControlAvailableEventArgs e)
        {
            Control box = (Control) e.Control;
            box.Background = new SolidColorBrush(Color.FromArgb(255, Globales.CajaColorRed, Globales.CajaColorGreen, Globales.CajaColorBlue));
        }

        bool HayCajaAbierta()
        {
            if (Cfg.RegistroCaja == 0)
                return false;

            RegistroCaja = DataWorkspace.ApplicationData.RegistroCajas_SingleOrDefault(Cfg.RegistroCaja);
            return (RegistroCaja != null && RegistroCaja.CajaAbierta);
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

        bool Inits()
        {
            CreEstadoIni = "V";
            CreEstadoFin = "V";

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
            if (Autoimpresor.Id != RegistroCaja.Autoimpresor.Id)
            {
                this.ShowMessageBox("Autoimpresor no correponde a este registro de caja!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                Close(promptUserToSave: false);
                return false;
            }

            Sucursal = Autoimpresor.Sucursal;
            Empresa = Autoimpresor.Sucursal.Empresa;
            Turno = RegistroCaja.Turno;
            EmpresaNombre = SoftCliente.NroEmpresas > 1 || Empresa == null ? SoftCliente.ClienteNombre : Empresa.Nombre;
            groupEmpresas.IsVisible = SoftCliente.NroEmpresas > 1;

            return true;
        }

        #endregion

        #region Metodos Generados

        partial void ScreenFacturaCobro_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();

            if (!HayCajaAbierta())
            {
                this.ShowMessageBox("Para registrar un cobro de facturar debe primero abrir su caja!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                this.Close(promptUserToSave: false);
                return;
            }

            if (!(Application.Current.User.Name == RegistroCaja.Usuario))
            {
                this.ShowMessageBox(string.Format("Caja abierta con otro usuario: '{0}'!", RegistroCaja.Usuario),
                                        SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                this.Close(promptUserToSave: false);
                return;
            }

            FindControls();
            Bindings();
            if (!Inits()) 
            {
                this.ShowMessageBox("Error al iniciar pantalla!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                this.Close(promptUserToSave: false);
                return;
            }
            Enablings();

            DVFecha = this.FindControl("DVFecha");
            DVFechaInicial = this.FindControl("DVFechaInicial");
            DVFechaFinal = this.FindControl("DVFechaFinal");

            RangoFechas = "D";
            Fecha = DateTime.Now;
            FechaInicial = DateTime.Now;
            FechaFinal = DateTime.Now;
        }

        partial void CalcularBS_Execute()
        {
            if (FormaPago == null || RegistroCaja == null)
                return;

            FormaPago.MontoBS = FormaPago.Monto - Math.Round(FormaPago.MontoUS * RegistroCaja.TipoCambio, 2);
        }

        partial void CalcularUS_Execute()
        {
            if (FormaPago == null || RegistroCaja == null)
                return;

            FormaPago.MontoUS = Math.Round((FormaPago.Monto - FormaPago.MontoBS) / RegistroCaja.TipoCambio, 2);
        }

        partial void CerrarDatosFactura_Execute()
        {
            this.CloseModalWindow("DatosFactura");
        }

        partial void DetalleFacturacion_Execute()
        {
            Application.ShowScreenDetalleFacturas(RegistroCaja.Id);
        }

        partial void Fecha_Changed()
        {
            BuscarOperaciones();
        }

        partial void FechaInicial_Changed()
        {
            BuscarOperaciones();
        }

        partial void FechaFinal_Changed()
        {
            BuscarOperaciones();
        }

        partial void OperacionesXCobrar_Loaded(bool succeeded)
        {
            Mensaje = OperacionesXCobrar.Count == 0 ? "No hay operaciones por cobrar!" : string.Empty;
            btnFactura.IsEnabled = OperacionesXCobrar.Count > 0;
            groupModosPago.IsVisible = OperacionesXCobrar.Count > 0;
        }

        partial void OperacionesXCobrar_SelectionChanged()
        {
            if (OperacionesXCobrar.SelectedItem == null)
                return;

            CreaOperacionPago();
        }

        partial void OtroCliente_CanExecute(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageFacturasPagos);
            #endif
        }

        partial void PagoContado_CanExecute(ref bool result)
        {
            #if DEBUG
            result = Application.Current.User.Name == "TestUser";
            #else
            result = Application.Current.User.HasPermission(Permissions.ManageFacturasPagos);
            #endif
        }

        //partial void PagoContado_Execute()
        //{
        //    if (Facturas.SelectedItem == null)
        //        return;
        //    Facturas.SelectedItem.MontoContado = Facturas.SelectedItem.Monto;
        //    Facturas.SelectedItem.MontoContadoBS = Facturas.SelectedItem.Monto;
        //    Facturas.SelectedItem.MontoContadoUS = 0M;
        //    Facturas.SelectedItem.MontoPorCobrar = 0M;
        //    Facturas.SelectedItem.MontoAnticipado = 0M;
        //    Facturas.SelectedItem.AnticiposOpNros = string.Empty;
        //    Facturas.SelectedItem.ClienteXCobrarNIT = string.Empty;
        //    Facturas.SelectedItem.ClienteXCobrarNombre = string.Empty;
        //}

        //partial void PagoPorCobrar_Execute()
        //{
        //    if (Facturas.SelectedItem == null)
        //        return;
        //    Facturas.SelectedItem.MontoContado = 0M;
        //    Facturas.SelectedItem.MontoContadoBS = 0M;
        //    Facturas.SelectedItem.MontoContadoUS = 0M;
        //    Facturas.SelectedItem.MontoPorCobrar = Facturas.SelectedItem.Monto;
        //    Facturas.SelectedItem.MontoAnticipado = 0M;
        //    Facturas.SelectedItem.AnticiposOpNros = string.Empty;

        //    Facturas.SelectedItem.ClienteXCobrarNIT = Facturas.SelectedItem.ClienteNIT;
        //    Facturas.SelectedItem.ClienteXCobrarNombre = Facturas.SelectedItem.ClienteNombre;
        //}

        partial void PagoContado_Execute()
        {
            ClienteOperacion operacionXCobrar = OperacionesXCobrar.SelectedItem;
            decimal montoPagado = (from op in operacionXCobrar.OperacionesRelacionadas
                                   where op.NroAutorizacion == operacionXCobrar.NroAutorizacion && 
                                            op.FacturaNro == operacionXCobrar.FacturaNro && op.TipoOperacion == "CC" && op.Estado == "V"
                                   select op.Monto).Sum();
            MedioPago medioPago = DataWorkspace.ApplicationData.MedioPagos.FirstOrDefault();
            FormaPago = new FormaPago();
            FormaPago.Monto = operacionXCobrar.Monto - montoPagado;
            FormaPago.MontoBS = FormaPago.Monto;
            FormaPago.MedioPagoBS = medioPago;
            FormaPago.MedioPagoUS = medioPago;
            FormaPago.ConAnticipos = 0M;
            FormaPago.Codigo = Guid.NewGuid().ToString();
            FormaPago.Concepto = "Forma de Pago";
            FormaPago.Empresa = Empresa;
        }

        partial void PagoFactura_Execute()
        {
            MessageBoxResult result = this.ShowMessageBox("Confirma el registro del Cobro de esta Factura?", SoftCliente.SoftProductoNombre, MessageBoxOption.YesNo);
            if(result == MessageBoxResult.Yes)
            {
                ClienteOperacion opCobrada = OperacionesXCobrar.SelectedItem;

                ClienteOperacion opPago = OperacionesPagadas.AddNew();
                
                opPago.Cliente = opCobrada.Cliente;
                opPago.Contabilizada = false;
                opPago.Estado = "V";    // Vigente
                opPago.NroAutorizacion = opCobrada.NroAutorizacion;
                opPago.FacturaNro = opCobrada.FacturaNro;
                opPago.Fecha = DateTime.Now;
                opPago.MedioPagoBS = FormaPago.MedioPagoBS.Codigo;
                opPago.MedioPagoUS = FormaPago.MedioPagoUS.Codigo;
                opPago.Monto = FormaPago.Monto;
                opPago.MontoBS = FormaPago.MontoBS;
                opPago.MontoUS = FormaPago.MontoUS;
                opPago.TipoCambio = FormaPago.TipoCambio;
                opPago.TipoOperacion = "CC";

                if (opCobrada.Monto == OperacionesPagadas.Sum(op => op.Monto))
                {
                    Factura = (from Factura f in DataWorkspace.ApplicationData.Facturas
                               where f.Dosificacion.NroAutorizacion == opCobrada.NroAutorizacion && f.Nro == opCobrada.FacturaNro
                               select f).SingleOrDefault();
                    if(Factura != null)
                        Factura.Situacion = "P";    // Pagada
                    opCobrada.Estado = "C";     // Cancelada
                }
                
                if(FormaPago != null)
                    FormaPago.Delete();

                Save();

                this.ShowMessageBox("Cobro exitosamente registrado!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                this.Close(promptUserToSave: false);
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

            BuscarOperaciones();
        }

        partial void ResumenCaja_Execute()
        {
            Application.ShowScreenResumenCaja(RegistroCaja.Id);
        }

        partial void ShowCaja_Execute()
        {
            groupCaja.IsVisible = !groupCaja.IsVisible;
        }

        partial void ShowFactura_Execute()
        {
            if (OperacionesXCobrar.SelectedItem == null)
            {
                this.ShowMessageBox("No hay una Operación seleccionada!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }

            string[] partes = OperacionesXCobrar.SelectedItem.FacturaNro.Split('-');
            string factNro = partes[0];
            string factNroAuto = partes.Length > 1 ? partes[1] : string.Empty;
            Application.Current.ShowScreenFacturaDef(factNroAuto, factNro, HabilitarAnularDesanular: false,
                                                        HabilitarEdicion: false);
        }

        #endregion
    }
}
