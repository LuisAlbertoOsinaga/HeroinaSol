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

namespace LightSwitchApplication
{
    public partial class ScreenCierreCaja
    {
        #region Propiedades

        IContentItemProxy groupCaja;
        IContentItemProxy groupEmpresas;

        string TemplateResumenCaja;

        #endregion

        #region Metodos Auxiliares

        void Bindings()
        {
            groupCaja.ControlAvailable += GroupCaja_ControlAvailable;
        }

        void Enablings()
        {
        }

        void FindControls()
        {
            groupCaja = this.FindControl("GroupCaja");
            groupEmpresas = this.FindControl("GroupEmpresas");
        }

        private void GroupCaja_ControlAvailable(object sender, ControlAvailableEventArgs e)
        {
            Control box = (Control)e.Control;
            box.Background = new SolidColorBrush(Color.FromArgb(255, Globales.CajaColorRed, Globales.CajaColorGreen, Globales.CajaColorBlue));
        }

        void GetTemplateResumenCaja()
        {
            if (!string.IsNullOrWhiteSpace(TemplateResumenCaja))
                return;

            // FileStream al template word
            string doctemplate = "ResumenCaja_01.dotx";
            dynamic path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + doctemplate;
            dynamic file = System.IO.File.Create(path);
            file.Close();

            //Write the stream to the file
            var resourceInfo = System.Windows.Application.GetResourceStream(new Uri("Resources/" + doctemplate, UriKind.Relative));
            System.IO.Stream stream = resourceInfo.Stream;
            using (FileStream fileStream = System.IO.File.Open(path, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write,
                                                                System.IO.FileShare.None))
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, int.Parse(stream.Length.ToString()));
                fileStream.Write(buffer, 0, buffer.Length);
            }

