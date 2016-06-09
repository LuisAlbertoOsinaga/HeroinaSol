using System;
using System.Linq;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using Microsoft.LightSwitch;
using Microsoft.LightSwitch.Framework.Client;
using Microsoft.LightSwitch.Presentation;
using Microsoft.LightSwitch.Presentation.Extensions;
using System.Collections.Specialized;

namespace LightSwitchApplication
{
    public partial class GridSucursales
    {
        #region Propiedades

        IContentItemProxy btnEditar;
        IContentItemProxy comboEmpresas;

        #endregion

        #region Metodos Auxiliares

        void Bindings()
        {
            comboEmpresas.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.Empresas", System.Windows.Data.BindingMode.OneWay);
            comboEmpresas.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.Empresas.SelectedItem", System.Windows.Data.BindingMode.TwoWay);
        }

        void Enablings()
        {
            comboEmpresas.IsEnabled = Empresas.Count > 1 && Application.Current.User.HasPermission(Permissions.CambiarEmpresa);
            comboEmpresas.IsVisible = Empresas.Count > 1;
        }

        void FindControls()
        {
            btnEditar = this.FindControl("Editar");
            comboEmpresas = this.FindControl("ComboEmpresas");
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

        partial void GridSucursales_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();
            FindControls();
            Bindings();
            Enablings();
        }

        partial void CerrarEdicion_Execute()
        {
            this.CloseModalWindow("DatosSucursal");
        }

        partial void Editar_Execute()
        {
            this.OpenModalWindow("DatosSucursal");
        }

        partial void Empresas_SelectionChanged()
        {
            EmpresaNombre = Empresas.SelectedItem != null && Empresas.Count == 1? Empresas.SelectedItem.Nombre : SoftCliente.ClienteNombre;
            comboEmpresas.IsVisible = Empresas.Count > 1;
        }

        partial void GridSucursales_Saved()
        {
            this.Refresh();
        }

        partial void SalvarEdicion_Execute()
        {
            this.CloseModalWindow("DatosSucursal");
            Save();
        }

        partial void Sucursales_Loaded(bool succeeded)
        {
            Mensaje = Sucursales.Count == 0 ? "No hay Sucursales definidas para desplegar!" : string.Empty;
            btnEditar.IsEnabled = Sucursales.Count > 0;
        }

        partial void Sucursales_SelectionChanged()
        {
            btnEditar.IsEnabled = Sucursales.SelectedItem != null;
        }

        #endregion
    }
}
