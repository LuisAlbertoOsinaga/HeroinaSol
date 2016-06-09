using System;

namespace LightSwitchApplication
{
    public static class ServicioDosificaciones
    {
        #region Métodos

        public static int GetFacturaSiguiente(int dosificacionId, out int facturaFinal, bool peek = false)
        {
            // Consigue secuencia
            using (DataWorkspace dw = new DataWorkspace())
            {
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
            return GetFacturaSiguiente(dosificacionId, out facturaFinal, peek)
                        .ToString().PadLeft(facturaFinal.ToString().Length, '0');
        }

        public static int PeekFacturaSiguiente(int dosificacionId, out int facturaFinal)
        {
            return GetFacturaSiguiente(dosificacionId, out facturaFinal, peek: true);
        }

        public static string PeekFacturaSiguienteFormat(int dosificacionId)
        {
            return GetFacturaSiguienteFormat(dosificacionId, peek: true);
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