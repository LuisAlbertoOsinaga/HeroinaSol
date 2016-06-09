using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ServicesClient
{
    public static class ServicioFacturacion
    {
        public static string GetInstalacionId()
        {
            dynamic path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "heroina";
            if (!File.Exists(path))
                return string.Empty;
            return File.ReadAllText(path);
        }

        public static string MontoALiteral(decimal monto)
        {
            NumeroLiteral nl = new NumeroLiteral();
            int pesos = (int) Math.Floor((double) monto);
            int centavos = (int) Math.Floor( (double) (monto - pesos) * 100 );
            string literal = nl.MostrarLiteral(pesos);
            literal = !string.IsNullOrWhiteSpace(literal) ? literal.Substring(0, 1).ToUpper() + literal.Remove(0, 1) : literal;
            return "Son: " + literal + " " + centavos.ToString("00") + "/100";
        }
    }

    class NumeroLiteral
    {
        private Dictionary<int, string> diccionarioLiterales = new Dictionary<int, string>();
        private StringBuilder cadenaLiteral;

        public NumeroLiteral()
        {
            this.InicializarLiterales();
        }

        /// <summary>
        /// Retorna un valor NUMÉRICO convertido a LITERAL.
        /// </summary>
        /// <param name="numero">Valor numérico a ser procesado</param>
        /// <returns></returns>
        public string MostrarLiteral(int numero)
        {
            cadenaLiteral = new StringBuilder();

            this.GenerarCadenaLiteral(numero);

            return cadenaLiteral.ToString().Trim();
        }

        private void InicializarLiterales()
        {
            diccionarioLiterales.Add(1, "un");
            diccionarioLiterales.Add(2, "dos");
            diccionarioLiterales.Add(3, "tres");
            diccionarioLiterales.Add(4, "cuatro");
            diccionarioLiterales.Add(5, "cinco");
            diccionarioLiterales.Add(6, "seis");
            diccionarioLiterales.Add(7, "siete");
            diccionarioLiterales.Add(8, "ocho");
            diccionarioLiterales.Add(9, "nueve");
            diccionarioLiterales.Add(10, "diez");
            diccionarioLiterales.Add(11, "once");
            diccionarioLiterales.Add(12, "doce");
            diccionarioLiterales.Add(13, "trece");
            diccionarioLiterales.Add(14, "catorce");
            diccionarioLiterales.Add(15, "quince");
            diccionarioLiterales.Add(16, "dieciséis");
            diccionarioLiterales.Add(17, "diecisiete");
            diccionarioLiterales.Add(18, "dieciocho");
            diccionarioLiterales.Add(19, "diecinueve");
            diccionarioLiterales.Add(20, "veinte");
            diccionarioLiterales.Add(21, "veintiún");
            diccionarioLiterales.Add(22, "veintidós");
            diccionarioLiterales.Add(23, "veintitrés");
            diccionarioLiterales.Add(24, "veinticuatro");
            diccionarioLiterales.Add(25, "veinticinco");
            diccionarioLiterales.Add(26, "veintiséis");
            diccionarioLiterales.Add(27, "veintisiete");
            diccionarioLiterales.Add(28, "veintiocho");
            diccionarioLiterales.Add(29, "veintinueve");
            diccionarioLiterales.Add(30, "treinta");
            diccionarioLiterales.Add(40, "cuarenta");
            diccionarioLiterales.Add(50, "cincuenta");
            diccionarioLiterales.Add(60, "sesenta");
            diccionarioLiterales.Add(70, "setenta");
            diccionarioLiterales.Add(80, "ochenta");
            diccionarioLiterales.Add(90, "noventa");
            diccionarioLiterales.Add(100, "ciento"); //cien
            diccionarioLiterales.Add(200, "doscientos");
            diccionarioLiterales.Add(300, "trescientos");
            diccionarioLiterales.Add(400, "cuatrocientos");
            diccionarioLiterales.Add(500, "quinientos");
            diccionarioLiterales.Add(600, "seiscientos");
            diccionarioLiterales.Add(700, "setecientos");
            diccionarioLiterales.Add(800, "ochocientos");
            diccionarioLiterales.Add(900, "novecientos");
        }

        private void GenerarCadenaLiteral(int numero)
        {
            if (numero == 0)
            {
                cadenaLiteral.Append("cero");
            }
            else if (numero < 1000)
            {
                this.ConvertirNumero(numero, 1000);
            }
            else if (numero < 1000000)
            {
                this.ConvertirNumero(numero / 1000, 1000);
                cadenaLiteral.Append("mil ");
                this.ConvertirNumero(numero % 1000, 1000);
            }
            else if (numero < 1000000000)
            {
                this.ConvertirNumero(numero / 1000000, 1000);
                if (numero < 2000000)
                    cadenaLiteral.Append("millón ");
                else
                    cadenaLiteral.Append("millones ");
                this.ConvertirNumero((numero % 1000000) / 1000, 1000);
                cadenaLiteral.Append("mil ");
                this.ConvertirNumero((numero % 1000000) % 1000, 1000);
            }
        }

        private void ConvertirNumero(int numero, int multiplicador)
        {
            if (diccionarioLiterales.ContainsKey(numero))
            {
                if (numero == 100)
                    cadenaLiteral.Append("cien ");
                else
                    cadenaLiteral.Append(diccionarioLiterales[numero] + " ");
            }
            else
            {
                if (multiplicador > 0)
                {
                    int div = numero / multiplicador;
                    int mod = numero % multiplicador;
                    int lectura = div * multiplicador;

                    if (diccionarioLiterales.ContainsKey(lectura))
                    {
                        if ((lectura / 100) == 0)
                            cadenaLiteral.Append(diccionarioLiterales[lectura] + " y ");
                        else
                            cadenaLiteral.Append(diccionarioLiterales[lectura] + " ");
                    }

                    ConvertirNumero(mod, multiplicador / 10);
                }
            }
        }
    }
}
