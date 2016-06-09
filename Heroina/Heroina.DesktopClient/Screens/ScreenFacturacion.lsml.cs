using System;
using System.Linq;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using Microsoft.LightSwitch;
using Microsoft.LightSwitch.Framework.Client;
using Microsoft.LightSwitch.Presentation;
using Microsoft.LightSwitch.Presentation.Extensions;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Runtime.InteropServices.Automation;
using System.Collections.Specialized;

namespace LightSwitchApplication
{
    public partial class ScreenFacturacion
    {
        #region Propiedades

        IContentItemProxy areaFactura;

        IContentItemProxy btnAdicionarAnticipos;
        IContentItemProxy btnBuscarClienteXCobrar;
        IContentItemProxy btnCalcularPagoBS;
        IContentItemProxy btnCalcularPagoUS;
        IContentItemProxy btnContado;
        IContentItemProxy btnDetalleFactura;
        IContentItemProxy btnHospedaje;
        IContentItemProxy btnNuevaFactura;
        IContentItemProxy btnOtroClientePorCobrar;
        IContentItemProxy btnPorCobrar;
        IContentItemProxy btnPrintAlquiler;
        IContentItemProxy btnPrintComercial;
        IContentItemProxy btnPrintHotelera;
        IContentItemProxy btnPrintServicios;
        IContentItemProxy btnRefresh;
        IContentItemProxy btnSave;
        IContentItemProxy btnTotalAlquiler;
        IContentItemProxy btnTotalComercial;
        IContentItemProxy btnTotalHotelera;
        IContentItemProxy btnTotalServicios;
        IContentItemProxy ctrlDescuentos;

        IContentItemProxy comboFacturaTipos;
        IContentItemProxy dtpFechaEmision;

        IContentItemProxy gridAnticipos;
        IContentItemProxy gridDetalleComercial;
        IContentItemProxy gridDetalleHotelera;
        IContentItemProxy gridDetalleServicio;
        IContentItemProxy gridDetalleAlquiler;

        IContentItemProxy groupCaja;
        IContentItemProxy groupEmpresas;
        IContentItemProxy groupFacturaHeader;
        IContentItemProxy grupoFooter;

        DataGrid dgAnticipos;

