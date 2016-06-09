using Microsoft.LightSwitch;
using Microsoft.LightSwitch.Presentation;
using Microsoft.LightSwitch.Presentation.Extensions;
using System;
using System.Collections.Generic;
using System.Windows;

namespace LightSwitchApplication
{
    public partial class ScreenCerrarCaja
    {
        #region Propiedades

        IContentItemProxy btnCerrarCaja;
        IContentItemProxy comboEmpresas;
        IContentItemProxy comboSucursales;
        IContentItemProxy comboAutoimpresores;
        
        #endregion

        #region Metodos Auxiliares

        void Bindings()
        {
            comboEmpresas.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.Empresas", System.Windows.Data.BindingMode.OneWay);
            comboEmpresas.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.Empresas.SelectedItem", System.Windows.Data.BindingMode.TwoWay);
            comboSucursales.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.Sucursales", System.Windows.Data.BindingMode.OneWay);
            comboSucursales.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.Sucursales.SelectedItem", System.Windows.Data.BindingMode.TwoWay);
            comboAutoimpresores.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.Autoimpresores", System.Windows.Data.BindingMode.OneWay);
            comboAutoimpresores.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.Autoimpresores.SelectedItem", System.Windows.Data.BindingMode.TwoWay);
        }

        private void CierraCaja(RegistroCaja regCaja, Configuracion cfgCaja)
        {
            regCaja.HoraFinal = DateTime.Now;
            regCaja.CajaAbierta = false;
            cfgCaja.RegistroCaja = 0;
            Save();
        }

        void Enablings()
        {
            btnCerrarCaja.IsEnabled = false;
        }

        void FindControls()
        {
            btnCerrarCaja = this.FindControl("CerrarCaja");
            comboEmpresas = this.FindControl("ComboEmpresas");
            comboSucursales = this.FindControl("ComboSucursales");
            comboAutoimpresores = this.FindControl("ComboAutoimpresores");
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

        #endregion

        #region Metodos Generados

        partial void ScreenCerrarCaja_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();
            FindControls();
            Bindings();
            Enablings();
        }

        partial void Autoimpresores_SelectionChanged()
        {
            RegistroCaja = null;
            Autoimpresor puestoSelected = Autoimpresores.SelectedItem;
            Sucursal sucursalSelected = Sucursales.SelectedItem;
            Empresa empresaSelected = Empresas.SelectedItem;

            if (puestoSelected == null)
                return;

            CfgPuesto = (from cfg in DataWorkspace.ApplicationData.Configuracions
                                                where cfg.Empresa == empresaSelected.Nombre &&
                                                        cfg.Sucursal == sucursalSelected.Nombre &&
                                                        cfg.Autoimpresor == puestoSelected.Nombre
                                                select cfg).SingleOrDefault();
            if (CfgPuesto != null)
            {
                RegistroCaja = (from reg in DataWorkspace.ApplicationData.RegistroCajas
                                        where reg.Id == CfgPuesto.RegistroCaja && reg.CajaAbierta
                                        select reg).SingleOrDefault();
            }

            Mensaje = RegistroCaja == null ? "No hay caja abierta en este Puesto!" : string.Empty;
            btnCerrarCaja.IsEnabled = RegistroCaja != null;
        }

        partial void CerrarCaja_Execute()
        {
            MessageBoxResult result = this.ShowMessageBox("Desea imprimir 'Resúmen de Caja'?", SoftCliente.SoftProductoNombre, MessageBoxOption.YesNo);
            if (result == MessageBoxResult.Yes)
                Utilidades.PrintResumenCaja(RegistroCaja, paraCierreCaja: true);
            CierraCaja(RegistroCaja, CfgPuesto);
            this.Save();
            this.ShowMessageBox("Caja exitosamente cerrada!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
            this.Close(promptUserToSave: false);
        }

        partial void Empresas_SelectionChanged()
        {
            EmpresaNombre = Empresas.SelectedItem != null ? Empresas.SelectedItem.Nombre : string.Empty;
            comboEmpresas.IsVisible = Empresas.SelectedItem != null && Empresas.Count > 1;
        }

        #endregion
    }
}
