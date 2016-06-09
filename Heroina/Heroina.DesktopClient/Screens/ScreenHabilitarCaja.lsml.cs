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
    public partial class ScreenHabilitarCaja
    {
        #region Propiedades

        IContentItemProxy comboAutoimpresores;
        IContentItemProxy comboEmpresas;
        IContentItemProxy comboSucursales;

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

        void ResumenCaja()
        {
            if (Empresas.SelectedItem == null || Autoimpresores.SelectedItem == null)
                return;

            Configuracion cnfg = (from c in DataWorkspace.ApplicationData.Configuracions
                                  where c.Empresa == Empresas.SelectedItem.Nombre &&
                                        c.Sucursal == Sucursales.SelectedItem.Nombre &&
                                        c.Autoimpresor == Autoimpresores.SelectedItem.Nombre
                                  select c).SingleOrDefault();
            if(cnfg == null || cnfg.RegistroCaja == 0)
            {
                RegistroCaja = null;
                return;
            }


            RegistroCaja = (from rc in DataWorkspace.ApplicationData.RegistroCajas
                            where rc.Id == cnfg.RegistroCaja
                            select rc).SingleOrDefault();

            Proceso proceso = DataWorkspace.ApplicationData.Procesos.AddNew();
            proceso.Empresa = Empresas.SelectedItem;
            proceso.Descripcion = "RESUMEN CAJA";
            proceso.Data = RegistroCaja.Id.ToString();
            DataWorkspace.ApplicationData.SaveChanges();

            RegistroCaja = (from rc in DataWorkspace.ApplicationData.RegistroCajas
                                where rc.Id == cnfg.RegistroCaja &&
                                    rc.CajaAbierta == true
                            select rc).SingleOrDefault();
        }

        #endregion

        #region Metodos Generados

        partial void ScreenHabilitarCaja_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();
            FindControls();
            Bindings();
            Enablings();
        }

        partial void CancelarHabilitacion_Execute()
        {
            this.CloseModalWindow("DatosCaja");
        }

        partial void ConfirmarHabilitacion_Execute()
        {
            this.CloseModalWindow("DatosCaja");
            RegistroCaja.Delete();
            Cfg.RegistroCaja = 0;
            Save();

            this.ShowMessageBox("Caja exitosamente habilitada!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
            this.Close(promptUserToSave: false);
        }

        partial void Empresas_SelectionChanged()
        {
            EmpresaNombre = Empresas.SelectedItem != null && Empresas.Count == 1 ? Empresas.SelectedItem.Nombre : SoftCliente.ClienteNombre;
            comboEmpresas.IsVisible = Empresas.Count > 1;
        }

        partial void HabilitarCaja_Execute()
        {
            ResumenCaja();
            if(RegistroCaja == null)
            {
                this.ShowMessageBox("No hay caja abierta en este puesto?", SoftCliente.SoftProductoNombre,
                                                            MessageBoxOption.Ok);
                return;
            }
            if(RegistroCaja.TotalFacturadoBS != 0 || RegistroCaja.TotalAnticiposBS != 0 || RegistroCaja.TotalCobranzasBS != 0)
            {
                this.ShowMessageBox("Caja ya tiene movimiento. Utilice la opción de 'Cierre de Caja'?", SoftCliente.SoftProductoNombre,
                                                            MessageBoxOption.Ok);
                return;
            }

            this.OpenModalWindow("DatosCaja");
        }

        #endregion
    }
}
