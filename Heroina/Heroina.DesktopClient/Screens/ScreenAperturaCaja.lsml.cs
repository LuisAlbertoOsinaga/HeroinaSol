using System;
using System.Linq;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using Microsoft.LightSwitch;
using Microsoft.LightSwitch.Framework.Client;
using Microsoft.LightSwitch.Presentation;
using Microsoft.LightSwitch.Presentation.Extensions;

using System.Windows.Controls;
using System.Windows.Media;

namespace LightSwitchApplication
{
    public partial class ScreenAperturaCaja
    {
        #region Propiedades

        IContentItemProxy comboTurnos;
        IContentItemProxy groupCaja;
        IContentItemProxy groupEmpresas;
                
        #endregion

        #region Metodos Auxiliares

        void Bindings()
        {
            comboTurnos.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.Turnos", System.Windows.Data.BindingMode.OneWay);
            comboTurnos.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.Turnos.SelectedItem", System.Windows.Data.BindingMode.TwoWay);

            groupCaja.ControlAvailable += GroupCaja_ControlAvailable;
        }

        void Enablings()
        {
        }

        void FindControls()
        {
            comboTurnos = this.FindControl("ComboTurnos");
            groupCaja = this.FindControl("GroupCaja");
            groupEmpresas = this.FindControl("GroupEmpresas");
        }

        private void GroupCaja_ControlAvailable(object sender, ControlAvailableEventArgs e)
        {
            Control box = (Control)e.Control;
            box.Background = new SolidColorBrush(Color.FromArgb(255, Globales.CajaColorRed , Globales.CajaColorGreen, Globales.CajaColorBlue));
        }

        bool HayCajaAbierta()
        {
            if (Cfg.RegistroCaja == 0)
                return false;

            RegistroCaja rc = DataWorkspace.ApplicationData.RegistroCajas_SingleOrDefault(Cfg.RegistroCaja);
            return (rc != null && rc.CajaAbierta);
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

        bool Inits()
        {
            Autoimpresor = (from a in DataWorkspace.ApplicationData.Autoimpresores
                            where a.Nombre == Cfg.Autoimpresor && a.Sucursal.Nombre == Cfg.Sucursal &&
                                  a.Sucursal.Empresa.Nombre == Cfg.Empresa
                            select a).FirstOrDefault();
            if(Autoimpresor == null)
            {
                this.ShowMessageBox("no hay registro de Autoimpresor!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                Close(promptUserToSave: false);
                return false;
            }

            Sucursal = Autoimpresor.Sucursal;
            Empresa = Autoimpresor.Sucursal.Empresa;
            EmpresaNombre = SoftCliente.NroEmpresas > 1 || Empresa == null ? SoftCliente.ClienteNombre : Empresa.Nombre;
            groupEmpresas.IsVisible = SoftCliente.NroEmpresas > 1;
            Fecha = DateTime.Now;
            Usuario = Application.Current.User.Name;
            
            return true;
        }

        #endregion

        #region Metodos Generados

        partial void ScreenAperturaCaja_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();

            if (HayCajaAbierta())
            {
                this.ShowMessageBox("Caja ya abierta!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                this.Close(promptUserToSave: false);
                return;
            }

            FindControls();
            Bindings();
            if (!Inits())
            {
                this.ShowMessageBox("Error al iniciar pantalla!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                this.Close(promptUserToSave: false);
                return;
            }
            Enablings();
        }

        partial void AperturaCaja_Execute()
        {
            HoraInicio = DateTime.Now;
            this.OpenModalWindow("DatosApertura");
        }

        partial void CancelarApertura_Execute()
        {
            this.CloseModalWindow("DatosApertura");
        }

        partial void ConfirmarApertura_Execute()
        {
            this.CloseModalWindow("DatosApertura");
            RegistroCaja registroCaja = null;

            // Busca registro
            registroCaja = (from RegistroCaja r in DataWorkspace.ApplicationData.RegistroCajas
                            where r.Autoimpresor.Id == Autoimpresor.Id && r.Fecha == Fecha && r.Turno.Codigo == Turnos.SelectedItem.Codigo
                            select r).FirstOrDefault();

            if(registroCaja != null)
            {
                this.ShowMessageBox("Caja ya fue abierta en este puesto, fecha y turno", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }

            registroCaja = DataWorkspace.ApplicationData.RegistroCajas.AddNew();

            registroCaja.Autoimpresor = Autoimpresor;
            registroCaja.Turno = Turnos.SelectedItem;
            registroCaja.Usuario = Usuario;
            registroCaja.Fecha = Fecha;
            registroCaja.TipoCambio = Cfg.TipoCambio;
            registroCaja.HoraInicio = HoraInicio;
            registroCaja.HoraFinal = HoraInicio;
            registroCaja.CajaAbierta = true;
            try
            {
                Save();
            }
            catch (Exception)
            {
                registroCaja.Delete();
                throw;
            }

            Cfg.FechaHoy = registroCaja.Fecha;
            Cfg.RegistroCaja = registroCaja.Id;
            Save();

            // Parametros Mails
            List<Parametro> pars;
            pars = (from Parametro p in DataWorkspace.ApplicationData.Parametros
                    where p.Empresa.Id == Empresa.Id && p.Categoria == "SMTP_ENABLED"
                    select p).ToList();
            Parametro parEnabled = pars.FirstOrDefault(p => p.Clave == "ENABLED");
            bool smtpEnabled = parEnabled != null && parEnabled.Valor == "S";
            Parametro parCajaEnabled = pars.FirstOrDefault(p => p.Clave == "CAJA_ENABLED");
            bool smtpCajaEnabled = parCajaEnabled != null && parCajaEnabled.Valor == "S";
            Parametro parAperturaEnabled = pars.FirstOrDefault(p => p.Clave == "APERTURA_ENABLED");
            bool smtpAperturaEnabled = parAperturaEnabled != null && parAperturaEnabled.Valor == "S";

            // Send Mails
            if (smtpEnabled && smtpCajaEnabled && smtpAperturaEnabled)
            {
                string body = "CAJA ABIERTA";

                if (registroCaja != null)
                {
                    body += string.Format("\n\nSucursal: {0}\nPuesto: {1}\nTurno: {2}\nFecha: {3}\nUsuario: {4}\nT/C: {5}",
                                                     registroCaja.Autoimpresor.Sucursal.Nombre,
                                                     registroCaja.Autoimpresor.Nombre,
                                                     registroCaja.Turno.Nombre,
                                                     registroCaja.HoraFinal.ToString("dd/MM/yyyy - hh:mm:ss"),
                                                     registroCaja.Usuario,
                                                     registroCaja.TipoCambio.ToString("###,###,###.0000"));
                }

                Utilidades.SendMails("info@CashFlow.com", "SUPERVISOR", 
                                        string.Format("CASHFOW - APERTURA DE CAJA - {0}", registroCaja.HoraInicio), 
                                        body, registroCaja.HoraInicio, Empresa, DataWorkspace.ApplicationData);
            }

            this.ShowMessageBox("Caja exitosamente abierta!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
            this.Close(promptUserToSave: false);
        }

        #endregion
    }
}
