using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.Automation;

namespace LightSwitchApplication
{
    public static class Utilidades
    {
        static string SoftProductoNombre = "CashFlow";

        public static string GetTemplateResumenCaja()
        {
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

            return path;
        }

        public static string  GetVersion()
        {
            string version = "1.0.0";

            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly == null)
                return version;

            string fullName = assembly.FullName;
            if (string.IsNullOrWhiteSpace(fullName))
                return version;

            int startParcialVersion = fullName.IndexOf("Version=") + 8;
            if (startParcialVersion < 8)
                return version;

            string parcialVersion = fullName.Substring(startParcialVersion);
            if (string.IsNullOrWhiteSpace(parcialVersion))
                return version;

            int endParcialVersion = parcialVersion.IndexOf(",");
            if (endParcialVersion <= 1)
                return version;

            parcialVersion = parcialVersion.Substring(0, endParcialVersion + 1);
            if (string.IsNullOrWhiteSpace(parcialVersion))
                return version;

            endParcialVersion = parcialVersion.IndexOf(".0,");
            version = endParcialVersion < 0 ? parcialVersion : parcialVersion.Remove(endParcialVersion);
            return version;
        }

        public static bool IsTimeStampEqual(byte[] timestamp1, byte[] timestamp2)
        {
            if (timestamp1 == null || timestamp2 == null)
                return false;

            if (timestamp1.Length != timestamp2.Length)
                return false;

            for (int i = 0; i < timestamp1.Length; i++)
            {
                if (timestamp1[i] != timestamp2[i])
                    return false;
            }

            return true;
        }

        public static void PrintResumenCaja(RegistroCaja regCaja, bool paraCierreCaja = false)
        {
            if (!AutomationFactory.IsAvailable)
            {
                throw new Exception("No está habilitada la Automatización Word!");
            }

            try
            {
                // Imprimir a través de word
                dynamic wordApp = null;
                dynamic wordDoc = null;
                try
                {
                    // Preparacion
                    string template = Utilidades.GetTemplateResumenCaja();
                    wordApp = AutomationFactory.CreateObject("Word.Application");
                    wordDoc = wordApp.Documents.Open(template);

                    // Llenado Datos
                    wordDoc.Bookmarks("Empresa").Range.InsertBefore(regCaja.Autoimpresor.Sucursal.Empresa.Nombre);
                    wordDoc.Bookmarks("Sucursal").Range.InsertBefore(regCaja.Autoimpresor.Sucursal.Nombre);
                    wordDoc.Bookmarks("Puesto").Range.InsertAfter(regCaja.Autoimpresor.Nombre);
                    wordDoc.Bookmarks("Turno").Range.InsertAfter(regCaja.Turno.Nombre);
                    wordDoc.Bookmarks("Fecha").Range.InsertAfter(regCaja.Fecha.ToString("dd-MM-yyyy"));
                    wordDoc.Bookmarks("Usuario").Range.InsertAfter(regCaja.Usuario);
                    wordDoc.Bookmarks("TipoCambio").Range.InsertAfter(regCaja.TipoCambio);
                    wordDoc.Bookmarks("VentasContado").Range.InsertAfter(regCaja.FacturadoContadoBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("VentasConAnticipos").Range.InsertAfter(regCaja.FacturadoConAnticiposBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("VentasPorCobrar").Range.InsertAfter(regCaja.FacturadoXCobrarBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("TotalVentas").Range.InsertAfter(regCaja.TotalFacturadoBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("FacturasEmitidas").Range.InsertAfter(regCaja.CantidadFacturas.ToString());
                    wordDoc.Bookmarks("FacturasAnuladas").Range.InsertAfter(regCaja.FacturasAnuladas.ToString());
                    wordDoc.Bookmarks("PrimeraFactura").Range.InsertAfter(string.Format("{0} ({1})", regCaja.FacturaInicialNro,
                                                                                                    regCaja.FacturaInicialFecha.ToString("dd-MM-yyyy hh:mm:ss")));
                    wordDoc.Bookmarks("UltimaFactura").Range.InsertAfter(string.Format("{0} ({1})", regCaja.FacturaFinalNro,
                                                                                                    regCaja.FacturaFinalFecha.ToString("dd-MM-yyyy hh:mm:ss")));
                    wordDoc.Bookmarks("RecaudacionContado").Range.InsertAfter(regCaja.FacturadoContadoBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("RecaudacionCobranzas").Range.InsertAfter(regCaja.TotalCobranzasBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("RecaudacionAnticipos").Range.InsertAfter(regCaja.TotalAnticiposBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("TotalRecaudacion").Range.InsertAfter(regCaja.TotalRecaudacionBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("EfectivoBS").Range.InsertAfter(regCaja.EfectivoBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("EfectivoUS").Range.InsertAfter(regCaja.EfectivoUS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("ChequesPropiosBS").Range.InsertAfter(regCaja.ChequesPropiosBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("ChequesPropiosUS").Range.InsertAfter(regCaja.ChequesPropiosUS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("ChequesAjenosBS").Range.InsertAfter(regCaja.ChequesAjenosBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("ChequesAjenosUS").Range.InsertAfter(regCaja.ChequesAjenosUS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("TarjetasCreditoBS").Range.InsertAfter(regCaja.TarjetasBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("TarjetasCreditoUS").Range.InsertAfter(regCaja.TarjetasUS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("DepositosBS").Range.InsertAfter(regCaja.DepositosBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("DepositosUS").Range.InsertAfter(regCaja.DepositosUS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("RecaudacionBS").Range.InsertAfter(regCaja.RecaudacionBS.ToString("###,###,###.00"));
                    wordDoc.Bookmarks("RecaudacionUS").Range.InsertAfter(regCaja.RecaudacionUS.ToString("###,###,###.00"));
                    if (paraCierreCaja)
                    {
                        string CierreCaja = string.Format("Registro: {0} - Apertura: {1} - Cierre: {2}",
                                            regCaja.Id.ToString(),
                                            regCaja.HoraInicio.ToString("dd/MM/yyyy - hh:mm:ss"),
                                            regCaja.HoraFinal.ToString("dd/MM/yyyy - hh:mm:ss"));
                        string codigoControl = Math.Abs((CierreCaja + regCaja.Autoimpresor.Sucursal.Empresa.Nombre).GetHashCode()).ToString();
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

        public static void SendMail(string from, string to, string subject, string body, DateTime fecha, Empresa empresa, ApplicationData appData)
        {
            try
            {
                SendEMail mail = appData.SendEMails.AddNew();
                mail.Empresa = empresa;
                mail.Fecha = fecha;
                mail.Random = Guid.NewGuid();
                mail.MailFrom = from;
                mail.MailTo = to;
                mail.Subject = subject;
                mail.Body = body;
                appData.SaveChanges();
            }
            catch (Exception)
            {
            }
        }

        public static void SendMails(string from, string rolMail, string subject, string body, DateTime fecha, Empresa empresa, ApplicationData appData)
        {
            try
            {
                List<RolEmail> mails = (from RolEmail r in appData.RolEmails
                                        where r.Rol == rolMail
                                        select r).ToList();
                if (mails == null || mails.Count == 0)
                    return;

                foreach (var mail in mails)
                    SendMail(from, mail.Mail, subject, body, fecha, empresa, appData);
            }
            catch (Exception)
            {
            }
        }

        public static string SoftProducto(SoftCliente softCliente)
        {

            return  SoftProductoNombre + " " + softCliente.SoftProductoModelo + " " + 
                            GetVersion() + " ";
        }
    }
}