            TemplateResumenCaja = path;
        }

        bool HayCajaAbierta()
        {
            if (Cfg.RegistroCaja == 0)
                return false;

            RegistroCaja = (from rc in DataWorkspace.ApplicationData.RegistroCajas
                                where rc.Id == Cfg.RegistroCaja
                                select rc).SingleOrDefault();
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

        void PrintResumenCaja(bool ParaCierreCaja = false)
        {
            if (!AutomationFactory.IsAvailable)
            {
                this.ShowMessageBox("No está habilitada la Automatización Word!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                return;
            }
            try
            {
                // Imprimir a través de word
                dynamic wordApp = null;
                dynamic wordDoc = null;
                try
                {
                    // Preparacion
                    GetTemplateResumenCaja();
                    wordApp = AutomationFactory.CreateObject("Word.Application");
                    wordDoc = wordApp.Documents.Open(TemplateResumenCaja);

                    // Llenado Datos
                    wordDoc.Bookmarks("Empresa").Range.InsertBefore(Empresa.Nombre);
                    wordDoc.Bookmarks("Sucursal").Range.InsertBefore(Sucursal.Nombre);
                    wordDoc.Bookmarks("Puesto").Range.InsertAfter(RegistroCaja.Autoimpresor.Nombre);
                    wordDoc.Bookmarks("Turno").Range.InsertAfter(RegistroCaja.Turno.Nombre);
                    wordDoc.Bookmarks("Fecha").Range.InsertAfter(RegistroCaja.Fecha.ToString("dd-MM-yyyy"));
                    wordDoc.Bookmarks("Usuario").Range.InsertAfter(RegistroCaja.Usuario);
                    wordDoc.Bookmarks("TipoCambio").Range.InsertAfter(RegistroCaja.TipoCambio);
                    wordDoc.Bookmarks("VentasContado").Range.InsertAfter(RegistroCaja.FacturadoContadoBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("VentasConAnticipos").Range.InsertAfter(RegistroCaja.FacturadoConAnticiposBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("VentasPorCobrar").Range.InsertAfter(RegistroCaja.FacturadoXCobrarBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("TotalVentas").Range.InsertAfter(RegistroCaja.TotalFacturadoBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("FacturasEmitidas").Range.InsertAfter(RegistroCaja.CantidadFacturas.ToString());
                    wordDoc.Bookmarks("FacturasAnuladas").Range.InsertAfter(RegistroCaja.FacturasAnuladas.ToString());
                    wordDoc.Bookmarks("PrimeraFactura").Range.InsertAfter(string.Format("{0} ({1})", RegistroCaja.FacturaInicialNro,
                                                                                                    RegistroCaja.FacturaInicialFecha.ToString("dd-MM-yyyy hh:mm:ss")));
                    wordDoc.Bookmarks("UltimaFactura").Range.InsertAfter(string.Format("{0} ({1})", RegistroCaja.FacturaFinalNro,
                                                                                                    RegistroCaja.FacturaFinalFecha.ToString("dd-MM-yyyy hh:mm:ss")));
                    wordDoc.Bookmarks("RecaudacionContado").Range.InsertAfter(RegistroCaja.FacturadoContadoBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("RecaudacionCobranzas").Range.InsertAfter(RegistroCaja.TotalCobranzasBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("RecaudacionAnticipos").Range.InsertAfter(RegistroCaja.TotalAnticiposBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("TotalRecaudacion").Range.InsertAfter(RegistroCaja.TotalRecaudacionBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("EfectivoBS").Range.InsertAfter(RegistroCaja.EfectivoBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("EfectivoUS").Range.InsertAfter(RegistroCaja.EfectivoUS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("ChequesPropiosBS").Range.InsertAfter(RegistroCaja.ChequesPropiosBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("ChequesPropiosUS").Range.InsertAfter(RegistroCaja.ChequesPropiosUS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("ChequesAjenosBS").Range.InsertAfter(RegistroCaja.ChequesAjenosBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("ChequesAjenosUS").Range.InsertAfter(RegistroCaja.ChequesAjenosUS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("TarjetasCreditoBS").Range.InsertAfter(RegistroCaja.TarjetasBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("TarjetasCreditoUS").Range.InsertAfter(RegistroCaja.TarjetasUS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("DepositosBS").Range.InsertAfter(RegistroCaja.DepositosBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("DepositosUS").Range.InsertAfter(RegistroCaja.DepositosUS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("RecaudacionBS").Range.InsertAfter(RegistroCaja.RecaudacionBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("RecaudacionUS").Range.InsertAfter(RegistroCaja.RecaudacionUS.ToString("###,###,###.00"));
                    if (ParaCierreCaja)
                    {
                        string CierreCaja = string.Format("Registro: {0} - Apertura: {1} - Cierre: {2}",
                                            RegistroCaja.Id.ToString(),
                                            RegistroCaja.HoraInicio.ToString("dd/MM/yyyy - hh:mm:ss"),
                                            RegistroCaja.HoraFinal.ToString("dd/MM/yyyy - hh:mm:ss"));
                        string codigoControl = Math.Abs((CierreCaja + EmpresaNombre).GetHashCode()).ToString();
                        CierreCaja += "     |" + codigoControl + "|";
                        wordDoc.Bookmarks("DatosCierre").Range.InsertAfter(CierreCaja);
                    }

                    // Print
                    wordApp.PrintOut();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (wordDoc != null)
                        wordDoc.Close(0);
                    if (wordApp != null)
                        wordApp.Quit();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Falla al crear factura word!", ex);
            }
        }

        #endregion

        #region Metodos Generados

        partial void ScreenCierreCaja_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();

            if (!HayCajaAbierta())
            {
                this.ShowMessageBox("Caja no abierta!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
                this.Close(promptUserToSave: false);
                return;
            }

            if(!(Application.Current.User.Name == RegistroCaja.Usuario))
            {
                MessageBoxResult result = this.ShowMessageBox(string.Format("Caja abierta por el usuario: '{0}'! \n Desea cerrar esta Caja?", RegistroCaja.Usuario),
                                        SoftCliente.SoftProductoNombre, MessageBoxOption.YesNo);
                if (result == MessageBoxResult.No)
                {
                    this.Close(promptUserToSave: false);
                    return;
                }
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

        partial void CierreCaja_Execute()
        {
            MessageBoxResult result = this.ShowMessageBox("Confirma el cierre de su Caja?", SoftCliente.SoftProductoNombre, MessageBoxOption.YesNo);
            if (result == MessageBoxResult.No)
                return;

            int idRegistroCaja = Cfg.RegistroCaja;
            DateTime ahora = DateTime.Now;

            // Cierra Caja
            RegistroCaja.HoraFinal = ahora;
            RegistroCaja.CajaAbierta = false;
            Cfg.RegistroCaja = 0;
            Save();

            // Vuelve a cargar para mostrar resultados
            int registroCajaId = RegistroCaja.Id;
            RegistroCaja = (from rc in DataWorkspace.ApplicationData.RegistroCajas
                            where rc.Id == registroCajaId
                            select rc).SingleOrDefault();

            // Parémetros para cierre de caja
            Parametro parCierre = (from Parametro p in DataWorkspace.ApplicationData.Parametros
                                   where p.Empresa.Id == Empresa.Id && 
                                   p.Categoria == "SISTEMA" && p.Clave == "CIERRE_IMPRIME_RESUMEN"
                                   select p).FirstOrDefault();
            bool imprimeResumen = parCierre != null ? parCierre.Valor.ToUpper() == "S" : false;
            if (imprimeResumen)
                PrintResumenCaja();

            // Parametros Mails
            List<Parametro> pars;
            pars = (from Parametro p in DataWorkspace.ApplicationData.Parametros
                    where p.Empresa.Id == Empresa.Id && p.Categoria == "SMTP_ENABLED"
                    select p).ToList();
            Parametro parEnabled = pars.FirstOrDefault(p => p.Clave == "ENABLED");
            bool smtpEnabled = parEnabled != null && parEnabled.Valor == "S";
            Parametro parCajaEnabled = pars.FirstOrDefault(p => p.Clave == "CAJA_ENABLED");
            bool smtpCajaEnabled = parCajaEnabled != null && parCajaEnabled.Valor == "S";
            Parametro parCierreEnabled = pars.FirstOrDefault(p => p.Clave == "CIERRE_ENABLED");
            bool smtpCierreEnabled = parCierreEnabled != null && parCierreEnabled.Valor == "S";

            // Send Mails
            if (smtpEnabled && smtpCajaEnabled && smtpCierreEnabled)
            {
                string body = "CAJA CERRADA";

                if (RegistroCaja != null)
                {
                    body += string.Format("\n\nSucursal: {0}\nPuesto: {1}\nTurno: {2}\nFecha: {3}\nUsuario: {4}\nT/C: {5}",
                                                     RegistroCaja.Autoimpresor.Sucursal.Nombre,
                                                     RegistroCaja.Autoimpresor.Nombre,
                                                     RegistroCaja.Turno.Nombre,
                                                     RegistroCaja.HoraFinal.ToString("dd/MM/yyyy - hh:mm:ss"),
                                                     RegistroCaja.Usuario,
                                                     RegistroCaja.TipoCambio.ToString("###,###,###.0000"));
                    body += string.Format("\n\nFacturado al contado (Bs): {0}\nFacturado con anticipos (Bs): {1}\n" +
                                            "Facturado por cobrar (Bs): {2}\nTotal facturado (Bs): {3}",
                                                    RegistroCaja.FacturadoContadoBS.ToString("###,###,###.00"),
                                                    RegistroCaja.FacturadoConAnticiposBS.ToString("###,###,###.00"),
                                                    RegistroCaja.FacturadoXCobrarBS.ToString("###,###,###.00"),
                                                    RegistroCaja.TotalFacturadoBS.ToString("###,###,###.00"));
                    body += string.Format("\n\nCantidad de facturas: {0}\nFacturas anuladas: {1}",
                                                    RegistroCaja.CantidadFacturas,
                                                    RegistroCaja.FacturasAnuladas);
                    body += string.Format("\n\nRecibido contado (Bs): {0}\nRecibido con anticipos (Bs): {1}\nRecibido por cobrar (Bs): {2}\nTotal recibido (Bs): {3}\n",
                                                    RegistroCaja.FacturadoContadoBS,
                                                    RegistroCaja.TotalAnticiposBS,
                                                    RegistroCaja.TotalCobranzasBS,
                                                    RegistroCaja.TotalRecaudacionBS);
                }

                Utilidades.SendMails("info@CashFlow.com", "SUPERVISOR", 
                                        string.Format("CASHFOW - CIERRE DE CAJA - {0}", RegistroCaja.HoraFinal), 
                                        body, RegistroCaja.HoraFinal, Empresa, DataWorkspace.ApplicationData);
            }

            // Realimentación
            this.ShowMessageBox("Caja exitosamente cerrada!", SoftCliente.SoftProductoNombre, MessageBoxOption.Ok);
            this.Close(promptUserToSave: false);
        }

        partial void DetalleFacturacion_Execute()
        {
            Application.ShowScreenDetalleFacturas(RegistroCaja.Id);
        }

        partial void ResumenCaja_Execute()
        {
            Application.ShowScreenResumenCaja(RegistroCaja.Id);
        }

        partial void ScreenCierreCaja_Closing(ref bool cancel)
        {
            var screens = this.Application.ActiveScreens; 
            foreach (var s in screens)
            {
                var screen = s.Screen;
                screen.Details.Dispatcher.BeginInvoke(() =>
                    {
                        screen.Close(promptUserToSave: false);
                    }
                );
            }
        }

        #endregion
    }
}