        string[] Mes = {"Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

        dynamic WordApp = null;
        dynamic WordDoc = null;

        #endregion

        #region Métodos Auxiliares

        bool BuscaClienteFactura()
        {
            if (Factura.ClienteNIT == "0")
                Factura.ClienteNombre = "SIN NOMBRE";

            if (string.IsNullOrWhiteSpace(Factura.ClienteNIT) && string.IsNullOrWhiteSpace(Factura.ClienteNombre))
            {
                this.ShowMessageBox("Cliente en blanco. Ingrese NIT o nombre del Cliente!");
                return false;
            }

            Cliente cli = null;
            if (Empresa != null)
            {
                if (!string.IsNullOrWhiteSpace(Factura.ClienteNIT))
                {
                    cli = (from Cliente c in DataWorkspace.ApplicationData.ClientesXEmpresa(Empresa.Id)
                           where c.NIT == Factura.ClienteNIT
                           select c).FirstOrDefault();
                    if (cli != null)
                        Factura.ClienteNombre = cli.RazonSocial;
                }
                else
                {
                    cli = (from Cliente c in DataWorkspace.ApplicationData.ClientesXEmpresa(Empresa.Id)
                           where c.RazonSocial == Factura.ClienteNombre
                           select c).FirstOrDefault();
                    if (cli != null)
                        Factura.ClienteNIT = cli.NIT;
                }
            }

            return !string.IsNullOrWhiteSpace(Factura.ClienteNIT) && !string.IsNullOrWhiteSpace(Factura.ClienteNombre);
        }

        void Bindings()
        {
            comboFacturaTipos.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.FacturaTipos", System.Windows.Data.BindingMode.OneWay);
            comboFacturaTipos.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.FacturaTipos.SelectedItem", System.Windows.Data.BindingMode.TwoWay);
            groupCaja.ControlAvailable += GroupCaja_ControlAvailable;
        }

        void CloseAreaFactura()
        {
            // Delete todas las entidadas relacionadas a la factura
            if (FacturaHospedaje != null)
                FacturaHospedaje.Delete();
            while (FacturaDetalles1.Count > 0)
                FacturaDetalles1.First().Delete();
            if (Factura != null)
                Factura.Delete();
            Factura = null;

            areaFactura.IsVisible = false;
            comboFacturaTipos.IsEnabled = true;
            ctrlDescuentos.IsVisible = false;

#if DEBUG
#else
            // Seguridad
            //comboEmpresas.IsEnabled = Application.Current.User.HasPermission("CambiarEmpresa");
            //comboSucursales.IsEnabled = Application.Current.User.HasPermission("CambiarSucursal");
            //comboAutoimpresores.IsEnabled = Application.Current.User.HasPermission("CambiarAutoimpresor");
            comboFacturaTipos.IsEnabled = true;
#endif

            btnNuevaFactura.IsEnabled = Cfg != null;
            btnHospedaje.IsEnabled = false;
            btnPrintComercial.IsEnabled = false;
            btnPrintHotelera.IsEnabled = false;
            btnPrintServicios.IsEnabled = false;
            btnTotalComercial.IsEnabled = false;
            btnTotalHotelera.IsEnabled = false;
            btnTotalServicios.IsEnabled = false;

            CloseDetalleFactura();
        }

        void CloseDetalleFactura()
        {
            btnDetalleFactura.IsEnabled = true;
            btnContado.IsEnabled = false;
            btnCalcularPagoBS.IsEnabled = false;
            btnCalcularPagoUS.IsEnabled = false;
            btnPorCobrar.IsEnabled = false;
            btnOtroClientePorCobrar.IsEnabled = false;
            btnBuscarClienteXCobrar.IsEnabled = false;
            btnAdicionarAnticipos.IsEnabled = false;
            btnRefresh.IsEnabled = false;
            btnSave.IsEnabled = false;
            btnPrintAlquiler.IsEnabled = false;
            btnPrintComercial.IsEnabled = false;
            btnPrintHotelera.IsEnabled = false;
            btnPrintServicios.IsEnabled = false;

            gridDetalleComercial.IsVisible = false;
            gridDetalleHotelera.IsVisible = false;
            gridDetalleServicio.IsVisible = false;

            grupoFooter.IsVisible = false;
        }

        void Enablings()
        {
            dtpFechaEmision.IsEnabled = false;
        }

        void FindControls()
        {
            areaFactura = this.FindControl("AreaFactura");

            btnAdicionarAnticipos = this.FindControl("AdicionarAnticipos");
            btnBuscarClienteXCobrar = this.FindControl("BuscarClienteXCobrar");
            btnCalcularPagoBS = this.FindControl("CalcularPagoBS");
            btnCalcularPagoUS = this.FindControl("CalcularPagoUS");
            btnContado = this.FindControl("Contado");
            btnDetalleFactura = this.FindControl("DetalleFactura");
            btnHospedaje = this.FindControl("btnHospedaje");
            btnNuevaFactura = this.FindControl("AbrirNuevaFactura");
            btnOtroClientePorCobrar = this.FindControl("OtroClientePorCobrar");
            btnPorCobrar = this.FindControl("PorCobrar");
            btnPrintAlquiler = this.FindControl("btnPrintAlquiler");
            btnPrintComercial = this.FindControl("btnPrintComercial");
            btnPrintHotelera = this.FindControl("btnPrintHotelera");
            btnPrintServicios = this.FindControl("btnPrintServicios");
            btnRefresh = this.FindControl("Refresh");
            btnSave = this.FindControl("Save");
            btnTotalAlquiler = this.FindControl("btnTotalAlquiler");
            btnTotalComercial = this.FindControl("btnTotalComercial");
            btnTotalHotelera = this.FindControl("btnTotalHotelera");
            btnTotalServicios = this.FindControl("btnTotalServicios");

            ctrlDescuentos = this.FindControl("Factura_Descuento");
            dtpFechaEmision = this.FindControl("FechaEmision");
            comboFacturaTipos = this.FindControl("ComboFacturaTipos");

            gridAnticipos = this.FindControl("GridAnticipos");
            gridDetalleComercial = this.FindControl("GroupFacturaComercial");
            gridDetalleHotelera = this.FindControl("GroupFacturaHotelera");
            gridDetalleServicio = this.FindControl("GroupFacturaServicio");
            gridDetalleAlquiler = this.FindControl("GroupFacturaAlquiler");

            groupCaja = this.FindControl("GroupCaja");
            groupEmpresas = this.FindControl("GroupEmpresas");
            groupFacturaHeader = this.FindControl("GroupFacturaHeader");
            grupoFooter = this.FindControl("GrupoFooter");
        }

        string GetPathTemplate()
        {
            // path al file template en Documents
            string fileTemplate = null;
            if(Factura.FacturaTipo.Codigo == "ALQ")
                fileTemplate = Cfg.FileTemplateAlquiler;
            else if (Factura.FacturaTipo.Codigo == "COM")
                fileTemplate = Cfg.FileTemplateComercial;
            else if (Factura.FacturaTipo.Codigo == "HOT")
                fileTemplate = Cfg.FileTemplateHotelera;
            else if (Factura.FacturaTipo.Codigo == "TUR")
                fileTemplate = Cfg.FileTemplateTuristica;
            else if (Factura.FacturaTipo.Codigo == "SER")
                fileTemplate = Cfg.FileTemplateServicios;
            if (string.IsNullOrWhiteSpace(fileTemplate))
                return "No hay nombre de archivo template configurado!";
            dynamic path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + fileTemplate;

            //Write the stream to the file
            var resourceInfo = System.Windows.Application.GetResourceStream(new Uri("Resources/" + fileTemplate, UriKind.Relative));
            System.IO.Stream stream = resourceInfo.Stream;
            using (FileStream fileStream = System.IO.File.Open(path, System.IO.FileMode.Create, System.IO.FileAccess.Write,
                                                                System.IO.FileShare.None))
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, int.Parse(stream.Length.ToString()));
                fileStream.Write(buffer, 0, buffer.Length);
            }

            return path;
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

        void NuevaFactura()
        {
            if (Factura == null)
            {
                Mensaje = string.Empty;
                groupFacturaHeader.DisplayName = "Factura ";

                if (FacturaTipos.SelectedItem != null)
                {
                    Dosificacion = (from d in DataWorkspace.ApplicationData.DosificacionesXAutoimpresor(Autoimpresor.Id)
                                    where d.FacturaTipo.Codigo == FacturaTipos.SelectedItem.Codigo &&
                                            FechaEmision >= d.FechaInicial && FechaEmision <= d.FechaLimiteEmision &&
                                            d.Activa
                                    select d).FirstOrDefault();
                }

                if (Dosificacion == null)
                {
                    Mensaje = "No hay una Dosificación válida para emitir este tipo de factura!";
                    return;
                }
                DateTime fechaUltFactura = new DateTime(Dosificacion.FechaUltimaFactura.Year, Dosificacion.FechaUltimaFactura.Month, Dosificacion.FechaUltimaFactura.Day);
                if (FechaEmision < fechaUltFactura)
                {
                    Mensaje = string.Format("Fecha de la factura no puede ser anterior a la de la última emitida: '{0}'!", fechaUltFactura.ToString("dd-mmm-yyyy"));
                    return;
                }

                OpenAreaFactura();
            }
            else
            {
                CloseAreaFactura();
            }
        }

