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
    public partial class GridFacturaTipos
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

        partial void GridFacturaTipos_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();
            FindControls();
            Bindings();
            Enablings();
        }

        partial void CerrarEdicion_Execute()
        {
            this.CloseModalWindow("DatosTipoFactura");
        }

        partial void FacturaTipos_Loaded(bool succeeded)
        {
            Mensaje = FacturaTipos.Count == 0 ? "No hay Tipos de Facturas definidas para desplegar!" : string.Empty;
            btnEditar.IsEnabled = FacturaTipos.Count > 0;
        }

        partial void FacturaTipos_SelectionChanged()
        {
            btnEditar.IsEnabled = FacturaTipos.SelectedItem != null;
        }

        partial void Editar_Execute()
        {
            this.OpenModalWindow("DatosTipoFactura");
        }

        partial void Empresas_SelectionChanged()
        {
            EmpresaNombre = Empresas.SelectedItem != null && Empresas.Count == 1 ? Empresas.SelectedItem.Nombre : SoftCliente.ClienteNombre;
            comboEmpresas.IsVisible = Empresas.Count > 1;
        }

        partial void GridFacturaTipos_Saved()
        {
            this.Refresh();
        }

        partial void SalvarEdicion_Execute()
        {
            this.CloseModalWindow("DatosTipoFactura");
            Save();
        }

        #endregion
    }
}
