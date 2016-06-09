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
    public partial class ScreenParametros
    {
        #region Propiedades
        #endregion

        #region Metodos Auxiliares
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

        partial void ScreenParametros_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();
        }

        partial void Empresas_SelectionChanged()
        {
            EmpresaNombre = Empresas.SelectedItem != null ? Empresas.SelectedItem.Nombre : SoftCliente.ClienteNombre;
        }

        partial void ParametrosAddNew_Execute()
        {
            if (Empresas.SelectedItem == null)
                return;

            Parametro par = Parametros.AddNew();
            par.Empresa = Empresas.SelectedItem;
        }

        partial void Parametros_Loaded(bool succeeded)
        {
            Mensaje = Parametros.Count == 0 ? "No se encontraron Parámetros para desplegar!" : string.Empty;
        }

        #endregion
    }
}
