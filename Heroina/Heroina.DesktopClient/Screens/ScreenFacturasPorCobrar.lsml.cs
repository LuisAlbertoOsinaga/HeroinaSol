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
    public partial class ScreenFacturasPorCobrar
    {
        #region Propiedades

        IContentItemProxy comboEmpresas;
        IContentItemProxy comboSucursales;
        IContentItemProxy groupEmpresas;

        #endregion

        #region Métodos Auxiliares

        void Bindings()
        {
            comboEmpresas.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.Empresas", System.Windows.Data.BindingMode.OneWay);
            comboEmpresas.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.Empresas.SelectedItem", System.Windows.Data.BindingMode.TwoWay);

            comboSucursales.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.Sucursales", System.Windows.Data.BindingMode.OneWay);
            comboSucursales.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.Sucursales.SelectedItem", System.Windows.Data.BindingMode.TwoWay);
        }

        void Enablings()
        {
            comboEmpresas.IsEnabled = Empresas.Count > 1 && Application.Current.User.HasPermission(Permissions.CambiarEmpresa); ;
            comboSucursales.IsEnabled = Application.Current.User.HasPermission(Permissions.CambiarSucursal); ; ;
        }

        void FindControls()
        {
            comboEmpresas = this.FindControl("ComboEmpresas");
            comboSucursales = this.FindControl("ComboSucursales");
            groupEmpresas = this.FindControl("GroupEmpresas");
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

            #endregion

            #region Soft Cliente

            SoftCliente = (from s in DataWorkspace.ApplicationData.SoftClientes
                           where s.SoftProductoId == Cfg.SoftProductoId
                           select s).FirstOrDefault();
            if (SoftCliente == null)
            {
                Mensaje = "No hay registro de cliente!";
                return;
            }

            SoftProducto = Utilidades.SoftProducto(SoftCliente);

            #endregion
        }

        #endregion

        #region Metodos Generados

        partial void ScreenFacturasPorCobrar_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();

            if (!HayCajaAbierta())
            {
                this.ShowMessageBox("Para registrar un anticipo debe primero abrir su caja!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                this.Close(promptUserToSave: false);
                return;
            }

            FindControls();
            Bindings();
            Enablings();

                //DVFecha = this.FindControl("DVFecha");
                //DVFechaInicial = this.FindControl("DVFechaInicial");
                //DVFechaFinal = this.FindControl("DVFechaFinal");

                //RangoFechas = "D";
                //Fecha = Cfg.FechaHoy;
                //FechaInicial = Cfg.FechaHoy;
                //FechaFinal = Cfg.FechaHoy;
        }

        partial void CancelarCobro_Execute()
        {
            this.CloseModalWindow("ModalCobro");
        }

        partial void CalcularBS_Execute()
        {
            ClienteOperaciones.SelectedItem.MontoBS = ClienteOperaciones.SelectedItem.Monto - Math.Round(ClienteOperaciones.SelectedItem.MontoUS * ClienteOperaciones.SelectedItem.TipoCambio, 2);
        }

        partial void CalcularUS_Execute()
        {
            ClienteOperaciones.SelectedItem.MontoUS = Math.Round((ClienteOperaciones.SelectedItem.Monto - ClienteOperaciones.SelectedItem.MontoBS) / ClienteOperaciones.SelectedItem.TipoCambio, 2);
        }

        partial void CloseCobro_Execute()
        {
            if (ClienteOperaciones.SelectedItem == null)
                return;

            ClienteOperacion opCred = ClienteOperaciones.SelectedItem;
            ClienteOperacion opCobro = ClienteOperaciones.AddNew();
            opCobro.Cliente = opCred.Cliente;
            opCobro.Referencia = opCred.Referencia;
            opCobro.Estado = "V";
            opCobro.FacturaNro = opCred.FacturaNro;
            opCobro.Fecha = Cfg.FechaHoy.GetValueOrDefault();
            opCobro.MedioPagoBS = opCred.MedioPagoBS;
            opCobro.MedioPagoUS = opCred.MedioPagoUS;
            opCobro.Monto = opCred.Monto;
            opCobro.MontoBS = opCred.MontoBS;
            opCobro.MontoUS = opCred.MontoUS;
            opCobro.OperacionOrigen = opCred;
            opCobro.TipoOperacion = "CC";

            opCred.Estado = "C";
            opCred.OperacionesRelacionadas.Add(opCobro);

            this.Save();
            ClienteOperaciones.Load();

            this.CloseModalWindow("ModalCobro");
        }

        partial void Cobrar_Execute()
        {
            if (ClienteOperaciones.SelectedItem == null)
                return;

            if (ClienteOperaciones.SelectedItem.TipoCambio != Cfg.TipoCambio)
                ClienteOperaciones.SelectedItem.TipoCambio = Cfg.TipoCambio;
            this.OpenModalWindow("ModalCobro");
        }

        partial void Empresas_SelectionChanged()
        {
            EmpresaNombre = Empresas.SelectedItem != null && Empresas.Count == 1 ? Empresas.SelectedItem.Nombre : SoftCliente.ClienteNombre;
            //groupEmpresas.IsVisible = Empresas.Count > 1;
        }

        //partial void RangoFechas_Changed()
        //{
        //    if (RangoFechas == "D" || RangoFechas == "S" || RangoFechas == "M" || RangoFechas == "A")
        //    {
        //        DVFecha.IsVisible = true;
        //        DVFechaInicial.IsVisible = false;
        //        DVFechaFinal.IsVisible = false;
        //    }
        //    else if (RangoFechas == "F")
        //    {
        //        DVFecha.IsVisible = false;
        //        DVFechaInicial.IsVisible = true;
        //        DVFechaFinal.IsVisible = true;
        //    }
        //    else    // RangoFechas == "T"
        //    {
        //        DVFecha.IsVisible = false;
        //        DVFechaInicial.IsVisible = false;
        //        DVFechaFinal.IsVisible = false;
        //    }
        //}

        //partial void Generar_Execute()
        //{
        //    if (RangoFechas == "D")
        //    {
        //        COFechaIni = new DateTime(Fecha.Value.Year, Fecha.Value.Month, Fecha.Value.Day, 0, 0, 0);
        //        COFechaFin = new DateTime(Fecha.Value.Year, Fecha.Value.Month, Fecha.Value.Day, 23, 59, 59);
        //    }
        //    else if (RangoFechas == "S")
        //    {
        //        COFechaIni = new DateTime(Fecha.Value.Year, Fecha.Value.Month, Fecha.Value.Day, 0, 0, 0);
        //        COFechaFin = new DateTime(Fecha.Value.Year, Fecha.Value.Month, Fecha.Value.Day, 23, 59, 59);
        //        while (COFechaIni.Value.DayOfWeek != DayOfWeek.Monday)
        //            COFechaIni = COFechaIni.Value.AddDays(-1);
        //        while (COFechaFin.Value.DayOfWeek != DayOfWeek.Sunday)
        //            COFechaFin = COFechaFin.Value.AddDays(1);
        //    }
        //    else if (RangoFechas == "M")
        //    {
        //        COFechaIni = new DateTime(Fecha.Value.Year, Fecha.Value.Month, 1, 0, 0, 0);
        //        COFechaFin = COFechaIni.Value.AddMonths(1).AddDays(-1);
        //        COFechaFin = new DateTime(COFechaFin.Value.Year, COFechaFin.Value.Month, COFechaFin.Value.Day, 23, 59, 59);
        //    }
        //    else if (RangoFechas == "A")
        //    {
        //        COFechaIni = new DateTime(Fecha.Value.Year, 1, 1, 0, 0, 0);
        //        COFechaFin = new DateTime(Fecha.Value.Year, 12, 31, 23, 59, 59);
        //    }
        //    else if (RangoFechas == "T")
        //    {
        //        COFechaIni = new DateTime(2000, 1, 1, 0, 0, 0);
        //        COFechaFin = new DateTime(3000, 12, 31, 23, 59, 59);
        //    }
        //    else    // RangoFechas == "F"
        //    {
        //        COFechaIni = FechaInicial;
        //        COFechaFin = FechaFinal;
        //    }

        //    Mensaje = ClienteOperaciones.Count == 0 ? "No hay facturas por cobrar en este rango de fechas!" : string.Empty;
        //}

        #endregion
    }
}
