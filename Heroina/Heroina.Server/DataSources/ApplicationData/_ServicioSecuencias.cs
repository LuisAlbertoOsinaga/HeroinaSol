using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightSwitchApplication.App_Code
{
    public class ServicioSecuencias
    {
        #region Propiedades

        public string Clase { get; private set; }
        public int Digitos { get; private set; }
        public string Nombre { get; private set; }

        #endregion

        #region Constructor

        public ServicioSecuencias(string clase, string nombre)
        {
            if (string.IsNullOrEmpty(clase))
                throw new ServicioSecuenciasException("Error en ctor() de ServicioSecuencia. 'clase' no puede ser nulo o blanco");
            if (string.IsNullOrEmpty(nombre))
                throw new ServicioSecuenciasException("Error en ctor() de ServicioSecuencia. 'nombre' no puede ser nulo o blanco");

            this.Clase = clase;
            this.Nombre = nombre;
        }

        #endregion

        #region Métodos

        public int Get()
        {
            // Consigue secuencia
            using (DataWorkspace dw = new DataWorkspace())
            {
                Secuencia secuencia = (from Secuencia sec in dw.ApplicationData.Secuencias
                                       where sec.Clase == this.Clase && sec.Nombre == this.Nombre
                                       select sec).FirstOrDefault();
                if (secuencia == null)
                    throw new ServicioSecuenciasException(string.Format("Error en ServicioSecuencia.Get. " +
                                                                        "Clase({0}), SecuenciaNombre({1})." +
                                                                        " Registro no encontrado!",
                                                            this.Clase, this.Nombre));

                if (secuencia.NroSiguiente > secuencia.NroFinal)
                    throw new ServicioSecuenciasException(string.Format("Error en ServicioSecuencia.Get. " +
                                                                        "Clase({0}), SecuenciaNombre({1})." +
                                                                    " nuevo 'Número Siguiente' ({2}) " + 
                                                                    "supera 'Número Final' ({3})",
                                                        this.Clase, this.Nombre, secuencia.NroSiguiente, secuencia.NroFinal));

                Digitos = secuencia.NroFinal.ToString().Length;
                return secuencia.NroSiguiente;
            }
        }
   
        public string GetFormat()
        {
            return Get().ToString().PadLeft(this.Digitos, '0');
        }
        
        public void InitSecuencia(int nroInicial, int nroFinal, int nroSiguiente = -1)
        {
            // Validacion
            if (nroInicial < 1)
                throw new ServicioSecuenciasException(string.Format("Error en ServicioSecuencia.InitSecuencia. Clase({0}), " + 
                                                                    "Nombre({1})." +
                                                                    " 'NroInicial' ({2}) tiene que ser mayor que cero",
                                                        this.Clase, this.Nombre, nroInicial));
            if (nroFinal <= nroInicial)
                throw new ServicioSecuenciasException(string.Format("Error en ServicioSecuencia.InitSecuencia. Clase({0}), " + 
                                                                    "Nombre({1})." +
                                                                    " 'NroFinal' ({2}) tiene que ser mayor que " + 
                                                                    "'Número Inicial' ({3})",
                                                        this.Clase, this.Nombre, nroFinal, nroInicial));
            if (nroSiguiente != -1 && nroSiguiente < nroInicial)
                throw new ServicioSecuenciasException(string.Format("Error en ServicioSecuencia.InitSecuencia. Clase({0}), " + 
                                                                    "Nombre({1})." +
                                                                    " 'NroSiguiente' ({2}) no puede ser menor que " + 
                                                                    "'NroInicial' ({3})",
                                                        this.Clase, this.Nombre, nroSiguiente, nroInicial));

            // Inicializa secuencia
            using (DataWorkspace dw = new DataWorkspace())
            {
                Secuencia secuencia = (from Secuencia sec in dw.ApplicationData.Secuencias
                                       where sec.Clase == this.Clase && sec.Nombre == this.Nombre
                                       select sec).FirstOrDefault();
                if (secuencia == null)
                    secuencia = dw.ApplicationData.Secuencias.AddNew();

                secuencia.Clase = this.Clase;
                secuencia.Nombre = this.Nombre;
                secuencia.NroInicial = nroInicial;
                secuencia.NroFinal = nroFinal;
                secuencia.NroSiguiente = nroSiguiente == -1 ? nroInicial : nroSiguiente;

                dw.ApplicationData.SaveChanges();
            }
        }
        
        public int Peek()
        {
            // Consigue secuencia
            using (DataWorkspace dw = new DataWorkspace())
            {
                Secuencia secuencia = (from Secuencia sec in dw.ApplicationData.Secuencias
                                       where sec.Clase == this.Clase && sec.Nombre == this.Nombre
                                       select sec).FirstOrDefault();
                if (secuencia == null)
                    throw new ServicioSecuenciasException(string.Format("Error en ServicioSecuencia.Peek. " +
                                                                        "Clase({0}), SecuenciaNombre({1})." +
                                                                        " Registro no encontrado!",
                                                            this.Clase, this.Nombre));

                if (secuencia.NroSiguiente > secuencia.NroFinal)
                    throw new ServicioSecuenciasException(string.Format("Error en ServicioSecuencia.Peek. " +
                                                                        "Clase({0}), Nombre({1})." +
                                                                    " nuevo 'NroSiguiente' ({2}) supera 'NroFinal' ({3})",
                                                        this.Clase, this.Nombre, secuencia.NroSiguiente, 
                                                        secuencia.NroFinal));

                int numeroActual = secuencia.NroSiguiente;
                Digitos = secuencia.NroFinal.ToString().Length;
                secuencia.NroSiguiente++;
                dw.ApplicationData.SaveChanges();
                return numeroActual;
            }
        }
        
        public string PeekFormat()
        {
            return Peek().ToString().PadLeft(this.Digitos, '0');
        }

        #endregion
    }

    public class ServicioSecuenciasException : Exception
    {
        public ServicioSecuenciasException() : base() { }
        public ServicioSecuenciasException(string message) : base(message) { }
        public ServicioSecuenciasException(string message, Exception innerException) : base(message, innerException) { }
    }
}