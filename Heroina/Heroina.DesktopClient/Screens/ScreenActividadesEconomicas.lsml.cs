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
    public partial class ScreenActividadesEconomicas
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

        partial void ScreenActividadesEconomicas_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();
            FindControls();
            Bindings();
        }

        partial void ActividadesEconomicasAddNew_Execute()
        {
            if (Empresas.SelectedItem == null)
                return;

            ActividadEconomica ae = ActividadesEconomicas.AddNew();
            ae.Empresa = Empresas.SelectedItem;
        }

        partial void ActividadesEconomicas_Loaded(bool succeeded)
        {
            Mensaje = ActividadesEconomicas.Count == 0 ? "No hay Actividades Económicas para desplegar!" : string.Empty;
            btnEditar.IsEnabled = ActividadesEconomicas.Count > 0;
        }

        partial void ActividadesEconomicas_SelectionChanged()
        {
            btnEditar.IsEnabled = ActividadesEconomicas.SelectedItem != null;
        }

        partial void CerrarEdicion_Execute()
        {
            this.CloseModalWindow("ModalEdicion");
        }

        partial void Editar_Execute()
        {
            this.OpenModalWindow("ModalEdicion");
        }

        partial void Empresas_SelectionChanged()
        {
            EmpresaNombre = Empresas.SelectedItem != null && Empresas.Count == 1 ? Empresas.SelectedItem.Nombre : SoftCliente.ClienteNombre;
            comboEmpresas.IsVisible = Empresas.Count > 1;
        }

        partial void SalvarEdicion_Execute()
        {
            Save();
            this.CloseModalWindow("ModalEdicion");
        }

        partial void ScreenActividadesEconomicas_Saved()
        {
            this.Refresh();
        }

        #endregion
    }
}