        void NuevoDetalleFactura()
        {
            int tiposHos = 0;
            int tiposRes = 0;
            int tiposLav = 0;
            int tiposCon = 0;
            int tiposReg = 0;

            FacturaDetalle facDet = Factura.FacturaDetalles.AddNew();
            facDet.NroLinea = FacturaDetalles1.Count();
            FacturaDetalles1.SelectedItem = facDet;
            if (Factura.FacturaTipo.Codigo == "HOT")
            {
                tiposHos = (from d in Factura.FacturaDetalles
                            where d.TipoDetalle != null && d.TipoDetalle.Codigo == "HOS"
                            select d).Count();
                tiposRes = (from d in Factura.FacturaDetalles
                            where d.TipoDetalle != null && d.TipoDetalle.Codigo == "RES"
                            select d).Count();
                tiposLav = (from d in Factura.FacturaDetalles
                            where d.TipoDetalle != null && d.TipoDetalle.Codigo == "LAV"
                            select d).Count();
                tiposCon = (from d in Factura.FacturaDetalles
                            where d.TipoDetalle != null && d.TipoDetalle.Codigo == "CON"
                            select d).Count();
                tiposReg = (from d in Factura.FacturaDetalles
                            where d.TipoDetalle != null && (d.TipoDetalle.Codigo == "REG" || d.TipoDetalle.Codigo == "OTR")
                            select d).Count();

                if (tiposHos == 0)
                {
                    facDet.TipoDetalle = (from t in FacturaTipoDetalles
                                          where t.Codigo == "HOS"
                                          select t).FirstOrDefault();
                }
                else if (tiposRes == 0)
                {
                    facDet.TipoDetalle = (from t in FacturaTipoDetalles
                                          where t.Codigo == "RES"
                                          select t).FirstOrDefault();
                }
                else if (tiposLav == 0)
                {
                    facDet.TipoDetalle = (from t in FacturaTipoDetalles
                                          where t.Codigo == "LAV"
                                          select t).FirstOrDefault();
                }
                else if (tiposCon == 0)
                {
                    facDet.TipoDetalle = (from t in FacturaTipoDetalles
                                          where t.Codigo == "CON"
                                          select t).FirstOrDefault();
                }
                else
                {
                    facDet.TipoDetalle = (from t in FacturaTipoDetalles
                                          where t.Codigo == "OTR"
                                          select t).FirstOrDefault();
                }
            }
            else if (Factura.FacturaTipo.Codigo == "TUR")
            {
                facDet.TipoDetalle = (from t in FacturaTipoDetalles
                                      where t.Codigo == "HOS"
                                      select t).FirstOrDefault();
            }
            else
            {
                facDet.TipoDetalle = (from t in FacturaTipoDetalles
                                      where t.Codigo == "REG"
                                      select t).FirstOrDefault();
            }

            if (facDet.TipoDetalle.Codigo == "RES")
                facDet.Concepto = Cfg.ConceptoRestaurante;
            else if (facDet.TipoDetalle.Codigo == "LAV")
                facDet.Concepto = Cfg.ConceptoLavanderia;
            else if (facDet.TipoDetalle.Codigo == "CON")
                facDet.Concepto = Cfg.ConceptoConferencias;

            btnHospedaje.IsEnabled = true;
            btnTotalAlquiler.IsEnabled = true;
            btnTotalComercial.IsEnabled = true;
            btnTotalHotelera.IsEnabled = true;
            btnTotalServicios.IsEnabled = true;
        }

        void OpenAreaFactura()
        {
            Factura = DataWorkspace.ApplicationData.Facturas.AddNew();
            Factura.Dosificacion = Dosificacion;
            Factura.Estado = "V";
            Factura.Situacion = "E";
            Factura.FacturaTipo = FacturaTipos.SelectedItem;
            Factura.FechaEmision = FechaEmision;
            Factura.TipoCambio = Factura.FacturaTipo.Codigo == "TUR" ? Cfg.TipoCambioOficial : Cfg.TipoCambio;
            Factura.MedioPagoContadoBS = "EF";
            Factura.MedioPagoContadoUS = "EF";
            Factura.Nro = ServicioDosificacionex.GetFacturaSiguienteFormat(DataWorkspace, Dosificacion.Id);
            SinCreditoFiscal = Factura.FacturaTipo.SinCreditoFiscal ? "SIN CRÉDITO FISCAL" : string.Empty;
            groupFacturaHeader.DisplayName = " FACTURA " + Factura.Nro + " ";

            areaFactura.IsVisible = true;
            comboFacturaTipos.IsEnabled = true;
            ctrlDescuentos.IsVisible = Factura.FacturaTipo.Codigo != "SER" && Factura.FacturaTipo.Codigo != "ALQ";

            btnTotalAlquiler.IsEnabled = false;
            btnTotalComercial.IsEnabled = false;
            btnTotalHotelera.IsEnabled = false;
            btnTotalServicios.IsEnabled = false;
            btnPrintAlquiler.IsEnabled = false;
            btnPrintComercial.IsEnabled = false;
            btnPrintHotelera.IsEnabled = false;
            btnPrintServicios.IsEnabled = false;
        }

        void OpenDetalleFactura()
        {
            if (FacturaTipos.SelectedItem == null || FacturaTipos.SelectedItem.Codigo == "COM")
            {
                gridDetalleComercial.IsVisible = true;
                gridDetalleHotelera.IsVisible = false;
                gridDetalleServicio.IsVisible = false;
                gridDetalleAlquiler.IsVisible = false;
            }
            else if (FacturaTipos.SelectedItem == null || FacturaTipos.SelectedItem.Codigo == "HOT" || FacturaTipos.SelectedItem.Codigo == "TUR")
            {
                gridDetalleComercial.IsVisible = false;
                gridDetalleHotelera.IsVisible = true;
                gridDetalleServicio.IsVisible = false;
                gridDetalleAlquiler.IsVisible = false;
            }
            else if (FacturaTipos.SelectedItem == null || FacturaTipos.SelectedItem.Codigo == "ALQ")
            {
                gridDetalleComercial.IsVisible = false;
                gridDetalleHotelera.IsVisible = false;
                gridDetalleServicio.IsVisible = false;
                gridDetalleAlquiler.IsVisible = true;
            }
            else  // "SER"
            {
                gridDetalleComercial.IsVisible = false;
                gridDetalleHotelera.IsVisible = false;
                gridDetalleServicio.IsVisible = true;
                gridDetalleAlquiler.IsVisible = false;
            }

            btnDetalleFactura.IsEnabled = false;
            grupoFooter.IsVisible = true;
        }

