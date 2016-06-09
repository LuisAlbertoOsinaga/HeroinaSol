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
    public partial class ScreenAnticipos
    {
        #region Propiedades

        IContentItemProxy groupAnticipo;
        IContentItemProxy groupCaja;
        IContentItemProxy groupEmpresas;

        #endregion

        #region Métodos Auxiliares

        void Bindings()
        {
            groupCaja.ControlAvailable += GroupCaja_ControlAvailable;
        }

        void Enablings()
        {
        }

        void FindControls()
        {
            groupAnticipo = this.FindControl("GroupAnticipo");
            groupCaja = this.FindControl("GroupCaja");
            groupEmpresas = this.FindControl("GroupEmpresas");
        }

        private void GroupCaja_ControlAvailable(object sender, ControlAvailableEventArgs e)
        {
            Control box = (Control)e.Control;
            box.Background = new SolidColorBrush(Color.FromArgb(255, Globales.CajaColorRed, Globales.CajaColorGreen, Globales.CajaColorBlue));
        }

        bool HayCajaAbierta()
        {
            if (Cfg.RegistroCaja == 0)
                return false;

            RegistroCaja = DataWorkspace.ApplicationData.RegistroCajas_SingleOrDefault(Cfg.RegistroCaja);
            return (RegistroCaja != null && RegistroCaja.CajaAbierta);
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
            if (Autoimpresor == null)
            {
                this.ShowMessageBox("no hay registro de Autoimpresor!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                Close(promptUserToSave: false);
                return false;
            }
            if (Autoimpresor.Id != RegistroCaja.Autoimpresor.Id)
            {
                this.ShowMessageBox("Autoimpresor no correponde a este registro de caja!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                Close(promptUserToSave: false);
                return false;
            }

            Sucursal = Autoimpresor.Sucursal;
            Empresa = Autoimpresor.Sucursal.Empresa;
            Turno = RegistroCaja.Turno;
            EmpresaNombre = SoftCliente.NroEmpresas > 1 || Empresa == null ? SoftCliente.ClienteNombre : Empresa.Nombre;
            groupEmpresas.IsVisible = SoftCliente.NroEmpresas > 1;
            return true;
        }

        #endregion

        #region Metodos Generados

        partial void ScreenAnticipos_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();

            if (!HayCajaAbierta())
            {
                this.ShowMessageBox("Para registrar un cobro de facturar debe primero abrir su caja!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                this.Close(promptUserToSave: false);
                return;
            }

            if (!(Application.Current.User.Name == RegistroCaja.Usuario))
            {
                this.ShowMessageBox(string.Format("Caja abierta con otro usuario: '{0}'!", RegistroCaja.Usuario),
                                        SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
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

        partial void Abandonar_Execute()
        {
            this.Close(promptUserToSave: false);
        }

        partial void BuscarCliente_Execute()
        {
            if (!string.IsNullOrWhiteSpace(ClienteNIT))
            {
                ClienteNombre = string.Empty;
                ClienteOperacion.Cliente = (from Cliente cli in DataWorkspace.ApplicationData.Clientes
                                            where cli.NIT == ClienteNIT
                                            select cli).FirstOrDefault();
                if (ClienteOperacion.Cliente != null)
                {
                    ClienteNIT = ClienteOperacion.Cliente.NIT;
                    ClienteNombre = ClienteOperacion.Cliente.RazonSocial;
                }
                Mensaje = ClienteOperacion.Cliente == null ? "Cliente no encontrado!" : string.Empty;
            }
            else
            {
                ClienteOperacion.Cliente = (from Cliente cli in DataWorkspace.ApplicationData.Clientes
                                            where cli.RazonSocial.ToUpper().Contains(ClienteNombre.ToUpper().Trim())
                                            select cli).FirstOrDefault();
                if (ClienteOperacion.Cliente != null)
                {
                    ClienteNIT = ClienteOperacion.Cliente.NIT;
                    ClienteNombre = ClienteOperacion.Cliente.RazonSocial;
                }
                Mensaje = ClienteOperacion.Cliente == null ? "Cliente no encontrado!" : string.Empty;
            }
        }

        partial void CalcularTotal_Execute()
        {
            ClienteOperacion.Monto = ClienteOperacion.MontoBS + Math.Round(ClienteOperacion.MontoUS * ClienteOperacion.TipoCambio, 2);
        }

        partial void Cancelar_Execute()
        {
            if (ClienteOperacion != null)
                ClienteOperacion.Delete();
            groupAnticipo.IsVisible = false;
        }

        partial void Confirmar_Execute()
        {
            CalcularTotal_Execute();
            ClienteOperacion.Estado = "V";
            ClienteOperacion.FacturaNro = null;
            ClienteOperacion.TipoOperacion = "OA";

            this.Save();
            this.Refresh();
            groupAnticipo.IsVisible = false;
            ClienteOperacion = null;
        }

        partial void DetalleFacturacion_Execute()
        {
            Application.ShowScreenDetalleFacturas(RegistroCaja.Id);
        }

        partial void NuevoAnticipo_Execute()
        {
            ClienteOperacion = DataWorkspace.ApplicationData.ClienteOperacions.AddNew();
            ClienteOperacion.Fecha = DateTime.Now;
            ClienteOperacion.TipoCambio = Cfg.TipoCambio;
            ClienteOperacion.MedioPagoBS = "EF";
            ClienteOperacion.MedioPagoUS = "EF";

            groupAnticipo = this.FindControl("GroupNuevoAnticipo");
            groupAnticipo.IsVisible = true;
        }

        partial void ResumenCaja_Execute()
        {
            Application.ShowScreenResumenCaja(RegistroCaja.Id);
        }

        #endregion
    }
}
