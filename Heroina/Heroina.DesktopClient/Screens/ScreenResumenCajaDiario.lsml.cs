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
    public partial class ScreenResumenCajaDiario
    {
        #region Propiedades

        IContentItemProxy comboAutoimpresores;
        IContentItemProxy comboEmpresas;
        IContentItemProxy comboSucursales;

        IContentItemProxy groupResumen;

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

        private void ConsigueResumenCaja(Guid guid)
        {
            ResumenCaja = (from r in DataWorkspace.ApplicationData.ResumenCajas
                            where r.Guid == guid
                            select r).FirstOrDefault();
            if (ResumenCaja == null)
                this.ShowMessageBox(string.Format("Registro Resumen de Caja no encontrado. {0}", guid.ToString()));
        }

        void FindControls()
        {
            comboAutoimpresores = this.FindControl("ComboAutoimpresores");
            comboEmpresas = this.FindControl("ComboEmpresas");
            comboSucursales = this.FindControl("ComboSucursales");

            groupResumen = this.FindControl("GroupResumen");
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

        void Inits()
        {
            Nivel = "E";
        }

        #endregion

        #region Metodos Generados

        partial void ScreenResumenCajaDiario_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();
            FindControls();
            Bindings();
            Inits();
        }

        partial void Empresas_SelectionChanged()
        {
            EmpresaNombre = Empresas.SelectedItem != null && Empresas.Count == 1 ? Empresas.SelectedItem.Nombre : SoftCliente.ClienteNombre;
        }

        partial void Nivel_Changed()
        {
            if(Nivel == "P")    // Puesto
            {
                comboEmpresas.IsVisible = true;
                comboSucursales.IsVisible = true;
                comboAutoimpresores.IsVisible = true;
            }
            else if(Nivel == "S")    // Sucursal
            {
                comboEmpresas.IsVisible = true;
                comboSucursales.IsVisible = true;
                comboAutoimpresores.IsVisible = false;
            }
            else    // Empresa
            {
                comboEmpresas.IsVisible = true;
                comboSucursales.IsVisible = false;
                comboAutoimpresores.IsVisible = false;
            }
        }

        partial void ResumenDiario_Execute()
        {
            groupResumen.IsVisible = false;

            int organizacionId = '0';
            if(Nivel == "P")
                organizacionId = Autoimpresores.SelectedItem.Id;
            else if(Nivel == "S")
                organizacionId = Sucursales.SelectedItem.Id;
            else
                organizacionId = Empresas.SelectedItem.Id;

            Guid guid = Guid.NewGuid();

            Proceso proceso = DataWorkspace.ApplicationData.Procesos.AddNew();
            proceso.Empresa = Empresas.SelectedItem;
            proceso.Descripcion = "RESUMEN DIARIO DE CAJA";
            proceso.Data = Empresas.SelectedItem.Nombre + "|" + Nivel + "|" + organizacionId.ToString() + "|" + 
                            Fecha.ToString("dd/MM/yyyy") + "|" + guid.ToString();
            Save();

            groupResumen.DisplayName = "Resumen Diario de Caja al " + Fecha.ToString("dd/MM/yyyy");
            groupResumen.IsVisible = true;
            ConsigueResumenCaja(guid);
        }

        #endregion
    }
}
