using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace LightSwitchApplication
{
    public static class ServicioEMail
    {
        public static bool SendMail(string sendFrom, string sendTo, string subject, string body, int empresaId)
        {
            // Consigue datos de configuracion
            List<Parametro> pars;
            using (DataWorkspace dataWorkspace = new DataWorkspace())
            {
                pars = (from Parametro p in dataWorkspace.ApplicationData.Parametros
                        where p.Empresa.Id == empresaId && p.Categoria == "SMTP"
                        select p).ToList();
            }
            if (pars == null || pars.Count == 0)
                return false;

            // Verifica existencia de servidor de correo configurado
            Parametro parServer = pars.FirstOrDefault(p => p.Clave == "SERVER");
            string smtpServer = parServer != null ? parServer.Valor : null;
            if (smtpServer == null)
                return false;

            // Consigue parametros de configuracion SMTP
            Parametro parPort = pars.FirstOrDefault(p => p.Clave == "PORT");
            string smtpPort = parPort != null ? parPort.Valor : string.Empty;
            Parametro parEnableSSL = pars.FirstOrDefault(p => p.Clave == "ENABLE_SSL");
            bool smtpEnableSSL = parEnableSSL != null && parEnableSSL.Valor == "S";
            Parametro parDeliveryMethod = pars.FirstOrDefault(p => p.Clave == "DELIVERY_METHOD");
            string smtpDeliveryMethod = parDeliveryMethod != null ? parDeliveryMethod.Valor : string.Empty;
            Parametro parUserId = pars.FirstOrDefault(p => p.Clave == "USER_ID");
            string smtpUserId = parUserId != null ? parUserId.Valor : string.Empty;
            Parametro parPassword = pars.FirstOrDefault(p => p.Clave == "PASSWORD");
            string smtpPassword = parPassword != null ? parPassword.Valor : string.Empty;

            // Prepara correo
            MailAddress fromAddress = new MailAddress(sendFrom);
            MailAddress toAddress = new MailAddress(sendTo);
            MailMessage mail = new MailMessage();
            mail.From = fromAddress;
            mail.To.Add(toAddress);
            mail.Subject = subject;
            if (body.ToLower().Contains("<html>"))
                mail.IsBodyHtml = true;
            mail.Body = body;
            // SmtpClient smtp = new SmtpClient(smtpServer, smtpPort);
            SmtpClient smtp = new SmtpClient(smtpServer);
            if (smtpEnableSSL)
                smtp.EnableSsl = true;
            if (smtpDeliveryMethod == "Network")
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            //add credentials here if required
            smtp.Credentials = new NetworkCredential(smtpUserId, smtpPassword);

            // Envía correo
            try
            {
                smtp.Send(mail);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}