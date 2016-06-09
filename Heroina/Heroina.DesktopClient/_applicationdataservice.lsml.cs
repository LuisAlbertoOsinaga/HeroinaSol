using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.LightSwitch;
using Microsoft.LightSwitch.Security.Server;
using System.Linq.Expressions;

namespace LightSwitchApplication
{
    public partial class ApplicationDataService
    {
        #region AutoImpresores

        void AutoImpresoresChanging(AutoImpresor entity)
        {
            entity.RegId = entity.Sucursal.RegId + "_" + entity.NroAutoImpresor.ToString("000");
        }

        partial void AutoImpresores_Filter(ref Expression<Func<AutoImpresor, bool>> filter)
        {
            Configuracion cfg = DataWorkspace.ApplicationData.Configuracions.FirstOrDefault();
            filter = (x => x.Sucursal.Empresa.SoftCliente.SoftProductoId == cfg.SoftProductoId);
        }

        partial void AutoImpresores_Inserting(AutoImpresor entity)
        {
            AutoImpresoresChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void AutoImpresores_Updating(AutoImpresor entity)
        {
            AutoImpresoresChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region Dosificacions

        void DosificacionsChanging(Dosificacion entity)
        {
            entity.RegId = entity.AutoImpresor.RegId + "_" + entity.NroTramite;
        }

        partial void Dosificacions_Filter(ref Expression<Func<Dosificacion, bool>> filter)
        {
            Configuracion cfg = DataWorkspace.ApplicationData.Configuracions.FirstOrDefault();
            filter = (x => x.AutoImpresor.Sucursal.Empresa.SoftCliente.SoftProductoId == cfg.SoftProductoId);
        }

        partial void Dosificacions_Inserting(Dosificacion entity)
        {
            DosificacionsChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void Dosificacions_Updating(Dosificacion entity)
        {
            DosificacionsChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region EmpresaLogos

        void EmpresaLogosChanging(EmpresaLogo entity)
        {
            entity.RegId = entity.Empresa.RegId;
        }


        partial void EmpresaLogos_Filter(ref Expression<Func<EmpresaLogo, bool>> filter)
        {
            Configuracion cfg = DataWorkspace.ApplicationData.Configuracions.FirstOrDefault();
            filter = (x => x.Empresa.SoftCliente.SoftProductoId == cfg.SoftProductoId);
        }

        partial void EmpresaLogos_Inserting(EmpresaLogo entity)
        {
            EmpresaLogosChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void EmpresaLogos_Updating(EmpresaLogo entity)
        {
            EmpresaLogosChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region Empresas

        void EmpresasChanging(Empresa entity)
        {
            entity.RegId = entity.SoftCliente.SoftProductoId + "___" + entity.EmpresaId;
        }

        partial void Empresas_Filter(ref Expression<Func<Empresa, bool>> filter)
        {
            Configuracion cfg = DataWorkspace.ApplicationData.Configuracions.FirstOrDefault();
            filter = (x => x.SoftCliente.SoftProductoId == cfg.SoftProductoId);
        }

        partial void Empresas_Inserting(Empresa entity)
        {
            EmpresasChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void Empresas_Updating(Empresa entity)
        {
            EmpresasChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }
        
        #endregion

        #region FacturaDetalles

        void FacturaDetallesChanging(FacturaDetalle entity)
        {
            entity.RegId = entity.Factura.RegId + "_" + entity.NroLinea.ToString("000");
        }

        partial void FacturaDetalles_Filter(ref Expression<Func<FacturaDetalle, bool>> filter)
        {
            Configuracion cfg = DataWorkspace.ApplicationData.Configuracions.FirstOrDefault();
            filter = (x => x.Factura.Dosificacion.AutoImpresor.Sucursal.Empresa.SoftCliente.SoftProductoId == 
                            cfg.SoftProductoId);
        }

        partial void FacturaDetalles_Inserting(FacturaDetalle entity)
        {
            FacturaDetallesChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void FacturaDetalles_Updating(FacturaDetalle entity)
        {
            FacturaDetallesChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region FacturaHospedajes

        void FacturaHospedajesChanging(FacturaHospedaje entity)
        {
            entity.RegId = entity.Factura.RegId;
        }

        partial void FacturaHospedajes_Filter(ref Expression<Func<FacturaHospedaje, bool>> filter)
        {
            Configuracion cfg = DataWorkspace.ApplicationData.Configuracions.FirstOrDefault();
            filter = (x => x.Factura.Dosificacion.AutoImpresor.Sucursal.Empresa.SoftCliente.SoftProductoId == 
                                cfg.SoftProductoId);
        }

        partial void FacturaHospedajes_Inserting(FacturaHospedaje entity)
        {
            FacturaHospedajesChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void FacturaHospedajes_Updating(FacturaHospedaje entity)
        {
            FacturaHospedajesChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region FacturaTipoDetalles

        void FacturaTipoDetallesChanging(FacturaTipoDetalle entity)
        {
            entity.RegId = entity.SoftCliente.RegId + "___" + entity.Codigo;
        }

        partial void FacturaTipoDetalles_Filter(ref Expression<Func<FacturaTipoDetalle, bool>> filter)
        {
            Configuracion cfg = DataWorkspace.ApplicationData.Configuracions.FirstOrDefault();
            filter = (x => x.SoftCliente.SoftProductoId == cfg.SoftProductoId);
        }

        partial void FacturaTipoDetalles_Inserting(FacturaTipoDetalle entity)
        {
            FacturaTipoDetallesChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void FacturaTipoDetalles_Updating(FacturaTipoDetalle entity)
        {
            FacturaTipoDetallesChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region FacturaTipos

        void FacturaTiposChanging(FacturaTipo entity)
        {
            entity.RegId = entity.SoftCliente.RegId + "___" + entity.Codigo;
        }

        partial void FacturaTipos_Filter(ref Expression<Func<FacturaTipo, bool>> filter)
        {
            Configuracion cfg = DataWorkspace.ApplicationData.Configuracions.FirstOrDefault();
            filter = (x => x.SoftCliente.SoftProductoId == cfg.SoftProductoId);
        }

        partial void FacturaTipos_Inserting(FacturaTipo entity)
        {
            FacturaTiposChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void FacturaTipos_Updating(FacturaTipo entity)
        {
            FacturaTiposChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region Facturas

        void FacturasChanging(Factura entity)
        {
            entity.RegId = entity.Dosificacion.RegId + "_" + entity.Nro;
        }

        partial void Facturas_Filter(ref Expression<Func<Factura, bool>> filter)
        {
            Configuracion cfg = DataWorkspace.ApplicationData.Configuracions.FirstOrDefault();
            filter = (x => x.Dosificacion.AutoImpresor.Sucursal.Empresa.SoftCliente.SoftProductoId == cfg.SoftProductoId);
        }

        partial void Facturas_Inserting(Factura entity)
        {
            FacturasChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void Facturas_Updating(Factura entity)
        {
            FacturasChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region Facturas_QRs

        void Factura_QRsChanging(Factura_QR entity)
        {
            entity.RegId = entity.Factura.RegId;
        }

        partial void Factura_QRs_Filter(ref Expression<Func<Factura_QR, bool>> filter)
        {
            Configuracion cfg = DataWorkspace.ApplicationData.Configuracions.FirstOrDefault();
            filter = (x => x.Factura.Dosificacion.AutoImpresor.Sucursal.Empresa.SoftCliente.SoftProductoId == cfg.SoftProductoId);
        }

        partial void Factura_QRs_Inserting(Factura_QR entity)
        {
            Factura_QRsChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void Factura_QRs_Updating(Factura_QR entity)
        {
            Factura_QRsChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region Metodos Comunes
        partial void Query_ExecuteFailed(QueryExecuteFailedDescriptor queryDescriptor)
        {
            ApplicationException ex = new ApplicationException("ApplicationDataService Query_ExecuteFailed!", queryDescriptor.Error);
            ex.Data.Add("Name", queryDescriptor.Name);
            ex.Data.Add("ToString", queryDescriptor.ToString());
            throw ex;
        }

        #endregion

        #region Secuencias

        void SecuenciasChanging(Secuencia entity)
        {
            entity.RegId = entity.Sucursal.RegId + "_" + entity.Clase + "_" + entity.Nombre;
        }

        partial void Secuencias_Filter(ref Expression<Func<Secuencia, bool>> filter)
        {
            Configuracion cfg = DataWorkspace.ApplicationData.Configuracions.FirstOrDefault();
            filter = (x => x.Sucursal.Empresa.SoftCliente.SoftProductoId == cfg.SoftProductoId);
        }

        partial void Secuencias_Inserting(Secuencia entity)
        {
            SecuenciasChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void Secuencias_Updating(Secuencia entity)
        {
            SecuenciasChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region SoftClientes

        void SoftClientesChanging(SoftCliente entity)
        {
            entity.SoftProductoId = entity.SoftProducto + "_" + entity.ClienteId;
            entity.RegId = entity.SoftProductoId;
        }

        partial void SoftClientes_Inserting(SoftCliente entity)
        {
            SoftClientesChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void SoftClientes_Updating(SoftCliente entity)
        {
            SoftClientesChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region Sucursals

        void SucursalsChanging(Sucursal entity)
        {
            entity.RegId = entity.Empresa.RegId + "_" + entity.SucursalNro.ToString("000");
        }

        partial void Sucursals_Filter(ref Expression<Func<Sucursal, bool>> filter)
        {
            Configuracion cfg = DataWorkspace.ApplicationData.Configuracions.FirstOrDefault();
            filter = (x => x.Empresa.SoftCliente.SoftProductoId == cfg.SoftProductoId);
        }

        partial void Sucursals_Inserting(Sucursal entity)
        {
            SucursalsChanging(entity);


            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void Sucursals_Updating(Sucursal entity)
        {
            SucursalsChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion
    }

    public class ServicioSecuenciaException : Exception
    {
        public ServicioSecuenciaException() : base() { }
        public ServicioSecuenciaException(string message) : base(message) { }
        public ServicioSecuenciaException(string message, Exception innerException) : base(message, innerException) { }
    }

}
