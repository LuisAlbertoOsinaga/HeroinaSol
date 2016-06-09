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
    public partial class GridEmpresas
    {
        #region Propiedades

        IContentItemProxy btnEditar;

        #endregion

        #region Metodos Auxiliares

        void FindControls()
        {
            btnEditar = this.FindControl("Editar");
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
         
        partial void GridEmpresas_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();
            FindControls();
        }

        partial void CerrarEdicion_Execute()
        {
            this.CloseModalWindow("DatosEmpresa");
        }

        partial void Editar_Execute()
        {
            this.OpenModalWindow("DatosEmpresa");
        }

        partial void Empresas_Loaded(bool succeeded)
        {
            Mensaje = Empresas.Count == 0 ? "No hay Empresas definidas para desplegar!" : string.Empty;
            btnEditar.IsEnabled = Empresas.Count > 0;
        }

        partial void Empresas_SelectionChanged()
        {
            btnEditar.IsEnabled = Empresas.SelectedItem != null;
        }

        partial void GridEmpresas_Saved()
        {
            this.Refresh();
        }

        partial void SalvarEdicion_Execute()
        {
            this.CloseModalWindow("DatosEmpresa");
            Save();
        }

        #endregion
    }
}
