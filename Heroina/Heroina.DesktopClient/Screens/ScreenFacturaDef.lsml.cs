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
    public partial class ScreenFacturaDef
    {
        #region Propiedades

        IContentItemProxy comboEstado;
        IContentItemProxy comboSituacion;
        IContentItemProxy groupFacturaHeader;

        IContentItemProxy groupDetalleServicios;
        IContentItemProxy groupDetalleComercial;
        IContentItemProxy groupDetalleHotelera;
        IContentItemProxy groupDetalleAlquiler;

        IContentItemProxy btnAnularDesanular;
        
        // Enable Edicion
        IContentItemProxy btnAdicionarAnticipo;
        IContentItemProxy btnBuscarCliente;
        IContentItemProxy btnCalcularBS;
        IContentItemProxy btnCalcularUS;
        IContentItemProxy btnContado;
        IContentItemProxy btnOtroCliente;
        IContentItemProxy btnPorCobrar;
        IContentItemProxy cbxMedioPagoContadoBS;
        IContentItemProxy cbxMedioPagoContadoUS;
        IContentItemProxy mnvMontoPorCobrar;
        IContentItemProxy mnvTipoCambio;
        IContentItemProxy txtClienteXCobrarNIT;
        IContentItemProxy txtClienteXCobrarNombre;
        IContentItemProxy txtMontoCalcularPagoBS;
        IContentItemProxy txtMontoCalcularPagoUS;
        IContentItemProxy txtMontoContadoBS;

        DataGrid dgAnticipos;
        IContentItemProxy gridAnticipos;

        string[] Mes = { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

        dynamic WordApp = null;
        dynamic WordDoc = null;

        #endregion

        #region Metodos Auxiliares

        void EnableEdicion()
        {
            btnAdicionarAnticipo.IsVisible = HabilitarEdicion;
            btnBuscarCliente.IsVisible = HabilitarEdicion;
            btnCalcularBS.IsVisible = HabilitarEdicion;
            btnCalcularUS.IsVisible = HabilitarEdicion;
            btnContado.IsVisible = HabilitarEdicion;
            btnOtroCliente.IsVisible = HabilitarEdicion;
            btnPorCobrar.IsVisible = HabilitarEdicion;
            cbxMedioPagoContadoBS.IsEnabled = HabilitarEdicion;
            cbxMedioPagoContadoUS.IsEnabled = HabilitarEdicion;
            mnvMontoPorCobrar.IsEnabled = HabilitarEdicion;
            mnvTipoCambio.IsEnabled = HabilitarEdicion;
            txtClienteXCobrarNIT.IsEnabled = HabilitarEdicion;
            txtClienteXCobrarNombre.IsEnabled = HabilitarEdicion;
            txtMontoContadoBS.IsEnabled = HabilitarEdicion;
            txtMontoCalcularPagoBS.IsEnabled = HabilitarEdicion;
            txtMontoCalcularPagoUS.IsEnabled = HabilitarEdicion;
        }

        void Enablings()
        {
            comboEstado.IsEnabled = false;
            comboSituacion.IsEnabled = false;
            btnAnularDesanular.IsVisible = HabilitarAnularDesanular;
            if (btnAnularDesanular.IsVisible)
                btnAnularDesanular.DisplayName = Factura.Estado == "V" ? "Anular" : "Desanular";

            EnableEdicion();
        }

        void FindControls()
        {
            comboEstado = this.FindControl("Estado");
            comboSituacion = this.FindControl("Situacion");
            groupFacturaHeader = this.FindControl("GroupFacturaHeader");

            groupDetalleServicios = this.FindControl("GroupFacturaServicio");
            groupDetalleComercial = this.FindControl("GroupDetalleComercial");
            groupDetalleHotelera = this.FindControl("GroupDetalleHotelera");
            groupDetalleAlquiler = this.FindControl("GroupDetalleAlquiler");

            gridAnticipos = this.FindControl("GridAnticipos");

            btnAnularDesanular = this.FindControl("AnularDesanular");
            
            // Enable Edicion
            btnAdicionarAnticipo = this.FindControl("AdicionarAnticipo");
            btnBuscarCliente = this.FindControl("BuscarCliente");
            btnCalcularBS = this.FindControl("CalcularPagoBS");
            btnCalcularUS = this.FindControl("CalcularPagoUS");
            btnContado = this.FindControl("Contado");
            btnOtroCliente = this.FindControl("OtroCliente");
            btnPorCobrar = this.FindControl("PorCobrar");
            cbxMedioPagoContadoBS = this.FindControl("MedioPagoContadoBS");
            cbxMedioPagoContadoUS = this.FindControl("MedioPagoContadoUS");
            mnvMontoPorCobrar = this.FindControl("MontoPorCobrar");
            mnvTipoCambio = this.FindControl("TipoCambio");
            txtClienteXCobrarNIT = this.FindControl("ClienteXCobrarNIT");
            txtClienteXCobrarNombre = this.FindControl("ClienteXCobrarNombre");
            txtMontoContadoBS = this.FindControl("MontoContadoBS");
            txtMontoCalcularPagoBS = this.FindControl("MontoContadoUS");
            txtMontoCalcularPagoUS = this.FindControl("MontoContado");
        }

        string GetPathTemplate()
        {
            // path al file template en Documents
            string fileTemplate = null;
            if (Factura.FacturaTipo.Codigo == "ALQ")
                fileTemplate = Cfg.FileTemplateAlquiler;
            else if (Factura.FacturaTipo.Codigo == "COM")
                fileTemplate = Cfg.FileTemplateComercial;
            else if (Factura.FacturaTipo.Codigo == "HOT" || Factura.FacturaTipo.Codigo == "TUR")
                fileTemplate = Cfg.FileTemplateHotelera;
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

        void Inits()
        {
            Empresa = (from e in DataWorkspace.ApplicationData.Empresas
                       where e.Nombre == Cfg.Empresa
                       select e).SingleOrDefault();
            EmpresaNombre = SoftCliente.NroEmpresas > 1 || Empresa == null ? SoftCliente.ClienteNombre : Empresa.Nombre;

            if (Empresa != null)
            {
                Factura = (from Factura f in DataWorkspace.ApplicationData.Facturas
                           where f.Dosificacion.Autoimpresor.Sucursal.Empresa.Id == Empresa.Id 
                                    && f.Dosificacion.NroAutorizacion == NroAutorizacion && f.Nro == FacturaNumero
                           select f).SingleOrDefault();
            }

            groupFacturaHeader.DisplayName = string.Format("Factura {0} - {1}", NroAutorizacion, FacturaNumero);
        }

        void PrintFacturax(bool original, int nroCopias, bool headerIzq)
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
                            WordDoc.Bookmarks("Sucursal").Range.InsertBefore(Factura.Dosificacion.Autoimpresor.Sucursal.Nombre);
                            WordDoc.Bookmarks("Calle").Range.InsertAfter(Factura.Dosificacion.Autoimpresor.Sucursal.Calle);
                            WordDoc.Bookmarks("Zona").Range.InsertAfter(Factura.Dosificacion.Autoimpresor.Sucursal.Zona);
                            WordDoc.Bookmarks("Telefonos").Range.InsertAfter(Factura.Dosificacion.Autoimpresor.Sucursal.Telefonos);
                            WordDoc.Bookmarks("Municipio").Range.InsertAfter(Factura.Dosificacion.Autoimpresor.Sucursal.Ciudad + "-" + Empresa.Pais);
                        }

                        // Derecha
                        WordDoc.Bookmarks("EmpresaNIT").Range.InsertAfter(Empresa.NIT);
                        WordDoc.Bookmarks("FacturaNro").Range.InsertAfter(Factura.Nro);
                        WordDoc.Bookmarks("AutorizacionNro").Range.InsertAfter(Factura.Dosificacion.NroAutorizacion);
                        WordDoc.Bookmarks("ActividadEconomica").Range.InsertAfter(Factura.Dosificacion.ActividadEconomica.Abreviacion);
                        WordDoc.Bookmarks("Autoimpresor").Range.InsertAfter("SFC " + Factura.Dosificacion.Autoimpresor.NroAutoImpresor.ToString());

                        // Cabecera
                        if (Factura.FacturaTipo.Codigo == "TUR")
                            WordDoc.Bookmarks("Titulo").Range.InsertBefore("FACTURA TURÍSTICA");
                        else if (Factura.FacturaTipo.Codigo == "ALQ")
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
                            if (Factura.FacturaTipo.Codigo == "COM" && Cfg.FacturaComercialRollo)
                                WordDoc.Tables[2].Rows.Add(WordDoc.Tables[2].Rows[i]);
                            else
                                WordDoc.Tables[6].Rows.Add(WordDoc.Tables[6].Rows[i]);
                        }

                        if (Factura.FacturaTipo.Codigo == "COM")
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
                        else if (Factura.FacturaTipo.Codigo == "HOT" || Factura.FacturaTipo.Codigo == "TUR")
                        {
                            WordDoc.Tables[6].Cell(1, 1).Range.Text = "CONCEPTO";
                            WordDoc.Tables[6].Cell(1, 2).Range.Text = "DESCRIPCIÓN";
                            WordDoc.Tables[6].Cell(1, 3).Range.Text = "IMPORTE (Bs)";
                        }
                        else if (Factura.FacturaTipo.Codigo == "ALQ")
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
                            if (Factura.FacturaTipo.Codigo == "COM")
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
                            else if (Factura.FacturaTipo.Codigo == "HOT" || Factura.FacturaTipo.Codigo == "TUR")
                            {
                                WordDoc.Tables[6].Cell(i, 1).Range.Text = factDet.TipoDetalle.Descripcion;
                                WordDoc.Tables[6].Cell(i, 2).Range.Text = factDet.Concepto;
                                WordDoc.Tables[6].Cell(i, 3).Range.Text = factDet.Importe.ToString("###,###,###.00");
                            }
                            else if (Factura.FacturaTipo.Codigo == "ALQ")
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

                        if (Factura.FacturaTipo.Codigo != "SER" && Factura.FacturaTipo.Codigo != "ALQ")
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
                                                                    Factura.FacturaTipo.Codigo != "TUR" ? -1M : Factura.Excento,
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
                        WordDoc.Bookmarks("Leyenda").Range.InsertAfter(Factura.Dosificacion.Leyenda);
                        WordDoc.Bookmarks("Subleyenda").Range.InsertAfter(Factura.Dosificacion.Subleyenda);

                        if (original)
                        {
                            // ORIGINAL
                            WordDoc.Bookmarks("OriginalCopia").Range.Text = "ORIGINAL";
                            WordApp.PrintOut();
                            Factura.NroImpresiones = 1;
                            WordDoc.Bookmarks("OriginalCopia").Range.Delete(1, 8);
                        }

                        if (nroCopias > 0)
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

        #region Metodos Generados

        partial void ScreenFacturaDef_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();
            FindControls();
            Inits();
            Enablings();

            gridAnticipos.ControlAvailable += (object o, ControlAvailableEventArgs e) =>
            {
                dgAnticipos = (DataGrid)e.Control;
                dgAnticipos.SelectionMode = DataGridSelectionMode.Extended;
            };

        }

        partial void ScreenFacturaDef_Saved()
        {
            this.Refresh();
        }

        partial void AbandonarAnticipos_Execute()
        {
            Factura.MontoAnticipado = 0M;
            Factura.AnticiposOpNros = string.Empty;
            this.CloseModalWindow("ModalAnticipos");
        }

        partial void AdicionarAnticipo_Execute()
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

        partial void AnularDesanular_Execute()
        {
            if(Factura.Estado == "V")
                this.OpenModalWindow("ModalAnulacion");
            else
            {
                MessageBoxResult result = this.ShowMessageBox("Confirma desanulación de esta factura?", SoftCliente.SoftProductoNombre,
                                                                MessageBoxOption.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Factura.Estado = "V";
                    Save();
                }
            }
        }

        partial void BuscarCliente_Execute()
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

        partial void CalcularPagoBS_Execute()
        {
            // CalcularPagoBS
            Factura.MontoContadoBS = Factura.MontoContado - Math.Round(Factura.MontoContadoUS * Factura.TipoCambio, 2);
        }

        partial void CalcularPagoUS_Execute()
        {
            Factura.MontoContadoUS = Math.Round((Factura.MontoContado - Factura.MontoContadoBS) / Factura.TipoCambio, 2);
        }

        partial void CancelarAnulacion_Execute()
        {
            this.CloseModalWindow("ModalAnulacion");
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

        partial void ConfirmarAnulacion_Execute()
        {
            if(Factura.CausaAnulacion == null)
            {
                this.ShowMessageBox("Se requiere una causa de anulación!", SoftCliente.SoftProductoNombre,
                    MessageBoxOption.Ok);
                return;
            }
            if(Factura.CausaAnulacion.Codigo != "FIM" && string.IsNullOrWhiteSpace(CodigoControl))
            {
                this.ShowMessageBox("Para esta causa de anulación se requiere el Código de Control de la factura!", SoftCliente.SoftProductoNombre,
                    MessageBoxOption.Ok);
                return;
            }
            if (!string.IsNullOrWhiteSpace(CodigoControl) && !(Factura.CodigoControl == CodigoControl))
            {
                this.ShowMessageBox("Código de Control incorrecto!", SoftCliente.SoftProductoNombre,
                    MessageBoxOption.Ok);
                return;
            }

            this.CloseModalWindow("ModalAnulacion");
            Factura.Estado = "A";

            ClienteOperacion cuentaXCobrar = this.DataWorkspace.ApplicationData.ClienteOperacionXCobrarXNroAutoYFacturaNro(Factura.Dosificacion.NroAutorizacion,
                                                                                                                            Factura.Nro);
            if (cuentaXCobrar != null)
                cuentaXCobrar.Estado = "A";

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
            Parametro parAnulacionEnabled = pars.FirstOrDefault(p => p.Clave == "ANULACION_ENABLED");
            bool smtpAnulacionEnabled = parAnulacionEnabled != null && parAnulacionEnabled.Valor == "S";

            // Send Email
            if (smtpEnabled && smtpCajaEnabled && smtpAnulacionEnabled)
            {
                string body = "FACTURA ANULADA";

                if (Factura != null)
                {
                    body += string.Format("\n\nFactura Nro: {0}\nNIT Cliente: {1}\nNombre Cliente: {2}\nMonto (Bs): {3}\nCausa de anulación: {4}\nUsuario: {5}",
                                                     Factura.Nro,
                                                     Factura.ClienteNIT,
                                                     Factura.ClienteNombre,
                                                     Factura.Monto.ToString("###,###,###.00"),
                                                     Factura.CausaAnulacion.Descripcion,
                                                     Factura.CreadoPor);
                }

                Utilidades.SendMails("info@CashFlow.com", "SUPERVISOR", 
                                        string.Format("CASHFOW - ANULACION DE FACTURA - {0}", Factura.Nro), 
                                        body, DateTime.Now, Empresa, DataWorkspace.ApplicationData);
            }
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

        partial void FacturaDetalles_Loaded(bool succeeded)
        {
            groupDetalleServicios.IsVisible = false;
            groupDetalleComercial.IsVisible = false;
            groupDetalleHotelera.IsVisible = false;
            groupDetalleAlquiler.IsVisible = false;
            
            if (Factura.FacturaTipo.Codigo == "SER")
                groupDetalleServicios.IsVisible = true;
            else if (Factura.FacturaTipo.Codigo == "COM")
                groupDetalleComercial.IsVisible = true;
            else if (Factura.FacturaTipo.Codigo == "HOT" || Factura.FacturaTipo.Codigo == "TUR")
                groupDetalleHotelera.IsVisible = true;
            else if (Factura.FacturaTipo.Codigo == "ALQ")
                groupDetalleAlquiler.IsVisible = true;
        }

        partial void ImprimirCopia_Execute()
        {
            MessageBoxResult result = this.ShowMessageBox("Confirma impresión de esta factura?", SoftCliente.SoftProductoNombre, MessageBoxOption.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                if(HabilitarEdicion)
                    Save();
                PrintFacturax(original: false, nroCopias: 1, headerIzq: Cfg.EncabezadoIzquierda);
                Save();
            }
        }

        partial void OtroCliente_Execute()
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

        partial void TodosLosAnticipos_Execute()
        {
            COClienteInicial = 0;
            COClienteFinal = int.MaxValue;
            Anticipos.Refresh();
        }

        #endregion
    }
}
