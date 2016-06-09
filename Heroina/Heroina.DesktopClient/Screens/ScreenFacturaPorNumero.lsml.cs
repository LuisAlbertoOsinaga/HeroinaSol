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
    public partial class ScreenFacturaPorNumero
    {
        #region Propiedades

        IContentItemProxy btnVerFactura;

        #endregion

        #region Metodos Auxiliares
        
        void FindControls()
        {
            btnVerFactura = this.FindControl("VerFactura");
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

        partial void ScreenFacturaPorNumero_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();
            FindControls();
        }

        partial void BuscarFactura_Execute()
        {
            btnVerFactura.IsEnabled = false;
            Facturas.Load();
        }

        partial void Empresas_Loaded(bool succeeded)
        {
            EmpresaNombre = SoftCliente.NroEmpresas > 1 || Empresas.SelectedItem == null ? SoftCliente.ClienteNombre : Empresas.SelectedItem.Nombre;
        }

        partial void Facturas_Loaded(bool succeeded)
        {
            Mensaje = Facturas.Count == 0 ? "No se encontró Facturas con este número" : string.Empty;
        }

        partial void Facturas_SelectionChanged()
        {
            btnVerFactura.IsEnabled = Facturas.SelectedItem != null;
        }

        partial void VerFactura_Execute()
        {
            if(Facturas.SelectedItem != null)
                Application.Current.ShowScreenFacturaDef(Facturas.SelectedItem.Dosificacion.NroAutorizacion, 
                                                            Facturas.SelectedItem.Nro, 
                                                            HabilitarAnularDesanular: false, HabilitarEdicion: true);
        }

        #endregion
    }
}
