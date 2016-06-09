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
    public partial class GridDosificaciones
    {
        #region Propiedades

        IContentItemProxy btnEditar;
        IContentItemProxy comboEmpresas;
        IContentItemProxy comboSucursales;
        IContentItemProxy comboAutoimpresores;
        IContentItemProxy groupDetalleEdit;
        IContentItemProxy groupDetalleNoEdit;

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
            comboEmpresas.IsEnabled = Empresas.Count > 1 && Application.Current.User.HasPermission(Permissions.CambiarEmpresa);
            comboEmpresas.IsVisible = Empresas.Count > 1;

            comboSucursales.IsEnabled = Application.Current.User.HasPermission(Permissions.CambiarSucursal);
            comboAutoimpresores.IsEnabled = Application.Current.User.HasPermission(Permissions.CambiarAutoimpresor);

            groupDetalleEdit.IsVisible = Application.Current.User.HasPermission(Permissions.EditarDosificaciones);
            groupDetalleNoEdit.IsVisible = !Application.Current.User.HasPermission(Permissions.EditarDosificaciones);
        }

        void FindControls()
        {
            btnEditar = this.FindControl("Editar");
            comboAutoimpresores = this.FindControl("ComboAutoimpresores");
            comboEmpresas = this.FindControl("ComboEmpresas");
            comboSucursales = this.FindControl("ComboSucursales");
            groupDetalleEdit = this.FindControl("Dosificaciones_SelectedItem_Edit");
            groupDetalleNoEdit = this.FindControl("Dosificaciones_SelectedItem_NoEdit");
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

        partial void GridDosificaciones_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();
            FindControls();
            Bindings();
            Enablings();
        }

        partial void CerrarEdicion_Execute()
        {
            this.CloseModalWindow("DatosDosificacion");
        }

        partial void DosificacionesAddNew_CanExecute(ref bool result)
        {
            result = Application.Current.User.HasPermission(Permissions.EditarDosificaciones);
        }

        partial void DosificacionesAddNew_Execute()
        {
            Dosificacion dosi = Dosificaciones.AddNew();
            dosi.Autoimpresor = Autoimpresores.SelectedItem;
            dosi.Leyenda = Cfg.Leyenda;
        }

        partial void Dosificaciones_Loaded(bool succeeded)
        {
            Mensaje = Dosificaciones.Count == 0 ? "No hay Dosificaciones definidas para desplegar!" : string.Empty;
            btnEditar.IsEnabled = Dosificaciones.Count > 0;
        }

        partial void Dosificaciones_SelectionChanged()
        {
            btnEditar.IsEnabled = Dosificaciones.SelectedItem != null;
        }

        partial void Editar_Execute()
        {
            this.OpenModalWindow("DatosDosificacion");
        }

        partial void Empresas_SelectionChanged()
        {
            EmpresaNombre = Empresas.SelectedItem != null && Empresas.Count == 1 ? Empresas.SelectedItem.Nombre : SoftCliente.ClienteNombre;
            comboEmpresas.IsVisible = Empresas.Count > 1;
        }

        partial void GridDosificaciones_Saving(ref bool handled)
        {
            foreach (var item in Dosificaciones)
            {
                if (string.IsNullOrWhiteSpace(item.NroAutorizacion) || item.NroAutorizacion.Length < 10)
                {
                    MessageBoxResult result = this.ShowMessageBox(string.Format("Nro. de Autorización muy corto, desea revisarlo '{0}'", item.NroAutorizacion),
                                                                    SoftCliente.SoftProductoNombre, MessageBoxOption.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        handled = true;
                        break;
                    }
                }
            }
        }

        partial void GridDosificaciones_Saved()
        {
            this.Refresh();
        }

        partial void SalvarEdicion_Execute()
        {
            this.CloseModalWindow("DatosDosificacion");
            Save();
        }

        #endregion
    }
}