        void PrintFactura(bool original, int nroCopias, bool headerIzq)
        {
            if (AutomationFactory.IsAvailable)
            {
                try
                {
                    // Imprimir a través de word
                    try
                    {
                        string path = GetPathTemplate();
                        try
                        {
                            WordApp = AutomationFactory.CreateObject("Word.Application");
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException("Falla al crear app word!", ex);
                        }
                        WordDoc = WordApp.Documents.Open(path);

                        //
                        // Llenado Datos
                        //

                        // Izquierda
                        if (headerIzq)
                        {
                            WordDoc.Bookmarks("Empresa").Range.InsertBefore(Empresa.RazonSocial);
                            WordDoc.Bookmarks("Sucursal").Range.InsertBefore(Sucursal.Nombre);
                            WordDoc.Bookmarks("Calle").Range.InsertAfter(Sucursal.Calle);
                            WordDoc.Bookmarks("Zona").Range.InsertAfter(Sucursal.Zona);
                            WordDoc.Bookmarks("Telefonos").Range.InsertAfter(Sucursal.Telefonos);
                            WordDoc.Bookmarks("Municipio").Range.InsertAfter(Sucursal.Ciudad + "-" + Empresa.Pais);
                        }

                        // Derecha
                        WordDoc.Bookmarks("EmpresaNIT").Range.InsertAfter(Empresa.NIT);
                        WordDoc.Bookmarks("FacturaNro").Range.InsertAfter(Factura.Nro);
                        WordDoc.Bookmarks("AutorizacionNro").Range.InsertAfter(Factura.Dosificacion.NroAutorizacion);
                        WordDoc.Bookmarks("ActividadEconomica").Range.InsertAfter(Dosificacion.ActividadEconomica.Nombre);
                        WordDoc.Bookmarks("Autoimpresor").Range.InsertAfter("SFC " + Autoimpresor.NroAutoImpresor.ToString());

                        // Cabecera
                        if (FacturaTipos.SelectedItem.Codigo == "TUR")
                            WordDoc.Bookmarks("Titulo").Range.InsertBefore("FACTURA TURÍSTICA");
                        else if (FacturaTipos.SelectedItem.Codigo == "ALQ")
                            WordDoc.Bookmarks("Titulo").Range.InsertBefore("RECIBO DE ALQUILER");
                        else
                            WordDoc.Bookmarks("Titulo").Range.InsertBefore("FACTURA");
                        if (Factura.FacturaTipo.SinCreditoFiscal)
                            WordDoc.Bookmarks("Subtitulo").Range.InsertBefore("SIN DERECHO A CRÉDITO FISCAL");
                        WordDoc.Bookmarks("Fecha").Range.InsertAfter(Factura.FechaEmision.Day.ToString() + " de " +
                                                                        Mes[Factura.FechaEmision.Month - 1] + " de " +
                                                                        Factura.FechaEmision.ToString("yyyy"));
                        WordDoc.Bookmarks("ClienteNombre").Range.InsertAfter(!(Factura.FacturaTipo.Codigo == "TUR" && Factura.ClienteNombre == "SIN NOMBRE") ?
                                                                                Factura.ClienteNombre : "0");
                        WordDoc.Bookmarks("ClienteNIT").Range.InsertAfter(Factura.ClienteNIT);

                        //
                        // Cuerpo
                        //

                        int i;
                        for (i = 1; i <= Factura.FacturaDetalles.Count(); i++)
                        {
                            if(Factura.FacturaTipo.Codigo == "COM" && Cfg.FacturaComercialRollo)
                                WordDoc.Tables[2].Rows.Add(WordDoc.Tables[2].Rows[i]);
                            else
                                WordDoc.Tables[6].Rows.Add(WordDoc.Tables[6].Rows[i]);
                        }

                        if (FacturaTipos.SelectedItem.Codigo == "COM")
                        {
                            if (!Cfg.FacturaComercialRollo)   // REGULAR
                            {
                                WordDoc.Tables[6].Cell(1, 1).Range.Text = "CANTIDAD";
                                WordDoc.Tables[6].Cell(1, 2).Range.Text = "DESCRIPCIÓN";
                                WordDoc.Tables[6].Cell(1, 3).Range.Text = "PRECIO UNITARIO";
                                WordDoc.Tables[6].Cell(1, 4).Range.Text = "IMPORTE (Bs)";
                            }
                            else // ROLLO
                            {
                                WordDoc.Tables[2].Cell(1, 1).Range.Text = "CAN.";
                                WordDoc.Tables[2].Cell(1, 2).Range.Text = "DESCRIPCIÓN";
                                WordDoc.Tables[2].Cell(1, 3).Range.Text = "P.UNIT";
                                WordDoc.Tables[2].Cell(1, 4).Range.Text = "IMPORTE";
                            }
                        }
                        else if (FacturaTipos.SelectedItem.Codigo == "HOT" || FacturaTipos.SelectedItem.Codigo == "TUR")
                        {
                            WordDoc.Tables[6].Cell(1, 1).Range.Text = "CONCEPTO";
                            WordDoc.Tables[6].Cell(1, 2).Range.Text = "DESCRIPCIÓN";
                            WordDoc.Tables[6].Cell(1, 3).Range.Text = "IMPORTE (Bs)";
                        }
                        else if (FacturaTipos.SelectedItem.Codigo == "ALQ")
                        {
                            WordDoc.Tables[6].Cell(1, 1).Range.Text = "PERIODO";
                            WordDoc.Tables[6].Cell(1, 2).Range.Text = "DESCRIPCIÓN DEL INMUEBLE";
                            WordDoc.Tables[6].Cell(1, 3).Range.Text = "SUBTOTAL (Bs)";
                        }
                        else // SER 
                        {
                            WordDoc.Tables[6].Cell(1, 1).Range.Text = "DETALLE";
                            WordDoc.Tables[6].Cell(1, 2).Range.Text = "SUBTOTAL (Bs)";
                        }

                        i = 2;
                        foreach (var factDet in Factura.FacturaDetalles)
                        {
                            if (FacturaTipos.SelectedItem.Codigo == "COM")
                            {
                                if (!Cfg.FacturaComercialRollo)     // REGULAR
                                {
                                    WordDoc.Tables[6].Cell(i, 1).Range.Text = factDet.Cantidad.ToString("####");
                                    WordDoc.Tables[6].Cell(i, 2).Range.Text = factDet.Concepto;
                                    WordDoc.Tables[6].Cell(i, 3).Range.Text = factDet.PrecioUnitario.ToString("###,###,###.00");
                                    WordDoc.Tables[6].Cell(i, 4).Range.Text = factDet.Importe.ToString("###,###,###.00");
                                }
                                else        // ROLLO
                                {
                                    WordDoc.Tables[2].Cell(i, 1).Range.Text = factDet.Cantidad.ToString("####");
                                    WordDoc.Tables[2].Cell(i, 2).Range.Text = factDet.Concepto;
                                    WordDoc.Tables[2].Cell(i, 3).Range.Text = factDet.PrecioUnitario.ToString("###,###,###.00");
                                    WordDoc.Tables[2].Cell(i, 4).Range.Text = factDet.Importe.ToString("###,###,###.00");
                                }
                            }
                            else if (FacturaTipos.SelectedItem.Codigo == "HOT" || FacturaTipos.SelectedItem.Codigo == "TUR")
                            {
                                WordDoc.Tables[6].Cell(i, 1).Range.Text = factDet.TipoDetalle.Descripcion;
                                WordDoc.Tables[6].Cell(i, 2).Range.Text = factDet.Concepto;
                                WordDoc.Tables[6].Cell(i, 3).Range.Text = factDet.Importe.ToString("###,###,###.00");
                            }
                            else if (FacturaTipos.SelectedItem.Codigo == "ALQ")
                            {
                                WordDoc.Tables[6].Cell(i, 1).Range.Text = factDet.Periodo;
                                WordDoc.Tables[6].Cell(i, 2).Range.Text = factDet.Concepto;
                                WordDoc.Tables[6].Cell(i, 3).Range.Text = factDet.Importe.ToString("###,###,###.00");
                            }
                            else // SER
                            {
                                WordDoc.Tables[6].Cell(i, 1).Range.Text = factDet.Concepto;
                                WordDoc.Tables[6].Cell(i, 2).Range.Text = factDet.Importe.ToString("###,###,###.00");
                            }
                            i++;
                        }

                        if (FacturaTipos.SelectedItem.Codigo != "SER" && FacturaTipos.SelectedItem.Codigo != "ALQ")
                        {
                            WordDoc.Bookmarks("Subtotal").Range.InsertAfter(Factura.Subtotal.ToString("###,###,##0.00"));
                            WordDoc.Bookmarks("Descuento").Range.InsertAfter(Factura.Descuento.ToString("###,###,##0.00"));
                        }
                        WordDoc.Bookmarks("Total").Range.InsertAfter(Factura.Monto.ToString("###,###,###.00"));
                        WordDoc.Bookmarks("MontoLiteral").Range.InsertAfter(Factura.MontoLiteral);

                        // Pie
                        WordDoc.Bookmarks("CodigoControl").Range.InsertAfter(Factura.CodigoControl);
                        WordDoc.Bookmarks("FechaLimiteEmision").Range.InsertAfter(Factura.Dosificacion.FechaLimiteEmision.ToString("dd/MM/yyyy"));
                        if (Cfg.ImprimeQR)  // QR
                        {
                            QrGenerator qrGen = new QrGenerator();
                            string textoQr = qrGen.GeneraTextoQr(Empresa.NIT,
                                                                    Empresa.RazonSocial,
                                                                    Factura.Nro,
                                                                    Factura.Dosificacion.NroAutorizacion,
                                                                    Factura.FechaEmision,
                                                                    Factura.Monto,
                                                                    Factura.CodigoControl,
                                                                    -1M, // ICE
                                                                    FacturaTipos.SelectedItem.Codigo != "TUR" ? -1M : Factura.Excento,
                                                                    Factura.Dosificacion.FechaLimiteEmision,
                                                                    Factura.ClienteNIT,
                                                                    Factura.ClienteNombre);
                            Microsoft.LightSwitch.Threading.Dispatchers.Main.BeginInvoke(() => { qrGen.GenerarFileBmp(textoQr); });
                            for (int j = 0; !qrGen.Generado && j < 100; j++) { System.Threading.Thread.Sleep(100); }

                            // Adiciona QR al documento
                            if (qrGen.Generado)
                            {
                                var shapeQr = WordDoc.Bookmarks("QR").Range.InlineShapes.AddPicture(qrGen.FileBmp);
                                shapeQr.Width = 75;
                                shapeQr.Height = 75;
                            }
                        }
                        WordDoc.Bookmarks("Leyenda").Range.InsertAfter(Dosificacion.Leyenda);
                        WordDoc.Bookmarks("Subleyenda").Range.InsertAfter(Dosificacion.Subleyenda);

                        if(original)
                        {
                            // ORIGINAL
                            WordDoc.Bookmarks("OriginalCopia").Range.Text = "ORIGINAL";
                            WordApp.PrintOut();
                            Factura.NroImpresiones = 1;
                            WordDoc.Bookmarks("OriginalCopia").Range.Delete(1, 8);
                        }

                        if(nroCopias > 0)
                        {
                            // COPIA
                            int k = 0;
                            WordDoc.Bookmarks("OriginalCopia").Range.Text = "COPIA";
                            while (k < nroCopias)
                            {
                                this.ShowMessageBox("Coloque otra hoja para la copia", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                                WordApp.PrintOut();
                                k++;
                            }
                            Factura.NroImpresiones += k;
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        if (WordDoc != null)
                            WordDoc.Close(0);
                        if (WordApp != null)
                            WordApp.Quit();
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Falla al crear factura word!", ex);
                }
            }
        }

        #endregion

        #region Métodos Generados

        partial void ScreenFacturacion_InitializeDataWorkspace(List<IDataService> saveChangesTo)
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

            // GetTemplates();

            gridAnticipos.ControlAvailable += (object o, ControlAvailableEventArgs e) =>
            {
                dgAnticipos = (DataGrid)e.Control;
                dgAnticipos.SelectionMode = DataGridSelectionMode.Extended;
            };

            btnNuevaFactura.IsEnabled = false;

            CloseAreaFactura();
        }

        partial void ScreenFacturacion_Saving(ref bool handled)
        {
            MessageBoxResult result = this.ShowMessageBox(string.Format("Factura por {0}Bs.\nAl contado: {1}Bs.\nPor Cobrar: {2}Bs.\nCon Anticipos: {3}Bs.\nConfirma emisión de esta factura?", 
                                                            Factura.Monto, Factura.MontoContado, 
                                                            Factura.MontoPorCobrar, Factura.MontoAnticipado), "CONFIRMACION", 
                                                            MessageBoxOption.YesNo);
            if (result == MessageBoxResult.No)
                handled = true;
        }

        partial void ScreenFacturacion_Saved()
        {
            if (Cfg.FacturacionImprimeOriginal)
            {
                PrintFactura(original: true, nroCopias: Cfg.FacturacionNroCopias, headerIzq: Cfg.EncabezadoIzquierda);
            }

            this.Refresh();
            CloseAreaFactura();
        }

        partial void Abandonar_Execute()
        {
            if (areaFactura.IsVisible)
            {
                if (Factura != null)
                {
                    if (FacturaHospedaje != null)
                        FacturaHospedaje.Delete();
                    while (Factura.FacturaDetalles.Count() > 0)
                    {
                        Factura.FacturaDetalles.First().Delete();
                    }
                    Factura.Delete();
                }
                CloseAreaFactura();
            }
            else
                this.Close(promptUserToSave: false);
        }

        partial void AbandonarAnticipos_Execute()
        {
            Factura.MontoAnticipado = 0M;
            Factura.AnticiposOpNros = string.Empty;
            this.CloseModalWindow("ModalAnticipos");
        }

        partial void AbrirNuevaFactura_Execute()
        {
            NuevaFactura();
        }

        partial void ActualizaTotales_Execute()
        {
            if (Factura.FacturaTipo != null && Factura.FacturaTipo.Codigo == "COM")
            {
                foreach (var item in Factura.FacturaDetalles)
                {
                    if (item.Cantidad > 0)
                        item.Importe = item.Cantidad * item.PrecioUnitario;
                }
            }

            // Valida Factura turística tenga Tipo Hospedaje
            if (Factura.FacturaTipo.Codigo == "TUR" && FacturaHospedaje == null)
            {
                this.ShowMessageBox("La factura turística no tiene línea de detalle de Hospedaje", 
                                    SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }

            Factura.Subtotal = FacturaDetalles1.Sum(d => d.Importe);
            Factura.Monto = Factura.Subtotal - Factura.Descuento;
            Factura.MontoContado = Factura.Monto;
            Factura.MontoContadoBS = Factura.FacturaTipo.Codigo == "TUR" && FacturaHospedaje.ImporteUS > 0M ? 0 : Factura.Monto;
            Factura.MontoContadoUS = Factura.FacturaTipo.Codigo == "TUR" && FacturaHospedaje.ImporteUS > 0M ? Math.Round(Factura.Monto / Factura.TipoCambio, 2)  : 0;
            Factura.MontoPorCobrar = 0M;
            Factura.MontoAnticipado = 0M;
            Factura.AnticiposOpNros = string.Empty;

            Factura.MontoLiteral = Factura.Monto > 0 ? ServicesClient.ServicioFacturacion.MontoALiteral(Factura.Monto) : string.Empty;

            btnContado.IsEnabled = true;
            btnCalcularPagoBS.IsEnabled = true;
            btnCalcularPagoUS.IsEnabled = true;
            btnPorCobrar.IsEnabled = true;
            btnOtroClientePorCobrar.IsEnabled = true;
            btnBuscarClienteXCobrar.IsEnabled = true;
            btnAdicionarAnticipos.IsEnabled = true;
            btnRefresh.IsEnabled = true;
            btnSave.IsEnabled = true;
            btnPrintAlquiler.IsEnabled = true;
            btnPrintComercial.IsEnabled = true;
            btnPrintHotelera.IsEnabled = true;
            btnPrintServicios.IsEnabled = true;
        }

        partial void AdicionarAnticipos_Execute()
        {
            Cliente cliente = (from Cliente c in DataWorkspace.ApplicationData.Clientes
                               where c.NIT == Factura.ClienteNIT
                               select c).FirstOrDefault();
            if (cliente == null)
            {
                this.ShowMessageBox("Cliente para anticipos no encontrado!");
                return;
            }
            COClienteInicial = cliente.Id;
            COClienteFinal = cliente.Id;
            Anticipos.Load();
            Anticipos.SelectedItem = null;
            this.OpenModalWindow("ModalAnticipos");
        }

        partial void Anticipos_SelectionChanged()
        {
            if (dgAnticipos == null)
                return;

            Factura.MontoAnticipado = 0;
            foreach (ClienteOperacion item in dgAnticipos.SelectedItems)
            {
                Factura.MontoAnticipado += item.Monto;
            }
        }

        partial void BuscaCliente_Execute()
        {
            bool clienteOk = BuscaClienteFactura();
            if (clienteOk && !string.IsNullOrWhiteSpace(Factura.ClienteNIT) && !string.IsNullOrWhiteSpace(Factura.ClienteNombre))
                DetalleFactura_Execute();
        }

        partial void BuscarClienteXCobrar_Execute()
        {
            if (!string.IsNullOrWhiteSpace(Factura.ClienteXCobrarNIT))
            {
                Cliente cliXCobrar = (from Cliente c in DataWorkspace.ApplicationData.ClientesXEmpresa(Empresa.Id)
                                      where c.NIT == Factura.ClienteXCobrarNIT
                                      select c).FirstOrDefault();
                Factura.ClienteXCobrarNIT = cliXCobrar != null ? cliXCobrar.NIT : string.Empty;
                Factura.ClienteXCobrarNombre = cliXCobrar != null ? cliXCobrar.RazonSocial : string.Empty;
            }
            else
            {
                Cliente cliXCobrar = (from Cliente c in DataWorkspace.ApplicationData.ClientesXEmpresa(Empresa.Id)
                                      where c.RazonSocial.ToUpper().Contains(Factura.ClienteXCobrarNombre.ToUpper())
                                      select c).FirstOrDefault();
                Factura.ClienteXCobrarNIT = cliXCobrar != null ? cliXCobrar.NIT : string.Empty;
                Factura.ClienteXCobrarNombre = cliXCobrar != null ? cliXCobrar.RazonSocial : string.Empty;
            }
        }

        partial void CalcularImporteHospedaje_Execute()
        {
            if (FacturaHospedaje.DiasHospedaje == 0)
                FacturaHospedaje.DiasHospedaje = (FacturaHospedaje.FechaSalida - FacturaHospedaje.FechaIngreso).Days;
            if (FacturaHospedaje.DiasHospedaje == 0)
                FacturaHospedaje.DiasHospedaje = 1;
            if (FacturaHospedaje.PrecioDiaUS == 0M)
            {
                FacturaHospedaje.ImporteBS = FacturaHospedaje.DiasHospedaje * FacturaHospedaje.PrecioDiaBS;
            }
            else
            {
                FacturaHospedaje.PrecioDiaBS = FacturaHospedaje.PrecioDiaUS * FacturaHospedaje.TipoCambio;
                FacturaHospedaje.ImporteUS = FacturaHospedaje.DiasHospedaje * FacturaHospedaje.PrecioDiaUS;
                FacturaHospedaje.ImporteBS = FacturaHospedaje.ImporteUS * FacturaHospedaje.TipoCambio;
            }
        }

        partial void CalcularPagoBS_Execute()
        {
            // CalcularPagoBS
            Factura.MontoContadoBS = Factura.MontoContado - Math.Round(Factura.MontoContadoUS * Factura.TipoCambio, 2);
        }

        partial void CalcularPagoUS_Execute()
        {
            Factura.MontoContadoUS = Math.Round((Factura.MontoContado - Factura.MontoContadoBS) / Factura.TipoCambio, 2);
        }

        partial void ConfirmarAnticipos_Execute()
        {
            Factura.AnticiposOpNros = string.Empty;
            foreach (ClienteOperacion item in dgAnticipos.SelectedItems)
            {
                Factura.AnticiposOpNros += string.IsNullOrWhiteSpace(Factura.AnticiposOpNros) ? item.Id.ToString() : "-" + item.Id.ToString();
            }
            Factura.MontoContado = Factura.Monto >= Factura.MontoAnticipado ? Factura.Monto - Factura.MontoAnticipado : 0M;
            Factura.MontoContadoBS = Factura.MontoContado;
            Factura.MontoContadoUS = 0;
            this.CloseModalWindow("ModalAnticipos");
        }

        partial void Contado_Execute()
        {
            Factura.MontoContado = Factura.Monto;
            Factura.MontoContadoBS = Factura.Monto;
            Factura.MontoContadoUS = 0M;
            Factura.MontoPorCobrar = 0M;
            Factura.MontoAnticipado = 0M;
            Factura.AnticiposOpNros = string.Empty;
            Factura.ClienteXCobrarNIT = string.Empty;
            Factura.ClienteXCobrarNombre = string.Empty;
        }

        partial void CloseDatosHospedaje_Execute()
        {
            this.CloseModalWindow("DatosHospedaje");
            if (FacturaDetalles1.SelectedItem != null && FacturaDetalles1.SelectedItem.TipoDetalle.Codigo == "HOS")
            {
                if (Factura.FacturaTipo.Codigo == "HOT")
                {
                    FacturaDetalles1.SelectedItem.Concepto = string.Format("{0}{8}Huésped: {1}. Hab. {2}. \nDesde el {3} al {4}. Por {5} día{6} a Bs. {7}",
                                                                        FacturaHospedaje.Concepto, FacturaHospedaje.Persona, FacturaHospedaje.Habitacion,
                                                                        FacturaHospedaje.FechaIngreso.ToString("dd/MM/yyyy"),
                                                                        FacturaHospedaje.FechaSalida.ToString("dd/MM/yyyy"),
                                                                        FacturaHospedaje.DiasHospedaje,
                                                                        FacturaHospedaje.DiasHospedaje != 1 ? "s" : string.Empty,
                                                                        FacturaHospedaje.PrecioDiaBS,
                                                                        !string.IsNullOrWhiteSpace(FacturaHospedaje.Concepto) ? "\n" : string.Empty);
                }
                else if (Factura.FacturaTipo.Codigo == "TUR")
                {
                    if (FacturaHospedaje.PrecioDiaUS == 0M)     // BS
                    {
                        FacturaDetalles1.SelectedItem.Concepto = string.Format("{0}{9}Turista: {1}. PAS - {2}. Hab. {3}.\nDesde el {4} al {5}. Por {6} día{7} a Bs. {8}",
                                                                            FacturaHospedaje.Concepto, FacturaHospedaje.Persona, FacturaHospedaje.Pasaporte,
                                                                            FacturaHospedaje.Habitacion,
                                                                            FacturaHospedaje.FechaIngreso.ToString("dd/MM/yyyy"),
                                                                            FacturaHospedaje.FechaSalida.ToString("dd/MM/yyyy"),
                                                                            FacturaHospedaje.DiasHospedaje,
                                                                            FacturaHospedaje.DiasHospedaje != 1 ? "s" : string.Empty,
                                                                            FacturaHospedaje.PrecioDiaBS,
                                                                            !string.IsNullOrWhiteSpace(FacturaHospedaje.Concepto) ? "\n" : string.Empty);
                    }
                    else    // US
                    {
                        FacturaDetalles1.SelectedItem.Concepto = string.Format("{0}{11}Turista: {1}. PAS - {2}. Hab. {3}. Desde el {4} al {5}." + 
                                                                                "\nPor {6} día{7} a $US. {8}, son {9} $US. T/C {10}.",
                                                                            FacturaHospedaje.Concepto, FacturaHospedaje.Persona, FacturaHospedaje.Pasaporte,
                                                                            FacturaHospedaje.Habitacion,
                                                                            FacturaHospedaje.FechaIngreso.ToString("dd/MM/yyyy"),
                                                                            FacturaHospedaje.FechaSalida.ToString("dd/MM/yyyy"),
                                                                            FacturaHospedaje.DiasHospedaje,
                                                                            FacturaHospedaje.DiasHospedaje != 1 ? "s" : string.Empty,
                                                                            FacturaHospedaje.PrecioDiaUS,
                                                                            Math.Round(FacturaHospedaje.DiasHospedaje * FacturaHospedaje.PrecioDiaUS, 2),
                                                                            FacturaHospedaje.TipoCambio,
                                                                            !string.IsNullOrWhiteSpace(FacturaHospedaje.Concepto) ? "\n" : string.Empty);
                    }
                }
                
                FacturaDetalles1.SelectedItem.Importe = FacturaHospedaje.ImporteBS;
            }
        }

        partial void DatosCaja_Execute()
        {
            groupCaja.IsVisible = !groupCaja.IsVisible;
        }

        partial void DetalleFactura_Execute()
        {
            bool buscaOk = BuscaClienteFactura();
            if (buscaOk)
                OpenDetalleFactura();
        }

        partial void DetalleFacturaAlquilerAddNew_Execute()
        {
            NuevoDetalleFactura();
        }

        partial void DetalleFacturacion_Execute()
        {
            Application.ShowScreenDetalleFacturas(RegistroCaja.Id);
        }

        partial void FacturaDetallesComercialAddNew_Execute()
        {
            NuevoDetalleFactura();
        }

        partial void FacturaDetallesHoteleraAddNew_Execute()
        {
            NuevoDetalleFactura();
        }

        partial void FacturaDetallesServicioAddNew_Execute()
        {
            NuevoDetalleFactura();
        }

        partial void FacturaTipos_Loaded(bool succeeded)
        {
            if (FacturaTipos != null)
                FacturaTipos.SelectedItem = (from t in FacturaTipos
                                             where t.Codigo == Cfg.TipoFactura
                                             select t).FirstOrDefault();
        }

        partial void Hospedaje_Execute()
        {
            if (FacturaDetalles1.SelectedItem != null && FacturaDetalles1.SelectedItem.TipoDetalle.Codigo == "HOS")
            {
                if (FacturaHospedaje == null)
                {
                    FacturaHospedaje = Factura.FacturaHospedajes.AddNew();
                    FacturaHospedaje.Persona = !(Factura.ClienteNombre == "SIN NOMBRE") ? Factura.ClienteNombre : string.Empty;
                    FacturaHospedaje.FechaIngreso = FechaEmision;
                    FacturaHospedaje.FechaSalida = FechaEmision;
                    FacturaHospedaje.TipoCambio = Cfg.TipoCambioOficial;
                }
                this.OpenModalWindow("DatosHospedaje");
            }
        }

        partial void OtroClienteXCobrar_Execute()
        {
            Factura.ClienteXCobrarNIT = string.Empty;
            Factura.ClienteXCobrarNombre = string.Empty;
        }

        partial void PorCobrar_Execute()
        {
            Factura.MontoContado = 0M;
            Factura.MontoContadoBS = 0M;
            Factura.MontoContadoUS = 0M;
            Factura.MontoPorCobrar = Factura.Monto;
            Factura.MontoAnticipado = 0M;
            Factura.AnticiposOpNros = string.Empty;

            Factura.ClienteXCobrarNIT = Factura.ClienteNIT;
            Factura.ClienteXCobrarNombre = Factura.ClienteNombre;
        }

        partial void Print_Execute()
        {
            if (Factura.ClienteNombre == "?")
                Factura.ClienteNombre = string.Empty;
            this.Save();
        }

        partial void ResumenCaja_Execute()
        {
            Application.ShowScreenResumenCaja(RegistroCaja.Id);
        }

        partial void TodosLosAnticipos_Execute()
        {
            COClienteInicial = 0;
            COClienteFinal = int.MaxValue;
            Anticipos.Refresh();
        }

        #endregion
    }
}
