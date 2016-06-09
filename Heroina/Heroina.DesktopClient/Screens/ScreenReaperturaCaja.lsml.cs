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
    public partial class ScreenReaperturaCaja
    {
        #region Propiedades

        IContentItemProxy comboAutoimpresores;
        IContentItemProxy comboEmpresas;
        IContentItemProxy comboSucursales;
        IContentItemProxy comboTurnos;

        #endregion

        #region Metodos Auxiliares

        void Bindings()
        {
            comboAutoimpresores.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.Autoimpresores", System.Windows.Data.BindingMode.OneWay);
            comboAutoimpresores.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.Autoimpresores.SelectedItem", System.Windows.Data.BindingMode.TwoWay);

            comboEmpresas.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.Empresas", System.Windows.Data.BindingMode.OneWay);
            comboEmpresas.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.Empresas.SelectedItem", System.Windows.Data.BindingMode.TwoWay);

            comboSucursales.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.Sucursales", System.Windows.Data.BindingMode.OneWay);
            comboSucursales.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.Sucursales.SelectedItem", System.Windows.Data.BindingMode.TwoWay);

            comboTurnos.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.Turnos", System.Windows.Data.BindingMode.OneWay);
            comboTurnos.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.Turnos.SelectedItem", System.Windows.Data.BindingMode.TwoWay);
        }

        void Enablings()
        {
            comboAutoimpresores.IsEnabled = Application.Current.User.HasPermission(Permissions.CambiarAutoimpresor);
            comboEmpresas.IsEnabled = Empresas.Count > 1 && Application.Current.User.HasPermission(Permissions.CambiarEmpresa);
            comboSucursales.IsEnabled = Application.Current.User.HasPermission(Permissions.CambiarSucursal); ;
        }

        void FindControls()
        {
            comboAutoimpresores = this.FindControl("ComboAutoimpresores");
            comboEmpresas = this.FindControl("ComboEmpresas");
            comboSucursales = this.FindControl("ComboSucursales");
            comboTurnos = this.FindControl("ComboTurnos");
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

        partial void ScreenReaperturaCaja_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();
            FindControls();
            Bindings();
            Enablings();
        }

        partial void CancelarReaperturaCaja_Execute()
        {
            this.CloseModalWindow("DatosCaja");
        }

        partial void ConfirmarReaperturaCaja_Execute()
        {
            this.CloseModalWindow("DatosCaja");
            RegistroCaja.HoraFinal = RegistroCaja.HoraInicio;
            RegistroCaja.CajaAbierta = true;
            Cfg.RegistroCaja = RegistroCaja.Id;
            Save();

            this.ShowMessageBox("Caja exitosamente reabierta!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
            this.Close(promptUserToSave: false);
        }

        partial void Empresas_SelectionChanged()
        {
            EmpresaNombre = Empresas.SelectedItem != null && Empresas.Count == 1 ? Empresas.SelectedItem.Nombre : SoftCliente.ClienteNombre;
            comboEmpresas.IsVisible = Empresas.Count > 1;
        }

        partial void ReaperturaCaja_Execute()
        {
            if(Empresas.SelectedItem == null)
            {
                this.ShowMessageBox("No hay Empresa seleccionada!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }
            if (Sucursales.SelectedItem == null)
            {
                this.ShowMessageBox("No hay Sucursal seleccionada!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }
            if (Autoimpresores.SelectedItem == null)
            {
                this.ShowMessageBox("No hay Puesto seleccionada!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }
            if (Turnos.SelectedItem == null)
            {
                this.ShowMessageBox("No hay Turno seleccionado!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }

            Configuracion cfgPuesto = (from c in DataWorkspace.ApplicationData.Configuracions
                                       where c.Empresa == Empresas.SelectedItem.Nombre &&
                                                c.Sucursal == Sucursales.SelectedItem.Nombre &&
                                                c.Autoimpresor == Autoimpresores.SelectedItem.Nombre
                                       select c).SingleOrDefault();
            if(cfgPuesto == null)
            {
                this.ShowMessageBox("No hay archivo de Configuración con estos datos!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }

            if(cfgPuesto.RegistroCaja > 0)
            {
                this.ShowMessageBox("Hay una Caja abierta en este Puesto! Cierre primero esa caja...", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }

            RegistroCaja = (from rc in DataWorkspace.ApplicationData.RegistroCajas
                            where rc.Autoimpresor.Sucursal.Empresa.Id == Empresas.SelectedItem.Id &&
                                    rc.Autoimpresor.Sucursal.Id == Sucursales.SelectedItem.Id &&
                                    rc.Autoimpresor.Id == Autoimpresores.SelectedItem.Id &&
                                    rc.Fecha == Fecha &&
                                    rc.Turno.Id == Turnos.SelectedItem.Id &&
                                    rc.CajaAbierta == false
                            select rc).SingleOrDefault();
            if (RegistroCaja == null)
            {
                this.ShowMessageBox("No hay Caja cerrada con estos datos?", SoftCliente.SoftProductoNombre,
                                                            MessageBoxOption.Ok);
                return;
            }

            this.OpenModalWindow("DatosCaja");
        }

        #endregion
    }
}
