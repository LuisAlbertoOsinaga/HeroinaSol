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
    public partial class ScreenResumenCaja
    {
        #region Propiedades

        IContentItemProxy btnImprimeResumen;
        IContentItemProxy groupEmpresa;
        IContentItemProxy groupResumenCaja;
        string TemplateResumenCaja;

        #endregion

        #region Metodos Auxiliares

        void Enablings()
        {
            btnImprimeResumen.IsVisible = true;
            groupEmpresa.IsVisible = Empresa != Empresas.SelectedItem;
            groupResumenCaja.IsVisible = true;
        }

        void FindControls()
        {
            btnImprimeResumen = this.FindControl("ImprimirResumen");
            groupEmpresa = this.FindControl("GroupEmpresa");
            groupResumenCaja = this.FindControl("GroupResumenCaja");
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
            RegistroCaja = DataWorkspace.ApplicationData.RegistroCajas_SingleOrDefault(RegistroCajaId);
            Sucursal = RegistroCaja.Autoimpresor.Sucursal;
            Empresa = RegistroCaja.Autoimpresor.Sucursal.Empresa;
            EstadoIni = "A";
            EstadoFin = "Z";
            TipoFactIni = "AAA";
            TipoFactFin = "ZZZ";
            FechaIni = RegistroCaja.HoraInicio;
            FechaFin = RegistroCaja.HoraFinal;
        }
      
        void ResumenCaja()
        {
            Proceso proceso = DataWorkspace.ApplicationData.Procesos.AddNew();
            proceso.Empresa = Empresa;
            proceso.Descripcion = "RESUMEN CAJA";
            proceso.Data = RegistroCaja.Id.ToString();
            DataWorkspace.ApplicationData.SaveChanges();

            Inits();   // Recupera Registro Caja Actualizado
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

        partial void ScreenResumenCaja_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();
            FindControls();
            Inits();
            ResumenCaja();
            Enablings();
        }

        partial void DetalleFacturacion_Execute()
        {
            Application.ShowScreenDetalleFacturas(RegistroCaja.Id);
        }

        partial void Empresas_SelectionChanged()
        {
            EmpresaNombre = Empresas.SelectedItem != null && Empresas.Count == 1 ? Empresas.SelectedItem.Nombre : SoftCliente.ClienteNombre;
        }

        partial void ImprimirResumen_Execute()
        {
            PrintResumenCaja();
        }

        #endregion
    }
}
