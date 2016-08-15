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
    public partial class ScreenDetalleFacturas
    {
        #region Propiedades

        IContentItemProxy groupFacturaDetalle;

        #endregion

        #region Metodos Auxiliares
   
        void FindControls()
        {
            groupFacturaDetalle = this.FindControl("GroupFacturaDetalle");
        }

        string GetPathTemplate()
        {
            // path al file template en Documents
            string fileTemplate = "DetalleFacturacion_01.dotx";
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
            EmpresaId = RegistroCaja.Autoimpresor.Sucursal.Empresa.Id;
            SucursalId = RegistroCaja.Autoimpresor.Sucursal.Id;
            AutoimpresorId = RegistroCaja.Autoimpresor.Id;
            Usuario = RegistroCaja.Usuario;
            FechaInicial = RegistroCaja.HoraInicio;
            FechaFinal = RegistroCaja.HoraFinal == RegistroCaja.HoraInicio ? new DateTime(3000, 1, 1) : RegistroCaja.HoraFinal;

            Empresa = DataWorkspace.ApplicationData.Empresas_SingleOrDefault(EmpresaId);
            Sucursal = DataWorkspace.ApplicationData.Sucursals_SingleOrDefault(SucursalId);
            Autoimpresor = DataWorkspace.ApplicationData.Autoimpresores_SingleOrDefault(AutoimpresorId);

            // Cantidad y Total facturado
            CantidadFacturas = Facturas.Count;
            FacturasAnuladas = Facturas.Where(f => f.Estado == "A").Count();
            TotalFacturadoBS = Facturas.Where(f => f.Estado == "V").Sum(f => f.Monto);  // Total Facturado en BS (libro de venta)

            // TotalFacturadoBS dividido en Al Contado, Con Anticipos y Por Cobrar 
            FacturadoContadoBS = Facturas.Where(f => f.Estado == "V").Sum(f => f.MontoContado);
            FacturadoConAnticiposBS = Facturas.Where(f => f.Estado == "V").Sum(f => f.MontoAnticipado);
            FacturadoPorCobrarBS = Facturas.Where(f => f.Estado == "V").Sum(f => f.MontoPorCobrar);
            
            // Facturado y Cobrado en US, expresado en US
            FacturadoUS = Facturas.Where(f => (f.Estado == "V" && f.TipoCambio == Cfg.TipoCambioOficial)).Sum(f => f.MontoContadoUS);
            CobradoUS = Facturas.Where(f => (f.Estado == "V" && f.TipoCambio == Cfg.TipoCambio)).Sum(f => f.MontoContadoUS);

            // Facturado y Cobrado en US, expresado en BS
            FacturadoUS_EnBs = Math.Round(FacturadoUS * Cfg.TipoCambioOficial, 2);
            CobradoUS_EnBs = Math.Round(CobradoUS * Cfg.TipoCambio, 2);

            // Facturado y Cobrado en BS
            FacturadoBS = Facturas.Where(f => (f.Estado == "V")).Sum(f => f.MontoContadoBS);
            
            // TotalVantasBS debe ser igual a TotalFacturadoBS
            TotalVentasBS = FacturadoUS_EnBs + CobradoUS_EnBs + FacturadoBS;
        }

        private void PrintFacturas()
        {
            if (AutomationFactory.IsAvailable)
            {
                dynamic wordApp = null;
                dynamic wordDoc = null;
                try
                {
                    // Imprimir a través de word
                    try
                    {
                        string path = GetPathTemplate();
                        try
                        {
                            wordApp = AutomationFactory.CreateObject("Word.Application");
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException("Falla al crear app word!", ex);
                        }
                        wordDoc = wordApp.Documents.Open(path);

                        // Cabecera
                        wordDoc.Bookmarks("Empresa").Range.InsertBefore(Empresa.Nombre);
                        wordDoc.Bookmarks("Sucursal").Range.InsertBefore(Sucursal.Nombre);
                        wordDoc.Bookmarks("Puesto").Range.InsertAfter(RegistroCaja.Autoimpresor.Nombre);
                        wordDoc.Bookmarks("Turno").Range.InsertAfter(RegistroCaja.Turno.Nombre);
                        wordDoc.Bookmarks("Fecha").Range.InsertAfter(RegistroCaja.Fecha.ToString("dd-MM-yyyy"));
                        wordDoc.Bookmarks("Usuario").Range.InsertAfter(RegistroCaja.Usuario);
                        wordDoc.Bookmarks("TipoCambio").Range.InsertAfter(RegistroCaja.TipoCambio);

                        // Facturas

                        Factura[] facturas = Facturas.ToArray();

                        for (int i = 0; i < facturas.Length; i++)
                        {
                            wordDoc.Tables[1].Rows.Add(wordDoc.Tables[1].Rows[i + 3]);
                            wordDoc.Tables[1].Cell(i + 3, 1).Range.Text = facturas[i].Nro;
                            wordDoc.Tables[1].Cell(i + 3, 2).Range.Text = facturas[i].FechaEmision.ToString("dd-MM-yyyy hh:mm:ss");
                            wordDoc.Tables[1].Cell(i + 3, 3).Range.Text = facturas[i].ClienteNombre;
                            wordDoc.Tables[1].Cell(i + 3, 4).Range.Text = facturas[i].ClienteNIT;
                            wordDoc.Tables[1].Cell(i + 3, 5).Range.Text = facturas[i].Subtotal.ToString("###,###,###.00");
                            wordDoc.Tables[1].Cell(i + 3, 6).Range.Text = facturas[i].Descuento.ToString("###,###,###.00");
                            wordDoc.Tables[1].Cell(i + 3, 7).Range.Text = facturas[i].Monto.ToString("###,###,###.00");
                            wordDoc.Tables[1].Cell(i + 3, 8).Range.Text = facturas[i].FacturaTipo.Descripcion;
                            wordDoc.Tables[1].Cell(i + 3, 9).Range.Text = facturas[i].Estado;
                        }

                        // Totales

                        wordDoc.Bookmarks("FacturasEmitidas").Range.InsertAfter(CantidadFacturas.ToString());
                        wordDoc.Bookmarks("FacturasAnuladas").Range.InsertAfter(FacturasAnuladas.ToString());
                        wordDoc.Bookmarks("TotalFacturadoBs").Range.InsertAfter(TotalFacturadoBS.ToString("###,###,###.00"));
                        wordDoc.Bookmarks("FacturadoContadoBs").Range.InsertAfter(FacturadoContadoBS.ToString("###,###,###.00"));
                        wordDoc.Bookmarks("FacturadoConAnticiposBs").Range.InsertAfter(FacturadoConAnticiposBS.ToString("###,###,###.00"));
                        wordDoc.Bookmarks("FacturadoPorCobrarBs").Range.InsertAfter(FacturadoPorCobrarBS.ToString("###,###,###.00"));
                        wordDoc.Bookmarks("FacturadoUS_CobradoUS_US").Range.InsertAfter(FacturadoUS.ToString("###,###,###.00"));
                        wordDoc.Bookmarks("FacturadoBs_CobradoUS_US").Range.InsertAfter(CobradoUS.ToString("###,###,###.00"));
                        wordDoc.Bookmarks("FacturadoUS_CobradoUS_Bs").Range.InsertAfter(FacturadoUS_EnBs.ToString("###,###,###.00"));
                        wordDoc.Bookmarks("FacturadoBs_CobradoUS_Bs").Range.InsertAfter(CobradoUS_EnBs.ToString("###,###,###.00"));
                        wordDoc.Bookmarks("FacturadoBs_CobradoBs_Bs").Range.InsertAfter(FacturadoBS.ToString("###,###,###.00"));
                        wordDoc.Bookmarks("TotalVentasBs").Range.InsertAfter(TotalVentasBS.ToString("###,###,###.00"));

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
        }

        #endregion

        #region Metodos Generados

        partial void ScreenDetalleFacturas_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();
            FindControls();
            Inits();
        }

        partial void Empresas_SelectionChanged()
        {
            EmpresaNombre = Empresas.SelectedItem != null && Empresas.Count == 1 ? Empresas.SelectedItem.Nombre : SoftCliente.ClienteNombre;
        }

        partial void Facturas_SelectionChanged()
        {
            if (Facturas.SelectedItem == null)
                return;

            groupFacturaDetalle.DisplayName = "Factura Nro. " + Facturas.SelectedItem.Dosificacion.NroAutorizacion + " - " + Facturas.SelectedItem.Nro;
        }

        partial void ImprimirFacturas_Execute()
        {
            PrintFacturas();
        }

        partial void VerFactura_Execute()
        {
            Application.ShowScreenFacturaDef(Facturas.SelectedItem.Dosificacion.NroAutorizacion,
                Facturas.SelectedItem.Nro, HabilitarAnularDesanular: false,
                HabilitarEdicion: true);
        }

        #endregion
    }
}
