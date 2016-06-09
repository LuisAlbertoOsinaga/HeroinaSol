using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

using ImageTools;
using ImageTools.IO.Bmp;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Rendering;

namespace LightSwitchApplication
{
    public class QrGenerator
    {
        #region Propiedades

        public bool Generado { get; private set; }

        private string _fileBmp;
        public string FileBmp 
        {
            get { return _fileBmp != null ? _fileBmp : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\QR.bmp"; } 
            private set { _fileBmp = value; }
        }

        #endregion

        #region Constructor

        public QrGenerator(string fileBmp = null)
        {
            this.FileBmp = fileBmp;
            this.Generado = false;
        }

        #endregion

        #region Metodos
        public void GenerarFileBmp(string texto)
        {
            this.Generado = false;

            // Genera QR en un WriteableBitmap (con ZXing)
            BarcodeWriter writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions { Width = 400, Height = 400 }
            };
            WriteableBitmap wbitmap = writer.Write(texto);
            if (wbitmap == null)
            {
                this.Generado = true;
                return;
            }

            // Escritura archivo fileBmp (formato bmp) con QR (con ImageTools)
            ExtendedImage image = wbitmap.ToImage();
            FileStream stream = new FileStream(this.FileBmp, FileMode.Create);
            BmpEncoder encoder = new BmpEncoder();
            encoder.Encode(image, stream);

            // exito
            this.Generado = true;
        }
        public string GeneraTextoQr(string empresaNIT, string empresaNombre, string nroFactura, string nroAutorizacion,
                                    DateTime fechaEmision, decimal monto, string codigoControl, decimal ICE, decimal montoNoGravado,
                                    DateTime fechaLimite, string clienteNIT, string clienteNombre)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(!string.IsNullOrWhiteSpace(empresaNIT) ? empresaNIT : string.Empty);
            sb.Append("|");
            sb.Append(!string.IsNullOrWhiteSpace(empresaNombre) ? empresaNombre : string.Empty);
            sb.Append("|");
            sb.Append(!string.IsNullOrWhiteSpace(nroFactura) ? nroFactura : string.Empty);
            sb.Append("|");
            sb.Append(!string.IsNullOrWhiteSpace(nroAutorizacion) ? nroAutorizacion : string.Empty);
            sb.Append("|");
            string sFecha = fechaEmision.ToString("dd/MM/yyyy").Replace('-', '/'); 
            sb.Append(sFecha);
            sb.Append("|");
            decimal pesos = (decimal)Math.Floor((double)monto);
            string sMonto = pesos.ToString() + (monto - pesos).ToString(".00");
            sb.Append(sMonto);
            sb.Append("|");
            sb.Append(!string.IsNullOrWhiteSpace(codigoControl) ? codigoControl : string.Empty);
            sb.Append("|");
            sFecha = fechaLimite.ToString("dd/MM/yyyy").Replace('-', '/');
            sb.Append(sFecha);
            sb.Append("|");
            if (ICE == -1)
                sb.Append("0");
            else
            {
                decimal pesosICE = (decimal)Math.Floor((double)ICE);
                string sICE = pesosICE.ToString() + (ICE - pesos).ToString(".00");
                sb.Append(sICE);
            }
            sb.Append("|");
            if (montoNoGravado == -1)
                sb.Append("0");
            else
            {
                decimal pesosMNG = (decimal)Math.Floor((double)montoNoGravado);
                string sMNG = pesosMNG.ToString() + (montoNoGravado - pesosMNG).ToString(".00");
                sb.Append(sMNG);
            }
            sb.Append("|");
            sb.Append(!string.IsNullOrWhiteSpace(clienteNIT) ? clienteNIT : string.Empty);
            sb.Append("|");
            sb.Append(!string.IsNullOrWhiteSpace(clienteNombre) ? clienteNombre : string.Empty);

            return sb.ToString();
        }

        #endregion
    }
}