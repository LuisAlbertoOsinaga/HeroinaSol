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
    public partial class ScreenLibroVentas
    {
        #region Propiedades

        IContentItemProxy comboEmpresas;
        IContentItemProxy comboTiposFacturas;

        #endregion

        #region Metodos Auxiliares

        void Bindings()
        {
            comboEmpresas.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.Empresas", System.Windows.Data.BindingMode.OneWay);
            comboEmpresas.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.Empresas.SelectedItem", System.Windows.Data.BindingMode.TwoWay);

            comboTiposFacturas.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.TiposFacturas", System.Windows.Data.BindingMode.OneWay);
            comboTiposFacturas.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.TiposFacturas.SelectedItem", System.Windows.Data.BindingMode.TwoWay);
        }

        private void CalculaTotales()
        {
            List<Factura> FacturasReporte = (from Factura f in DataWorkspace.ApplicationData.Facturas
                                             where f.Dosificacion.Autoimpresor.Sucursal.Empresa == Empresas.SelectedItem &&
                                                     (string.Compare(f.FacturaTipo.Codigo, FacturaTipoFacturaInicial) >= 0 &&
                                                         string.Compare(f.FacturaTipo.Codigo, FacturaTipoFacturaFinal) <= 0) &&
                                                     (string.Compare(f.Estado, FacturaEstadoInicial) >= 0 &&
                                                         string.Compare(f.Estado, FacturaEstadoFinal) <= 0) &&
                                                     (f.FechaEmision >= FacturaFechaInicial && f.FechaEmision <= FacturaFechaFinal)
                                             select f).ToList();

            CantidadFacturasVig = (from Factura f in FacturasReporte
                                   where f.Estado == "V"
                                   select f).Count();

            TotalImporteTotalVig = (from Factura f in FacturasReporte
                                    where f.Estado == "V"
                                    select f).Sum(f => f.Monto);

            TotalImporteICEVig = (from Factura f in FacturasReporte
                                  where f.Estado == "V"
                                  select f).Sum(f => f.ICE);

            TotalImporteExcentoVig = (from Factura f in FacturasReporte
                                      where f.Estado == "V"
                                      select f).Sum(f => f.Excento);

            TotalImporteNetoVig = (from Factura f in FacturasReporte
                                   where f.Estado == "V"
                                   select f).Sum(f => f.Neto);

            TotalDebitoFiscalVig = (from Factura f in FacturasReporte
                                    where f.Estado == "V"
                                    select f).Sum(f => f.DebitoFiscal);

            TotalITVig = TotalImporteTotalVig * 0.03M;

            CantidadFacturasAnul = (from Factura f in FacturasReporte
                                   where f.Estado == "A"
                                   select f).Count();

            TotalImporteTotalAnul = (from Factura f in FacturasReporte
                                    where f.Estado == "A"
                                    select f).Sum(f => f.Monto);

            TotalImporteICEAnul = (from Factura f in FacturasReporte
                                  where f.Estado == "A"
                                  select f).Sum(f => f.ICE);

            TotalImporteExcentoAnul = (from Factura f in FacturasReporte
                                      where f.Estado == "A"
                                      select f).Sum(f => f.Excento);

            TotalImporteNetoAnul = (from Factura f in FacturasReporte
                                   where f.Estado == "A"
                                   select f).Sum(f => f.Neto);

            TotalDebitoFiscalAnul = (from Factura f in FacturasReporte
                                    where f.Estado == "A"
                                    select f).Sum(f => f.DebitoFiscal);

            TotalITAnul = TotalImporteTotalAnul * 0.03M;
        }

        void Enablings()
        {
            comboEmpresas.IsEnabled = Empresas.Count > 1 && Application.Current.User.HasPermission(Permissions.CambiarEmpresa);
            comboEmpresas.IsVisible = Empresas.Count > 1;
        }

        void FindControls()
        {
            comboEmpresas = this.FindControl("ComboEmpresas");
            comboTiposFacturas = this.FindControl("ComboTiposFacturas");
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

        partial void ScreenLibroVentas_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();
            FindControls();
            Bindings();
            Enablings();

            TipoFacturaString = "TOD";
            Estado = "T";
            RangoFechas = "M";
            Fecha = Cfg != null ? Cfg.FechaHoy.GetValueOrDefault() : DateTime.Now;
        }

        partial void Empresas_SelectionChanged()
        {
            EmpresaNombre = Empresas.SelectedItem != null && Empresas.Count == 1 ? Empresas.SelectedItem.Nombre : SoftCliente.ClienteNombre;
            comboEmpresas.IsVisible = Empresas.Count > 1;
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
            TotalImporteTotalAnul = 0M;
            TotalImporteICEAnul = 0M;
            TotalImporteExcentoAnul = 0m;
            TotalImporteNetoAnul = 0m;
            TotalDebitoFiscalAnul = 0m;
            TotalITAnul = 0m;
            CantidadFacturasVig = 0;
            TotalImporteTotalVig = 0M;
            TotalImporteICEVig = 0M;
            TotalImporteExcentoVig = 0m;
            TotalImporteNetoVig = 0m;
            TotalDebitoFiscalVig = 0m;
            TotalITVig = 0m;
            if (Facturas.Count > 0)
                CalculaTotales();

            Mensaje = Facturas.Count == 0 ? "No hay facturas para desplegar!" : string.Empty;
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

        partial void VerFactura_Execute()
        {
            if (Facturas.SelectedItem != null)
                Application.ShowScreenFacturaDef(Facturas.SelectedItem.Dosificacion.NroAutorizacion, 
                                                    Facturas.SelectedItem.Nro, 
                                                    HabilitarAnularDesanular: false,
                                                    HabilitarEdicion: false);
        }

        #endregion
    }
}
