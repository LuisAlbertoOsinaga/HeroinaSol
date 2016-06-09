using System;
using System.Linq;

namespace LightSwitchApplication
{
    public static class ServicioDosificaciones
    {
        #region Métodos

        public static int GetFacturaSiguiente(int dosificacionId, out int facturaFinal, out int digitos, bool peek = false)
        {
            // Consigue secuencia
            using (DataWorkspace dw = new DataWorkspace())
            {
                Configuracion cfg = (from Configuracion c in dw.ApplicationData.Configuracions
                                     select c).FirstOrDefault();

                Dosificacion dosificacion = dw.ApplicationData.Dosificacions_Single(dosificacionId);
                if (dosificacion == null)
                    throw new ServicioDosificacionesException(string.Format("Error en ServicioDosificaciones.GetFacturaSiguiente. " +
                                                                        "Dosificación({0})." +
                                                                        " Registro no encontrado!", dosificacionId));

                if (dosificacion.FacturaSiguiente > dosificacion.FacturaFinal)
                    throw new ServicioDosificacionesException(string.Format("Error en ServicioDosificaciones.Get. " +
                                                                        "Dosificación({0})." +
                                                                    " nueva 'Factura Siguiente' ({2}) " + 
                                                                    "supera 'Factura Final' ({3})",
                                                        dosificacionId, dosificacion.FacturaSiguiente, 
                                                        dosificacion.FacturaFinal));

                facturaFinal = dosificacion.FacturaFinal;
                digitos = cfg != null && cfg.FacturaNroCerosAdelante ? dosificacion.Digitos : 0;
                int facturaSiguiente = dosificacion.FacturaSiguiente;
                if(peek)
                {
                    dosificacion.FacturaSiguiente++;
                    dw.ApplicationData.SaveChanges();
                }
                return facturaSiguiente;
            }
        }

        public static string GetFacturaSiguienteFormat(int dosificacionId, bool peek = false)
        {
            int facturaFinal;
            int digitos;
            return GetFacturaSiguiente(dosificacionId, out facturaFinal, out digitos, peek)
                        .ToString().PadLeft(digitos, '0');
        }

        public static int PeekFacturaSiguiente(int dosificacionId, out int facturaFinal, out int digitos)
        {
            return GetFacturaSiguiente(dosificacionId, out facturaFinal, out digitos, peek: true);
        }

        public static string PeekFacturaSiguienteFormat(int dosificacionId)
        {
            return GetFacturaSiguienteFormat(dosificacionId, peek: true);
        }

        #endregion
    }

    public static class ServicioDosificacionex
    {
        #region Métodos

        public static int GetFacturaSiguiente(DataWorkspace dw, int dosificacionId, out int facturaFinal, out int digitos, bool peek = false)
        {
            Configuracion cfg = (from Configuracion c in dw.ApplicationData.Configuracions
                                 select c).FirstOrDefault();

            // Consigue secuencia
                Dosificacion dosificacion = dw.ApplicationData.Dosificacions_Single(dosificacionId);
                if (dosificacion == null)
                    throw new ServicioDosificacionesException(string.Format("Error en ServicioDosificaciones.GetFacturaSiguiente. " +
                                                                        "Dosificación({0})." +
                                                                        " Registro no encontrado!", dosificacionId));

            // Fuera para dosificación por tiempo
                //if (dosificacion.FacturaSiguiente > dosificacion.FacturaFinal)
                //    throw new ServicioDosificacionesException(string.Format("Error en ServicioDosificaciones.Get. " +
                //                                                        "Dosificación({0})." +
                //                                                    " nueva 'Factura Siguiente' ({1}) " +
                //                                                    "supera 'Factura Final' ({2})",
                //                                        dosificacionId, dosificacion.FacturaSiguiente,
                //                                        dosificacion.FacturaFinal));

                facturaFinal = dosificacion.FacturaFinal;
                digitos = cfg != null && cfg.FacturaNroCerosAdelante ? dosificacion.Digitos : 0;
                int facturaSiguiente = dosificacion.FacturaSiguiente;
                if (peek)
                {
                    dosificacion.FacturaSiguiente++;
                }
                return facturaSiguiente;
        }

        public static string GetFacturaSiguienteFormat(DataWorkspace dw, int dosificacionId, bool peek = false)
        {
            int facturaFinal;
            int digitos;
            return GetFacturaSiguiente(dw, dosificacionId, out facturaFinal, out digitos, peek)
                        .ToString().PadLeft(digitos, '0');
        }

        public static int PeekFacturaSiguiente(DataWorkspace dw, int dosificacionId, out int facturaFinal, out int digitos)
        {
            return GetFacturaSiguiente(dw, dosificacionId, out facturaFinal, out digitos, peek: true);
        }

        public static string PeekFacturaSiguienteFormat(DataWorkspace dw, int dosificacionId)
        {
            return GetFacturaSiguienteFormat(dw, dosificacionId, peek: true);
        }

        #endregion
    }

    public class ServicioDosificacionesException : Exception
    {
        public ServicioDosificacionesException() : base() { }
        public ServicioDosificacionesException(string message) : base(message) { }
        public ServicioDosificacionesException(string message, Exception innerException) : base(message, innerException) { }
    }
}