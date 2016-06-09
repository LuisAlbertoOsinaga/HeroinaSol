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
    public partial class ScreenFacturaAnulacion
    {
        #region Propiedades

        IContentItemProxy comboAutoimpresores;
        IContentItemProxy comboEmpresas;
        IContentItemProxy comboSucursales;
        IContentItemProxy comboTipoFacturas;
        
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

            comboTipoFacturas.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.TiposFacturas", System.Windows.Data.BindingMode.OneWay);
            comboTipoFacturas.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.TiposFacturas.SelectedItem", System.Windows.Data.BindingMode.TwoWay);
        }
    
        void Enablings()
        {
            comboEmpresas.IsEnabled = false;
            comboSucursales.IsEnabled = false;
            comboAutoimpresores.IsEnabled = false;
            comboTipoFacturas.IsEnabled = true;
        }
     
        void FindControls()
        {
            comboAutoimpresores = this.FindControl("ComboAutoimpresores");
            comboEmpresas = this.FindControl("ComboEmpresas");
            comboSucursales = this.FindControl("ComboSucursales");
            comboTipoFacturas = this.FindControl("ComboTiposFacturas");
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
            Empresas.SelectedItem = (from e in Empresas where e.Nombre == Cfg.Empresa select e).SingleOrDefault();
            Sucursales.SelectedItem = (from s in Sucursales where s.Nombre == Cfg.Sucursal select s).SingleOrDefault();
            Autoimpresores.SelectedItem = (from a in Autoimpresores where a.Nombre == Cfg.Autoimpresor select a).SingleOrDefault();
        }

        private string LastNroAutorizacion(FacturaTipo facturaTipo)
        {
            Dosificacion dos = (from Dosificacion d in DataWorkspace.ApplicationData.Dosificacions
                                    where d.FacturaTipo.Id == facturaTipo.Id && d.Activa 
                                    orderby d.FechaLimiteEmision descending
                                    select d).FirstOrDefault();
            return dos != null ? dos.NroAutorizacion : string.Empty;
        }

        #endregion

        #region Metodos Generados

        partial void ScreenFacturaAnulacion_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();
            FindControls();
            Bindings();
            Inits();
            Enablings();
        }

        partial void AnularFactura_Execute()
        {
            //
            // Validaciones
            //
            
            RegistroCaja = (from r in DataWorkspace.ApplicationData.RegistroCajas
                            where r.Id == Cfg.RegistroCaja
                            select r).SingleOrDefault();
            if (RegistroCaja == null)
            {
                this.ShowMessageBox("No se pudo encontrar el registro de Caja!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }
            if(!RegistroCaja.CajaAbierta)
            {
                this.ShowMessageBox("Caja cerrada!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }
            if(!(Application.Current.User.Name == RegistroCaja.Usuario))
            {
                this.ShowMessageBox(string.Format("Caja abierta con otro usuario: '{0}'!", RegistroCaja.Usuario), 
                                        SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }

            if (string.IsNullOrWhiteSpace(NroAutorizacion))
            {
                this.ShowMessageBox("Nro. de Autorización en blanco!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }
            if (string.IsNullOrWhiteSpace(FacturaNro))
            {
                this.ShowMessageBox("Factura Nro. en blanco!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }

            Factura FacturaXAnular = (from f in DataWorkspace.ApplicationData.Facturas
                                      where f.Dosificacion.NroAutorizacion == NroAutorizacion && f.Nro == FacturaNro
                                      select f).SingleOrDefault();
            if (FacturaXAnular == null)
            {
                this.ShowMessageBox("No se encontró una factura con esos datos!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }
            else if (FacturaXAnular.Dosificacion.Autoimpresor.Sucursal.Empresa != Empresas.SelectedItem)
            {
                this.ShowMessageBox("Factura no se puede anular porque pertenece a otra Empresa!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }
            else if (FacturaXAnular.Dosificacion.Autoimpresor.Sucursal != Sucursales.SelectedItem)
            {
                this.ShowMessageBox("Factura no se puede anular porque pertenece a otra Sucursal!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }
            else if (FacturaXAnular.Dosificacion.Autoimpresor != Autoimpresores.SelectedItem)
            {
                this.ShowMessageBox("Factura no se puede anular porque pertenece a otro puesto!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }
            else if (FacturaXAnular.CreadoPor != RegistroCaja.Usuario
                        || FacturaXAnular.FechaEmision < RegistroCaja.HoraInicio
                        || (RegistroCaja.HoraFinal > RegistroCaja.HoraInicio && FacturaXAnular.FechaEmision > RegistroCaja.HoraFinal))
            {
                this.ShowMessageBox("Factura no se puede anular porque pertenece a otro turno!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }

            Application.ShowScreenFacturaDef(FacturaXAnular.Dosificacion.NroAutorizacion, FacturaXAnular.Nro, 
                HabilitarAnularDesanular: true, HabilitarEdicion: false);
        }

        partial void Empresas_SelectionChanged()
        {
            EmpresaNombre = Empresas.SelectedItem != null && Empresas.Count == 1 ? Empresas.SelectedItem.Nombre : SoftCliente.ClienteNombre;
        }

        partial void TiposFacturas_SelectionChanged()
        {
            if (TiposFacturas.SelectedItem != null)
                NroAutorizacion = LastNroAutorizacion(TiposFacturas.SelectedItem);
        }

        #endregion
    }
}
