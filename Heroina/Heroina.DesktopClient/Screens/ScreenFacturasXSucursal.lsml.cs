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
    public partial class ScreenFacturasXSucursal
    {
        #region Propiedades

        IContentItemProxy comboEmpresas;
        IContentItemProxy comboSucursales;
        IContentItemProxy comboTipoFacturas;

        #endregion

        #region Metodos Auxiliares

        void Bindings()
        {
            comboEmpresas.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.Empresas", System.Windows.Data.BindingMode.OneWay);
            comboEmpresas.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.Empresas.SelectedItem", System.Windows.Data.BindingMode.TwoWay);

            comboSucursales.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.Sucursales", System.Windows.Data.BindingMode.OneWay);
            comboSucursales.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.Sucursales.SelectedItem", System.Windows.Data.BindingMode.TwoWay);

            comboTipoFacturas.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.TiposFacturas", System.Windows.Data.BindingMode.OneWay);
            comboTipoFacturas.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.TiposFacturas.SelectedItem", System.Windows.Data.BindingMode.TwoWay);
        }

        private void CalculaTotales()
        {
            List<Factura> FacturasReporte = (from Factura f in DataWorkspace.ApplicationData.Facturas
                                             where f.Dosificacion.Autoimpresor.Sucursal == Sucursales.SelectedItem &&
                                                     (string.Compare(f.FacturaTipo.Codigo, FacturaTipoFacturaInicial) >= 0 &&
                                                         string.Compare(f.FacturaTipo.Codigo, FacturaTipoFacturaFinal) <= 0) &&
                                                     (string.Compare(f.Estado, FacturaEstadoInicial) >= 0 &&
                                                         string.Compare(f.Estado, FacturaEstadoFinal) <= 0) &&
                                                     (f.FechaEmision >= FacturaFechaInicial && f.FechaEmision <= FacturaFechaFinal)
                                             select f).ToList();

            CantidadFacturasVig = (from Factura f in FacturasReporte
                                   where f.Estado == "V"
                                   select f).Count();

            TotalSubTotalVig = (from Factura f in FacturasReporte
                                where f.Estado == "V"
                                select f).Sum(f => f.Subtotal);

            TotalDescuentosVig = (from Factura f in FacturasReporte
                                  where f.Estado == "V"
                                  select f).Sum(f => f.Descuento);

            TotalImporteTotalVig = (from Factura f in FacturasReporte
                                    where f.Estado == "V"
                                    select f).Sum(f => f.Monto);

            CantidadFacturasAnul = (from Factura f in FacturasReporte
                                    where f.Estado == "A"
                                    select f).Count();

            TotalSubTotalAnul = (from Factura f in FacturasReporte
                                 where f.Estado == "A"
                                 select f).Sum(f => f.Subtotal);

            TotalDescuentosAnul = (from Factura f in FacturasReporte
                                   where f.Estado == "A"
                                   select f).Sum(f => f.Descuento);

            TotalImporteTotalAnul = (from Factura f in FacturasReporte
                                     where f.Estado == "A"
                                     select f).Sum(f => f.Monto);
        }

        void Enablings()
        {
            comboEmpresas.IsEnabled = Empresas.Count > 1 && Application.Current.User.HasPermission(Permissions.CambiarEmpresa);
            comboEmpresas.IsVisible = Empresas.Count > 1;

            comboSucursales.IsEnabled = Sucursales.Count > 1 && Application.Current.User.HasPermission(Permissions.CambiarSucursal);
        }

        void FindControls()
        {
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

        partial void ScreenFacturasXSucursal_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();
            FindControls();
            Bindings();
            Enablings();

            TipoFacturaString = "TOD";
            Estado = "V";
            RangoFechas = "D";
            Fecha = Cfg != null ? Cfg.FechaHoy.GetValueOrDefault() : DateTime.Now;
        }

        partial void RangoFechas_Changed()
        {
            IContentItemProxy boxFecha = this.FindControl("Fecha");
            IContentItemProxy boxFechaInicial = this.FindControl("FechaInicial");
            IContentItemProxy boxFechaFinal = this.FindControl("FechaFinal");

            if (RangoFechas == "D" || RangoFechas == "S" || RangoFechas == "M" || RangoFechas == "A")
            {
                boxFecha.IsVisible = true;
                boxFechaInicial.IsVisible = false;
                boxFechaFinal.IsVisible = false;
            }
            else if (RangoFechas == "E")
            {
                boxFecha.IsVisible = false;
                boxFechaInicial.IsVisible = true;
                boxFechaFinal.IsVisible = true;
            }
            else    // RangoFechas == "T"
            {
                boxFecha.IsVisible = false;
                boxFechaInicial.IsVisible = false;
                boxFechaFinal.IsVisible = false;
            }
        }

        partial void Generar_Execute()
        {
            TipoFacturaString = TiposFacturas.SelectedItem.Codigo;

            if (TipoFacturaString != "TOD")
            {
                FacturaTipoFacturaInicial = TipoFacturaString;
                FacturaTipoFacturaFinal = TipoFacturaString;
            }
            else // TipoFactura == "TOD"
            {
                FacturaTipoFacturaInicial = "AAA";
                FacturaTipoFacturaFinal = "ZZZ";
            }

            if (Estado == "V" || Estado == "A")
            {
                FacturaEstadoInicial = Estado;
                FacturaEstadoFinal = Estado;
            }
            else // Estado == "T"
            {
                FacturaEstadoInicial = "A";
                FacturaEstadoFinal = "Z";
            }

            if (RangoFechas == "D")
            {
                FacturaFechaInicial = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 0, 0, 0);
                FacturaFechaFinal = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 23, 59, 59);
            }
            else if (RangoFechas == "S")
            {
                FacturaFechaInicial = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 0, 0, 0);
                FacturaFechaFinal = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 23, 59, 59);
                while (FacturaFechaInicial.DayOfWeek != DayOfWeek.Monday)
                    FacturaFechaInicial = FacturaFechaInicial.AddDays(-1);
                while (FacturaFechaFinal.DayOfWeek != DayOfWeek.Sunday)
                    FacturaFechaFinal = FacturaFechaFinal.AddDays(1);
            }
            else if (RangoFechas == "M")
            {
                FacturaFechaInicial = new DateTime(Fecha.Year, Fecha.Month, 1, 0, 0, 0);
                FacturaFechaFinal = FacturaFechaInicial.AddMonths(1).AddDays(-1);
                FacturaFechaFinal = new DateTime(FacturaFechaFinal.Year, FacturaFechaFinal.Month, FacturaFechaFinal.Day, 23, 59, 59);
            }
            else if (RangoFechas == "A")
            {
                FacturaFechaInicial = new DateTime(Fecha.Year, 1, 1, 0, 0, 0);
                FacturaFechaFinal = new DateTime(Fecha.Year, 12, 31, 23, 59, 59);
            }
            else if (RangoFechas == "T")
            {
                FacturaFechaInicial = new DateTime(2000, 1, 1, 0, 0, 0);
                FacturaFechaFinal = new DateTime(3000, 12, 31, 23, 59, 59);
            }
            else    // RangoFechas == "E"
            {
                FacturaFechaInicial = FechaInicial;
                FacturaFechaFinal = FechaFinal;
            }

            CantidadFacturasAnul = 0;
            TotalSubTotalAnul = 0M;
            TotalDescuentosAnul = 0M;
            TotalImporteTotalAnul = 0m;
            CantidadFacturasVig = 0;
            TotalSubTotalVig = 0M;
            TotalDescuentosVig = 0M;
            TotalImporteTotalVig = 0m;
            if (Facturas.Count != 0)
                CalculaTotales();

            Mensaje = Facturas.Count == 0 ? "No hay facturas para desplegar!" : string.Empty;
        }

        partial void Empresas_SelectionChanged()
        {
            EmpresaNombre = Empresas.SelectedItem != null && Empresas.Count == 1 ? Empresas.SelectedItem.Nombre : SoftCliente.ClienteNombre;
            comboEmpresas.IsVisible = Empresas.Count > 1;
        }

        partial void Facturas_SelectionChanged()
        {
            if (Facturas.SelectedItem != null)
            {
                IContentItemProxy gridDetalle_Hotelera = this.FindControl("FacturaDetalles_Hotelera");
                IContentItemProxy gridDetalle_Servicio = this.FindControl("FacturaDetalles_Servicio");
                IContentItemProxy gridDetalle_Comercial = this.FindControl("FacturaDetalles_Comercial");
                IContentItemProxy gridDetalle_Alquiler = this.FindControl("FacturaDetalles_Alquiler");
                IContentItemProxy groupDetalle_Hotelera = this.FindControl("GroupDetalleHotelera");
                IContentItemProxy groupDetalle_Servicio = this.FindControl("GroupDetalleServicio");
                IContentItemProxy groupDetalle_Comercial = this.FindControl("GroupDetalleComercial");
                IContentItemProxy groupDetalle_Alquiler = this.FindControl("GroupDetalleAlquiler");
                groupDetalle_Hotelera.IsVisible = false;
                groupDetalle_Servicio.IsVisible = false;
                groupDetalle_Comercial.IsVisible = false;
                groupDetalle_Alquiler.IsVisible = false;

                if (Facturas.SelectedItem.FacturaTipo.Codigo == "HOT" || Facturas.SelectedItem.FacturaTipo.Codigo == "TUR")
                {
                    gridDetalle_Hotelera.DisplayName = string.Format("Detalle Factura Nro '{0}'", Facturas.SelectedItem.Nro);
                    groupDetalle_Hotelera.IsVisible = true;
                }
                else if (Facturas.SelectedItem.FacturaTipo.Codigo == "SER")
                {
                    gridDetalle_Servicio.DisplayName = string.Format("Detalle Factura Nro '{0}'", Facturas.SelectedItem.Nro);
                    groupDetalle_Servicio.IsVisible = true;
                }
                else if (Facturas.SelectedItem.FacturaTipo.Codigo == "COM")
                {
                    gridDetalle_Comercial.DisplayName = string.Format("Detalle Factura Nro '{0}'", Facturas.SelectedItem.Nro);
                    groupDetalle_Comercial.IsVisible = true;
                }
                else if (Facturas.SelectedItem.FacturaTipo.Codigo == "ALQ")
                {
                    gridDetalle_Alquiler.DisplayName = string.Format("Detalle Factura Nro '{0}'", Facturas.SelectedItem.Nro);
                    groupDetalle_Alquiler.IsVisible = true;
                }
            }
        }

        partial void VerFactura_Execute()
        {
            if (Facturas.SelectedItem != null)
                Application.ShowScreenFacturaDef(Facturas.SelectedItem.Dosificacion.NroAutorizacion, 
                                                    Facturas.SelectedItem.Nro, 
                                                    HabilitarAnularDesanular: false,
                                                    HabilitarEdicion: true);
        }

        #endregion
    }
}
