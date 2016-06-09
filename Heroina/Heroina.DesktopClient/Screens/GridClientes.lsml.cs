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
    public partial class GridClientes
    {
        #region Propiedades

        IContentItemProxy groupEmpresas;

        #endregion

        #region Metodos Auxiliares

        void Bindings()
        {
        }

        void Enablings()
        {
        }

        void FindControls()
        {
            groupEmpresas = this.FindControl("GroupEmpresas");
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

        bool Inits()
        {
            Empresa = (from e in DataWorkspace.ApplicationData.Empresas
                       where e.Nombre == Cfg.Empresa
                       select e).FirstOrDefault();
            EmpresaNombre = SoftCliente.NroEmpresas > 1 || Empresa == null ? SoftCliente.ClienteNombre : Empresa.Nombre;
            groupEmpresas.IsVisible = SoftCliente.NroEmpresas > 1;
            return true;
        }

        #endregion

        #region Metodos Generados

        partial void GridClientes_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();

            FindControls();
            Bindings();
            Inits();
            Enablings();
        }

        partial void gridAddNew_Execute()
        {
            Cliente cli = Clientes.AddNew();
            cli.Empresa = Empresa;
        }

        #endregion
    }
}
