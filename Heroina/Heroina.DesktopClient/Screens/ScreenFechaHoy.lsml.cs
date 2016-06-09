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
    public partial class ScreenFechaHoy
    {
        #region Metodos especiales

        void CloseAllScreens()
        {
            var screens = this.Application.ActiveScreens;
            foreach (var s in screens)
            {
                var screen = s.Screen;
                screen.Details.Dispatcher.BeginInvoke( () => { screen.Close(promptUserToSave: false); } ); 
            }        
        }

        #endregion

        partial void ScreenFechaHoy_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            Mensaje = string.Empty;

            // Configuracion
            string id = ServicesClient.ServicioFacturacion.GetInstalacionId();
            if(string.IsNullOrWhiteSpace(id))
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

            // Soft Cliente
            SoftCliente = (from s in DataWorkspace.ApplicationData.SoftClientes
                           where s.SoftProductoId == Cfg.SoftProductoId
                           select s).FirstOrDefault();
            if (SoftCliente == null)
            {
                Mensaje = "No hay registro de cliente!";
                return;
            }

            // Fecha Hoy   
            FechaHoy = Cfg.FechaHoy ?? DateTime.Now;
            var btnActualiza = this.FindControl("ActualizarFecha");
            btnActualiza.IsVisible = true;
        }

        partial void ActualizarFecha_Execute()
        {
            Cfg.FechaHoy = FechaHoy;
            this.Save();
            this.CloseAllScreens();
        }
    }
}
