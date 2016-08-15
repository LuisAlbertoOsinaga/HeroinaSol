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
        public static bool ValidateFactura = true;

        #region ActividadEconomicas

        void ActividadEconomicasChanging(ActividadEconomica entity)
        {
            entity.RegId = entity.Empresa.RegId + "_" + entity.Nro.ToString("000");
        }

        partial void ActividadEconomicas_Filter(ref Expression<Func<ActividadEconomica, bool>> filter)
        {
            filter = (x => x.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
        }

        partial void ActividadEconomicas_Inserting(ActividadEconomica entity)
        {
            ActividadEconomicasChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void ActividadEconomicas_Updating(ActividadEconomica entity)
        {
            ActividadEconomicasChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region Autoimpresores

        void AutoimpresoresChanging(Autoimpresor entity)
        {
            entity.RegId = entity.Sucursal.RegId + "_" + entity.NroAutoImpresor.ToString("000");

            if (!string.IsNullOrWhiteSpace(entity.Nombre))
                entity.Nombre = entity.Nombre.Trim().ToUpper();
        }

        partial void Autoimpresores_Filter(ref Expression<Func<Autoimpresor, bool>> filter)
        {
            filter = (x => x.Sucursal.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
        }

        partial void Autoimpresores_Inserting(Autoimpresor entity)
        {
            AutoimpresoresChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void Autoimpresores_Updating(Autoimpresor entity)
        {
            AutoimpresoresChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region CausaAnulaciones

        void CausaAnulacionesChanging(CausaAnulacion entity)
        {
            if (!string.IsNullOrWhiteSpace(entity.Codigo))
                entity.Codigo = entity.Codigo.Trim().ToUpper();
            if (!string.IsNullOrWhiteSpace(entity.Descripcion))
                entity.Descripcion = entity.Descripcion.Trim().ToUpper();
            entity.RegId = entity.Empresa.SoftCliente.RegId + "_" + entity.Codigo;
        }

        partial void CausaAnulaciones_Filter(ref Expression<Func<CausaAnulacion, bool>> filter)
        {
            filter = (x => x.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
        }

        partial void CausaAnulaciones_Inserting(CausaAnulacion entity)
        {
            CausaAnulacionesChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void CausaAnulaciones_Updating(CausaAnulacion entity)
        {
            CausaAnulacionesChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }
        
        #endregion

        #region Clientes

        void ClientesChanging(Cliente entity)
        {
            entity.RegId = entity.Empresa.SoftCliente.RegId + "_" + entity.NIT;
            if (!string.IsNullOrWhiteSpace(entity.RazonSocial))
                entity.RazonSocial = entity.RazonSocial.Trim().ToUpper();
        }

        partial void Clientes_Filter(ref Expression<Func<Cliente, bool>> filter)
        {
            filter = (x => x.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
        }

        partial void Clientes_Inserting(Cliente entity)
        {
            ClientesChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void Clientes_Updating(Cliente entity)
        {
            ClientesChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region ClienteOperacions

        void ClienteOperacionsChanching(ClienteOperacion entity)
        {
            entity.RegId = entity.Cliente.Empresa.SoftCliente.RegId + "_" + Guid.NewGuid().ToString("N");
        }

        partial void ClienteOperacions_Filter(ref Expression<Func<ClienteOperacion, bool>> filter)
        {
            filter = (x => x.Cliente.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
        }

        partial void ClienteOperacions_Inserting(ClienteOperacion entity)
        {
            ClienteOperacionsChanching(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void ClienteOperacions_Updating(ClienteOperacion entity)
        {
            ClienteOperacionsChanching(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region Configuracions

        void ConfiguracionsChanging(Configuracion entity)
        {
            entity.RegId = entity.InstalacionId;
        }

        partial void Configuracions_Inserting(Configuracion entity)
        {
            ConfiguracionsChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void Configuracions_Updating(Configuracion entity)
        {
            ConfiguracionsChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region Dosificacions

        void DosificacionsChanging(Dosificacion entity)
        {
            entity.RegId = entity.Autoimpresor.RegId + "_" + entity.NroAutorizacion;
        }

        partial void Dosificacions_Filter(ref Expression<Func<Dosificacion, bool>> filter)
        {
            filter = (x => x.Autoimpresor.Sucursal.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
        }

        partial void Dosificacions_Inserting(Dosificacion entity)
        {
            DosificacionsChanging(entity);

            entity.Activa = true;
            entity.Digitos = 0;
            entity.FacturaAlarma = 0;
            entity.FacturaSiguiente = entity.FacturaInicial;
            entity.FechaUltimaFactura = new DateTime(2000, 1, 1);
            entity.SinCreditoFiscal = false;

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

        #region Empresas

        void EmpresasChanging(Empresa entity)
        {
            entity.RegId = entity.SoftCliente.SoftProductoId + "___" + entity.EmpresaId;

            ActualizaDefiniciones(entity);
        }

        partial void Empresas_Filter(ref Expression<Func<Empresa, bool>> filter)
        {
            filter = (x => x.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
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

        partial void Empresas_Validate(Empresa entity, EntitySetValidationResultsBuilder results)
        {
            SoftCliente sfc = (from s in DataWorkspace.ApplicationData.SoftClientes
                               where entity.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId
                               select s).FirstOrDefault();
            if (sfc == null)
            {
                results.AddEntityError("No se encontró registro SoftCliente para esta empresa!");
                return;
            }

            if (entity.Id == 0 && Empresas.Count() == sfc.NroEmpresas)
            {
                results.AddEntityError("Configuración no permite adicionar más empresas!");
                return;
            }
        }

        #endregion

        #region FacturaDetalles

        void FacturaDetallesChanging(FacturaDetalle entity)
        {
            entity.RegId = entity.Factura.RegId + "_" + entity.NroLinea.ToString("000");
        }

        partial void FacturaDetalles_Filter(ref Expression<Func<FacturaDetalle, bool>> filter)
        {
            filter = (x => x.Factura.Dosificacion.Autoimpresor.Sucursal.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
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

        partial void FacturaDetalles_Validate(FacturaDetalle entity, EntitySetValidationResultsBuilder results)
        {
            if (entity.Importe <= 0)
                results.AddPropertyError("Importe debe ser mayor a cero!", entity.Details.Properties.Importe);
        }

        #endregion

        #region FacturaHospedajes

        void FacturaHospedajesChanging(FacturaHospedaje entity)
        {
            entity.RegId = entity.Factura.RegId;
        }

        partial void FacturaHospedajes_Filter(ref Expression<Func<FacturaHospedaje, bool>> filter)
        {
            filter = (x => x.Factura.Dosificacion.Autoimpresor.Sucursal.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
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
            entity.RegId = entity.Empresa.RegId + "_" + entity.Codigo;

            if (!string.IsNullOrWhiteSpace(entity.Codigo))
                entity.Codigo = entity.Codigo.Trim().ToUpper();
            if (!string.IsNullOrWhiteSpace(entity.Descripcion))
                entity.Descripcion = entity.Descripcion.Trim().ToUpper();
        }

        partial void FacturaTipoDetalles_Filter(ref Expression<Func<FacturaTipoDetalle, bool>> filter)
        {
            filter = (x => x.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
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
            entity.RegId = entity.Empresa.RegId + "___" + entity.Codigo;

            if (!string.IsNullOrWhiteSpace(entity.Codigo))
                entity.Codigo = entity.Codigo.Trim().ToUpper();
            if (!string.IsNullOrWhiteSpace(entity.Descripcion))
                entity.Descripcion = entity.Descripcion.Trim().ToUpper();
        }

        partial void FacturaTipos_Filter(ref Expression<Func<FacturaTipo, bool>> filter)
        {
            filter = (x => x.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
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
            Configuracion cfg = DataWorkspace.ApplicationData.Configuracions.FirstOrDefault();

            // Tipo Venta
            if (entity.Monto == entity.MontoContado)
                entity.TipoVenta = "CON";   // Contado
            else if (entity.Monto == entity.MontoPorCobrar)
                entity.TipoVenta = "CRE";   // Por Cobrar
            else if (entity.Monto == entity.MontoAnticipado)
                entity.TipoVenta = "ANT";
            else
                entity.TipoVenta = "COM";

            // Impuestos
            entity.Excento = entity.FacturaTipo.SinCreditoFiscal ? entity.Monto : 0M;
            entity.Neto = entity.Monto - entity.ICE - entity.Excento;
            entity.DebitoFiscal = entity.Neto * cfg.TasaIVA;
        }

        partial void Facturas_Filter(ref Expression<Func<Factura, bool>> filter)
        {
            filter = (x => x.Dosificacion.Autoimpresor.Sucursal.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
        }

        partial void Facturas_Inserting(Factura entity)
        {
            FacturasChanging(entity);
            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;

            entity.CodigoControl = Gn.SrvFany.ServiciosFacturacion.CodigoControl(entity.Dosificacion.LlaveDosificacion,
                                                                                        entity.Dosificacion.NroAutorizacion,
                                                                                        entity.Nro,
                                                                                        entity.ClienteNIT,
                                                                                        entity.FechaEmision,
                                                                                        entity.Monto);
            entity.Estado = "V"; // Vigente
            entity.Situacion = "E"; // Emitida

            Configuracion cfg = DataWorkspace.ApplicationData.Configuracions.FirstOrDefault();

            // Dosificación
            entity.Dosificacion.FechaUltimaFactura = entity.FechaEmision;
            entity.Nro = ServicioDosificacionex.PeekFacturaSiguienteFormat(DataWorkspace, entity.Dosificacion.Id);

            // Cliente
            Cliente cliente = (from Cliente cli in DataWorkspace.ApplicationData.Clientes
                                   where cli.Empresa == entity.Dosificacion.Autoimpresor.Sucursal.Empresa && cli.NIT == entity.ClienteNIT
                                   select cli).FirstOrDefault();
            if (cliente == null)
            {
                cliente = DataWorkspace.ApplicationData.Clientes.AddNew();
                cliente.Empresa = entity.Dosificacion.Autoimpresor.Sucursal.Empresa;
                cliente.NIT = entity.ClienteNIT;
                cliente.RazonSocial = entity.ClienteNombre;
            }
            else
                cliente.RazonSocial = entity.ClienteNombre;
            
            // Situacion
            entity.Situacion = entity.MontoPorCobrar == 0 ? "P" : "E";      // P-Pagada E-Emitida

            // Por Cobrar
            if(entity.MontoPorCobrar > 0M)
            {
                Cliente cliXCobrar = null;
                if (entity.ClienteXCobrarNIT == entity.ClienteNIT)
                    cliXCobrar = cliente;
                else
                    cliXCobrar = (from Cliente cli in DataWorkspace.ApplicationData.Clientes
                                            where cli.Empresa.Id == entity.Dosificacion.Autoimpresor.Sucursal.Empresa.Id && cli.NIT == entity.ClienteXCobrarNIT
                                            select cli).FirstOrDefault();

                ClienteOperacion cliOp = DataWorkspace.ApplicationData.ClienteOperacions.AddNew();
                cliOp.Cliente = cliXCobrar;
                cliOp.Referencia = entity.Nro;
                cliOp.NroAutorizacion = entity.Dosificacion.NroAutorizacion;
                cliOp.FacturaNro = entity.Nro;
                cliOp.Fecha = entity.FechaEmision;
                cliOp.MedioPagoBS = "EF";
                cliOp.MedioPagoUS = "EF";
                cliOp.Monto = entity.MontoPorCobrar;
                cliOp.MontoBS = entity.MontoPorCobrar;
                cliOp.MontoUS = 0M;
                cliOp.TipoCambio = cfg != null ? cfg.TipoCambio : 1;
                cliOp.TipoOperacion = "OC";
                cliOp.Estado = "V";
            }

            // Anticipos
            if(entity.MontoAnticipado > 0M)
            {
                string[] opNros = entity.AnticiposOpNros.Split('-');
                foreach (var op in opNros)
                {
                    ClienteOperacion cop = (from ClienteOperacion c in DataWorkspace.ApplicationData.ClienteOperacions
                                            where c.Id.ToString() == op
                                            select c).FirstOrDefault();
                    cop.Estado = "C";
                    cop.NroAutorizacion = entity.Dosificacion.NroAutorizacion;
                    cop.FacturaNro = entity.Nro;

                    ClienteOperacion cliOp = DataWorkspace.ApplicationData.ClienteOperacions.AddNew();
                    cliOp.Cliente = cop.Cliente;
                    cliOp.Referencia = cop.Referencia;
                    cliOp.NroAutorizacion = cop.NroAutorizacion;
                    cliOp.FacturaNro = cop.FacturaNro;
                    cliOp.Fecha = entity.FechaEmision;
                    cliOp.MedioPagoBS = cop.MedioPagoBS;
                    cliOp.MedioPagoUS = cop.MedioPagoUS;
                    cliOp.Monto = cop.Monto;
                    cliOp.MontoBS = cop.MontoBS;
                    cliOp.MontoUS = cop.MontoUS;
                    cliOp.TipoCambio = cop.TipoCambio;
                    cliOp.TipoOperacion = "CA";
                    cliOp.Estado = "V";

                    cop.OperacionesRelacionadas.Add(cliOp);
                }
            }
        }

        partial void Facturas_Updating(Factura entity)
        {
            FacturasChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;

            Configuracion cfg = DataWorkspace.ApplicationData.Configuracions.FirstOrDefault();

            // Por Cobrar, Cancelacion anterior
            if (entity.Details.Properties.MontoPorCobrar.OriginalValue > 0M && 
                entity.Details.Properties.ClienteXCobrarNIT.OriginalValue != null &&
                 entity.Details.Properties.ClienteXCobrarNIT.OriginalValue != entity.ClienteXCobrarNIT)
            {
                // Cancela por cobrar anterior

                Cliente cliXCobrar = (from Cliente cli in DataWorkspace.ApplicationData.Clientes
                                  where cli.Empresa.Id == entity.Dosificacion.Autoimpresor.Sucursal.Empresa.Id &&
                                            cli.NIT == entity.Details.Properties.ClienteXCobrarNIT.OriginalValue
                                  select cli).FirstOrDefault();

                ClienteOperacion cliOp = (from ClienteOperacion co in DataWorkspace.ApplicationData.ClienteOperacions
                                              where co.Cliente.Id == cliXCobrar.Id && 
                                              co.NroAutorizacion == entity.Dosificacion.NroAutorizacion && co.FacturaNro == entity.Nro
                                              select co).FirstOrDefault();
                cliOp.Estado = "C";
            }

            // Por Cobrar nuevo
            if (entity.MontoPorCobrar > 0M)
            {
                Cliente cliXCobrar = (from Cliente cli in DataWorkspace.ApplicationData.Clientes
                                  where cli.Empresa.Id == entity.Dosificacion.Autoimpresor.Sucursal.Empresa.Id && cli.NIT == entity.ClienteXCobrarNIT
                                  select cli).FirstOrDefault();

                ClienteOperacion cliOp = (from ClienteOperacion co in DataWorkspace.ApplicationData.ClienteOperacions
                                          where co.Cliente.Id == cliXCobrar.Id &&
                                          co.NroAutorizacion == entity.Dosificacion.NroAutorizacion && co.FacturaNro == entity.Nro
                                          select co).FirstOrDefault();
                if (cliOp != null)
                {
                    cliOp.Monto = entity.MontoPorCobrar;
                    cliOp.MontoBS = entity.MontoPorCobrar;
                    cliOp.MontoUS = 0M;
                }
                else
                {
                    cliOp = DataWorkspace.ApplicationData.ClienteOperacions.AddNew();
                    cliOp.Cliente = cliXCobrar;
                    cliOp.Referencia = entity.Nro;
                    cliOp.NroAutorizacion = entity.Dosificacion.NroAutorizacion;
                    cliOp.FacturaNro = entity.Nro;
                    cliOp.Fecha = entity.FechaEmision;
                    cliOp.MedioPagoBS = "EF";
                    cliOp.MedioPagoUS = "EF";
                    cliOp.Monto = entity.MontoPorCobrar;
                    cliOp.MontoBS = entity.MontoPorCobrar;
                    cliOp.MontoUS = 0M;
                    cliOp.TipoCambio = cfg != null ? cfg.TipoCambio : 1;
                    cliOp.TipoOperacion = "OC";
                    cliOp.Estado = "V";
                }
            }

            // Anticipos, Cancelacion anteriores
            if(entity.Details.Properties.MontoAnticipado.OriginalValue > 0M && 
                !string.IsNullOrWhiteSpace(entity.Details.Properties.AnticiposOpNros.OriginalValue))
            {
                if (entity.Details.Properties.AnticiposOpNros.OriginalValue != entity.AnticiposOpNros)
                {
                    string[] opNros = entity.Details.Properties.AnticiposOpNros.OriginalValue.Split('-');
                    foreach (var op in opNros)
                    {
                        ClienteOperacion cop = (from ClienteOperacion c in DataWorkspace.ApplicationData.ClienteOperacions
                                                where c.Id.ToString() == op
                                                select c).FirstOrDefault();
                        if(cop != null)
                        {
                            cop.Estado = "V";
                            ClienteOperacion cliOp = cop.OperacionesRelacionadas.FirstOrDefault();
                            if (cliOp != null)
                                cliOp.Estado = "C";
                        }
                    }
                }
            }

            // Anticipos nuevos
            if (entity.MontoAnticipado > 0M)
            {
                string[] opNros = entity.AnticiposOpNros.Split('-');
                foreach (var op in opNros)
                {
                    ClienteOperacion cop = (from ClienteOperacion c in DataWorkspace.ApplicationData.ClienteOperacions
                                            where c.Id.ToString() == op
                                            select c).FirstOrDefault();
                    cop.Estado = "C";
                    cop.NroAutorizacion = entity.Dosificacion.NroAutorizacion;
                    cop.FacturaNro = entity.Nro;

                    ClienteOperacion cliOp = DataWorkspace.ApplicationData.ClienteOperacions.AddNew();
                    cliOp.Cliente = cop.Cliente;
                    cliOp.Referencia = cop.Referencia;
                    cliOp.NroAutorizacion = cop.NroAutorizacion;
                    cliOp.FacturaNro = cop.FacturaNro;
                    cliOp.Fecha = entity.FechaEmision;
                    cliOp.MedioPagoBS = cop.MedioPagoBS;
                    cliOp.MedioPagoUS = cop.MedioPagoUS;
                    cliOp.Monto = cop.Monto;
                    cliOp.MontoBS = cop.MontoBS;
                    cliOp.MontoUS = cop.MontoUS;
                    cliOp.TipoCambio = cop.TipoCambio;
                    cliOp.TipoOperacion = "CA";
                    cliOp.Estado = "V";

                    cop.OperacionesRelacionadas.Add(cliOp);
                }
            }
        }

        partial void Facturas_Validate(Factura entity, EntitySetValidationResultsBuilder results)
        {
            if (!ValidateFactura)
                return;

            if (entity.FacturaDetalles == null || entity.FacturaDetalles.Count() == 0)
                results.AddPropertyError("Factura no tiene detalle!", entity.Details.Properties.FacturaDetalles);
            if (entity.Subtotal <= 0)
                results.AddPropertyError("Subtotal debe ser mayor a cero!", entity.Details.Properties.Subtotal);
            if(entity.Monto <= 0)
                results.AddPropertyError("Monto debe ser mayor a cero!", entity.Details.Properties.Monto);

            if (entity.FacturaTipo.Codigo == "HOT")
            {
                if((from d in entity.FacturaDetalles where d.TipoDetalle.Codigo == "HOS" select d).Count() > 1)
                    results.AddPropertyError("Factura tiene más de un detalle de tipo 'HOSPEDAJE'!", entity.Details.Properties.FacturaDetalles);
                if ((from d in entity.FacturaDetalles where d.TipoDetalle.Codigo == "RES" select d).Count() > 1)
                    results.AddPropertyError("Factura tiene más de un detalle de tipo 'RESTAURANT'!", entity.Details.Properties.FacturaDetalles);
                if ((from d in entity.FacturaDetalles where d.TipoDetalle.Codigo == "LAV" select d).Count() > 1)
                    results.AddPropertyError("Factura tiene más de un detalle de tipo 'LAVANDERIA'!", entity.Details.Properties.FacturaDetalles);
                if ((from d in entity.FacturaDetalles where d.TipoDetalle.Codigo == "CON" select d).Count() > 1)
                    results.AddPropertyError("Factura tiene más de un detalle de tipo 'CONFERENCIAS'!", entity.Details.Properties.FacturaDetalles);
                if ((from d in entity.FacturaDetalles where d.TipoDetalle.Codigo == "OTR" || d.TipoDetalle.Codigo == "REG" select d).Count() > 1)
                    results.AddPropertyError("Factura tiene más de un detalle de tipo 'OTROS' o 'REGULAR'!", entity.Details.Properties.FacturaDetalles);
            }

            if (entity.FacturaTipo.Codigo == "TUR")
            {
                if (entity.FacturaTipo.Codigo == "TUR" && !(entity.ClienteNIT == "0") && !(entity.ClienteNombre == "SIN NOMBRE"))
                    results.AddPropertyError("Factura TURÍSTICA debe tener '0' en NIT/CI y en nombre del Cliente 'SIN NOMBRE'!", entity.Details.Properties.FacturaDetalles);
                if ((from d in entity.FacturaDetalles where d.TipoDetalle.Codigo == "HOS" select d).Count() > 1)
                    results.AddPropertyError("Factura tiene más de un detalle de tipo 'HOSPEDAJE'!", entity.Details.Properties.FacturaDetalles);
                if ((from d in entity.FacturaDetalles where d.TipoDetalle.Codigo != "HOS" select d).Count() > 0)
                    results.AddPropertyError("Factura tiene detalles no de tipo 'HOSPEDAJE'!", entity.Details.Properties.FacturaDetalles);
            }

            // Monto a pagar
            if (entity.Monto != entity.MontoContado + entity.MontoPorCobrar + entity.MontoAnticipado)
                results.AddPropertyError("La suma de contado, por cobrar y anticipos no es igual al monto de la Factura!", entity.Details.Properties.MontoContado);

            // Por Cobrar
            if(entity.MontoPorCobrar > 0M)
            {
                if (entity.ClienteXCobrarNIT == entity.ClienteNIT && entity.ClienteXCobrarNombre == entity.ClienteNombre)
                    return;

                Cliente cliXCobrar = (from Cliente c in DataWorkspace.ApplicationData.ClientesXEmpresa(entity.Dosificacion.Autoimpresor.Sucursal.Empresa.Id)
                                      where c.NIT == entity.ClienteXCobrarNIT
                                      select c).FirstOrDefault();
                if(cliXCobrar == null)
                    results.AddPropertyError("Cliente Por Cobrar no encontrado en Clientes!", entity.Details.Properties.ClienteXCobrarNIT);
                if(cliXCobrar.RazonSocial != entity.ClienteXCobrarNombre)
                    results.AddPropertyError("Nombre de Cliente Por Cobrar no corresponde a su NIT!", entity.Details.Properties.ClienteXCobrarNIT);
            }

            // Anticipos
            if(!string.IsNullOrWhiteSpace(entity.AnticiposOpNros))
            {
                string[] opNros = entity.AnticiposOpNros.Split('-');
                decimal totalAnticipos = 0M;
                foreach (var op in opNros)
                {
                    ClienteOperacion cop = (from ClienteOperacion c in DataWorkspace.ApplicationData.ClienteOperacions
                                            where c.Id.ToString() == op
                                            select c).FirstOrDefault();
                    if (cop == null)
                    {
                        results.AddPropertyError(string.Format("Anticipo OpNro '{0}' no encontrado en Anticipos!", op), entity.Details.Properties.AnticiposOpNros);
                    }
                    else
                        totalAnticipos += cop.Monto;
                }
                if(entity.MontoAnticipado != totalAnticipos)
                    results.AddPropertyError("Monto de Anticipos adicionados a la Factura no es igual al de las operaciones!", 
                                                entity.Details.Properties.ClienteXCobrarNIT);
            }
        }

        #endregion

        #region FormaPagos

        void FormaPagosChanging(FormaPago entity)
        {
            entity.RegId = entity.Empresa.RegId + "___" + entity.Concepto + "_" + entity.Codigo;
        }

        partial void FormaPagos_Filter(ref Expression<Func<FormaPago, bool>> filter)
        {
            filter = (x => x.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
        }

        partial void FormaPagos_Inserting(FormaPago entity)
        {
            FormaPagosChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void FormaPagos_Updating(FormaPago entity)
        {
            FormaPagosChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }
        
        #endregion

        #region MedioPagos


        void MedioPagosChanging(MedioPago entity)
        {
            entity.RegId = entity.Empresa.RegId + "___" + entity.Codigo;

            if (!string.IsNullOrWhiteSpace(entity.Codigo))
                entity.Codigo = entity.Codigo.Trim().ToUpper();
            if (!string.IsNullOrWhiteSpace(entity.Descripcion))
                entity.Descripcion = entity.Descripcion.Trim().ToUpper();
        }

        partial void MedioPagos_Filter(ref Expression<Func<MedioPago, bool>> filter)
        {
            filter = (x => x.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
        }

        partial void MedioPagos_Inserting(MedioPago entity)
        {
            MedioPagosChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void MedioPagos_Updating(MedioPago entity)
        {
            MedioPagosChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region Parametros

        void ParametrosChanging(Parametro entity)
        {
            entity.RegId = entity.Empresa.RegId + "_" + entity.Categoria + "_" + entity.Clave;
        }

        partial void Parametros_Filter(ref Expression<Func<Parametro, bool>> filter)
        {
            filter = x => x.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId;
        }

        partial void Parametros_Inserting(Parametro entity)
        {
            ParametrosChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void Parametros_Updating(Parametro entity)
        {
            ParametrosChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region Procesos

        void ProcesosChanging(Proceso entity)
        {
            entity.RegId = entity.Empresa.RegId + "___" + Guid.NewGuid().ToString("N");
        }

        partial void Procesos_Filter(ref Expression<Func<Proceso, bool>> filter)
        {
            filter = (x => x.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
        }

        partial void Procesos_Inserting(Proceso entity)
        {
            ProcesosChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;

            ProcesoEjecucion(entity);
        }

        partial void Procesos_Inserted(Proceso entity)
        {
        }

        partial void Procesos_Updating(Proceso entity)
        {
            ProcesosChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        partial void Procesos_Updated(Proceso entity)
        {
            entity.FechaFinal = DateTime.Now;
        }

        #endregion

        #region RegistroCajas

        void RegistroCajasChanging(RegistroCaja entity)
        {
            entity.RegId = entity.Autoimpresor.RegId + "____" + entity.Fecha.ToString("dd/MM/yyyy") + "_" + entity.Turno.Codigo;
        }

        partial void RegistroCajas_Filter(ref Expression<Func<RegistroCaja, bool>> filter)
        {
            filter = (x => x.Autoimpresor.Sucursal.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
        }

        partial void RegistroCajas_Inserting(RegistroCaja entity)
        {
            RegistroCajasChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void RegistroCajas_Updating(RegistroCaja entity)
        {
            RegistroCajasChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;

            if (entity.CajaAbierta == false)
                ActualizaRegistroCaja(entity);
        }

        #endregion

        #region ResumenCajas

        void ResumenCajasChanging(ResumenCaja entity)
        {
            entity.RegId = entity.Empresa.RegId + "_" + entity.Guid;
        }

        partial void ResumenCajas_Filter(ref Expression<Func<ResumenCaja, bool>> filter)
        {
            filter = x => x.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId;
        }

        partial void ResumenCajas_Inserting(ResumenCaja entity)
        {
            ResumenCajasChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void ResumenCajas_Updating(ResumenCaja entity)
        {
            ResumenCajasChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region RolEmails

        void RolEmailsChanging(RolEmail entity)
        {
            entity.RegId = entity.Empresa.RegId + "_" + entity.Rol + "_" + entity.Mail;
            entity.Rol = entity.Rol.Trim().ToUpper();
        }

        partial void RolEmails_Filter(ref Expression<Func<RolEmail, bool>> filter)
        {
            filter = x => x.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId;
        }

        partial void RolEmails_Inserting(RolEmail entity)
        {
            RolEmailsChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void RolEmails_Updating(RolEmail entity)
        {
            RolEmailsChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region Secuencias

        void SecuenciasChanging(Secuencia entity)
        {
            entity.RegId = entity.Sucursal.RegId + "_" + entity.Clase + "_" + entity.Nombre;
        }

        partial void Secuencias_Filter(ref Expression<Func<Secuencia, bool>> filter)
        {
            filter = (x => x.Sucursal.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
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

        #region SendMails

        void SendMailsChanging(SendEMail entity)
        {
            entity.RegId = entity.Empresa.RegId + "_" + entity.Random;
        }

        partial void SendEMails_Filter(ref Expression<Func<SendEMail, bool>> filter)
        {
            filter = x => x.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId;
        }

        partial void SendEMails_Inserting(SendEMail entity)
        {
            SendMailsChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;

            if (string.IsNullOrWhiteSpace(entity.MailFrom))
            {
                Parametro par = (from Parametro p in Parametros
                                    where p.Empresa.Id == entity.Empresa.Id && p.Categoria == "SMTP" && p.Clave == "FROM"
                                    select p).FirstOrDefault();

                entity.MailFrom = par != null && !string.IsNullOrEmpty(par.Valor) ? par.Valor : "info@gnesios.com";
            }
            ServicioEMail.SendMail(entity.MailFrom, entity.MailTo, entity.Subject, entity.Body, entity.Empresa.Id);
        }

        partial void SendEMails_Updating(SendEMail entity)
        {
            SendMailsChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion

        #region SoftClientes

        void SoftClientesChanging(SoftCliente entity)
        {
            entity.SoftProductoId = entity.SoftProductoCodeName + "___" + entity.ClienteId;
            entity.RegId = entity.SoftProductoId;

            ActualizaConfiguracionInicial(entity);
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
            filter = (x => x.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
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

        #region Turnos

        void TurnosChanging(Turno entity)
        {
            entity.RegId = entity.Empresa.SoftCliente.RegId + "_" + entity.Codigo;

            if (!string.IsNullOrWhiteSpace(entity.Codigo))
                entity.Codigo = entity.Codigo.Trim().ToUpper();
            if (!string.IsNullOrWhiteSpace(entity.Nombre))
                entity.Nombre = entity.Nombre.Trim().ToUpper();
        }

        partial void Turnos_Filter(ref Expression<Func<Turno, bool>> filter)
        {
            filter = (x => x.Empresa.SoftCliente.SoftProductoId == LightSwitch.SoftCliente.SoftProductoId);
        }

        partial void Turnos_Inserting(Turno entity)
        {
            TurnosChanging(entity);

            entity.CreadoPor = Application.User.Name;
            entity.CreadoEn = DateTime.Now;
        }

        partial void Turnos_Updating(Turno entity)
        {
            TurnosChanging(entity);

            entity.ActualizadoPor = Application.User.Name;
            entity.ActualizadoEn = DateTime.Now;
        }

        #endregion




        #region Metodos Auxiliares

        private void ProcesoEjecucion(Proceso proceso)
        {
            if (proceso.Descripcion == "RESUMEN CAJA")
                ProcesosResumenCaja(proceso);
            else if (proceso.Descripcion == "RESUMEN DIARIO DE CAJA")
                ProcesosResumenCajaDiario(proceso);

            //else if (proceso.Descripcion.Substring(0, 6) == "A HERO")
            //    ProcesoMigracioAHero(proceso);
            //else if (proceso.Descripcion.Substring(0, 7) == "DE HERO")
            //    ProcesoMigracioDeHero(proceso);
        }

        //private void ProcesoMigracioAHero(Proceso proceso)
        //{
        //    // datos de proceso inicio
        //    proceso.FechaInicio = DateTime.Now;
        //    proceso.Finalizado = false;

        //    if (proceso.Descripcion == "A HERO")
        //        Migracion.AHero(proceso);
        //    else if (proceso.Descripcion == "A HEROACTIVIDADECONOMICA")
        //        Migracion.AHeroActividadEconomica(proceso);
        //    else if (proceso.Descripcion == "A HEROAUTOIMPRESOR")
        //        Migracion.AHeroAutoimpresor(proceso);
        //    else if (proceso.Descripcion == "A HEROCAUSAANULACION")
        //        Migracion.AHeroCausaAnulacion(proceso);
        //    else if (proceso.Descripcion == "A HEROCLIENTE")
        //        Migracion.AHeroCliente(proceso);
        //    else if (proceso.Descripcion == "A HEROCLIENTEOPERACION")
        //        Migracion.AHeroClienteOperacion(proceso);
        //    else if (proceso.Descripcion == "A HEROCONFIGURACION")
        //        Migracion.AHeroConfiguracion(proceso);
        //    else if (proceso.Descripcion == "A HERODOSIFICACION")
        //        Migracion.AHeroDosificacion(proceso);
        //    else if (proceso.Descripcion == "A HEROEMPRESA")
        //        Migracion.AHeroEmpresa(proceso);
        //    else if (proceso.Descripcion == "A HEROFACTURA")
        //        Migracion.AHeroFactura(proceso);
        //    else if (proceso.Descripcion == "A HEROFACTURADETALLE")
        //        Migracion.AHeroFacturaDetalle(proceso);
        //    else if (proceso.Descripcion == "A HEROFACTURAHOSPEDAJE")
        //        Migracion.AHeroFacturaHospedaje(proceso);
        //    else if (proceso.Descripcion == "A HEROFACTURATIPO")
        //        Migracion.AHeroFacturaTipo(proceso);
        //    else if (proceso.Descripcion == "A HEROFACTURATIPODETALLE")
        //        Migracion.AHeroFacturaTipoDetalle(proceso);
        //    else if (proceso.Descripcion == "A HEROMEDIOPAGO")
        //        Migracion.AHeroMedioPago(proceso);
        //    else if (proceso.Descripcion == "A HEROREGISTROCAJA")
        //        Migracion.AHeroRegistroCaja(proceso);
        //    else if (proceso.Descripcion == "A HEROSOFTCLIENTE")
        //        Migracion.AHeroSoftCliente(proceso);
        //    else if (proceso.Descripcion == "A HEROSUCURSAL")
        //        Migracion.AHeroSucursal(proceso);
        //    else if (proceso.Descripcion == "A HEROTURNO")
        //        Migracion.AHeroTurno(proceso);

        //    // datos de proceso final
        //    proceso.FechaFinal = DateTime.Now;
        //    proceso.Finalizado = true;
        //}

        //private void ProcesoMigracioDeHero(Proceso proceso)
        //{
        //    // datos de proceso inicio
        //    proceso.FechaInicio = DateTime.Now;
        //    proceso.Finalizado = false;

        //    if (proceso.Descripcion == "DE HERO")
        //        Migracion.DeHero(proceso);
        //    else if (proceso.Descripcion == "DE HEROACTIVIDADECONOMICA")
        //        Migracion.DeHeroActividadEconomica(proceso);
        //    else if (proceso.Descripcion == "DE HEROAUTOIMPRESOR")
        //        Migracion.DeHeroAutoimpresor(proceso);
        //    else if (proceso.Descripcion == "DE HEROCAUSAANULACION")
        //        Migracion.DeHeroCausaAnulacion(proceso);
        //    else if (proceso.Descripcion == "DE HEROCLIENTE")
        //        Migracion.DeHeroCliente(proceso);
        //    else if (proceso.Descripcion == "DE HEROCLIENTEOPERACION")
        //        Migracion.DeHeroClienteOperacion(proceso);
        //    else if (proceso.Descripcion == "DE HEROCONFIGURACION")
        //        Migracion.DeHeroConfiguracion(proceso);
        //    else if (proceso.Descripcion == "DE HERODOSIFICACION")
        //        Migracion.DeHeroDosificacion(proceso);
        //    else if (proceso.Descripcion == "DE HEROEMPRESA")
        //        Migracion.DeHeroEmpresa(proceso);
        //    else if (proceso.Descripcion == "DE HEROFACTURA")
        //        Migracion.DeHeroFactura(proceso);
        //    else if (proceso.Descripcion == "DE HEROFACTURADETALLE")
        //        Migracion.DeHeroFacturaDetalle(proceso);
        //    else if (proceso.Descripcion == "DE HEROFACTURAHOSPEDAJE")
        //        Migracion.DeHeroFacturaHospedaje(proceso);
        //    else if (proceso.Descripcion == "DE HEROFACTURATIPO")
        //        Migracion.DeHeroFacturaTipo(proceso);
        //    else if (proceso.Descripcion == "DE HEROFACTURATIPODETALLE")
        //        Migracion.DeHeroFacturaTipoDetalle(proceso);
        //    else if (proceso.Descripcion == "DE HEROMEDIOPAGO")
        //        Migracion.DeHeroMedioPago(proceso);
        //    else if (proceso.Descripcion == "DE HEROREGISTROCAJA")
        //        Migracion.DeHeroRegistroCaja(proceso);
        //    else if (proceso.Descripcion == "DE HEROSOFTCLIENTE")
        //        Migracion.DeHeroSoftCliente(proceso);
        //    else if (proceso.Descripcion == "DE HEROSUCURSAL")
        //        Migracion.DeHeroSucursal(proceso);
        //    else if (proceso.Descripcion == "DE HEROTURNO")
        //        Migracion.DeHeroTurno(proceso);

        //    // datos de proceso final
        //    proceso.FechaFinal = DateTime.Now;
        //    proceso.Finalizado = true;
        //}

        private void ProcesosResumenCaja(Proceso entity)
        {
            entity.Finalizado = false;
            entity.FechaInicio = DateTime.Now;
            int regCajaId = int.Parse(entity.Data);
            RegistroCaja registroCaja = RegistroCajas_SingleOrDefault(regCajaId);

            ActualizaRegistroCaja(registroCaja);

            entity.FechaFinal = DateTime.Now;
            entity.Finalizado = true;
        }

        private void ProcesosResumenCajaDiario(Proceso entity)
        {
            // datos de proceso inicio
            entity.FechaInicio = DateTime.Now;
            entity.Finalizado = false;

            // Separa partes de la data
            string[] partes = entity.Data.Split('|');

            // Consigue Empresa
            string empresaNombre = partes[0];

            // Consigue Organizacion Scope y Id
            string organizacionScope = partes[1];
            int organizacionId = int.Parse(partes[2]);

            // Consigue fecha para el Resumen
            string[] partesFecha = partes[3].Split('/', '-');
            int day = int.Parse(partesFecha[0]);
            int month = int.Parse(partesFecha[1]);
            int year = int.Parse(partesFecha[2]);
            DateTime fechaResumen = new DateTime(year, month, day);

            // Crea e inicializa registro Resumen Caja para el día
            ResumenCaja resumenCaja = ResumenCajas.AddNew();
            resumenCaja.Empresa = (from e in Empresas
                                   where e.Nombre == empresaNombre
                                   select e).FirstOrDefault();
            resumenCaja.Guid = Guid.Parse(partes[4]);
            resumenCaja.OrganizacionScope = organizacionScope;
            resumenCaja.OrganizacionId = organizacionId;
            resumenCaja.Rango = "D";
            resumenCaja.FechaInicial = fechaResumen;
            resumenCaja.FechaFinal = fechaResumen;

            // Consigue registros caja del día
            List<RegistroCaja> regsCajas;
            if (resumenCaja.OrganizacionScope == "P")    // Puesto o Autoimpresor
            {
                regsCajas = (from RegistroCaja reg in RegistroCajas
                             where reg.Autoimpresor.Id == organizacionId && reg.Fecha == fechaResumen
                             select reg).ToList();
            }
            else if (resumenCaja.OrganizacionScope == "S")   // Sucursal
            {
                regsCajas = (from RegistroCaja reg in RegistroCajas
                             where reg.Autoimpresor.Sucursal.Id == organizacionId && reg.Fecha == fechaResumen
                             select reg).ToList();
            }
            else    // Empresa
            {
                regsCajas = (from RegistroCaja reg in RegistroCajas
                             where reg.Autoimpresor.Sucursal.Empresa.Id == organizacionId && reg.Fecha == fechaResumen
                             select reg).ToList();
                foreach (RegistroCaja regCaja in regsCajas)
                    ActualizaRegistroCaja(regCaja);
            }

            // Sumariza registros
            resumenCaja.FacturasEmitidas = regsCajas.Sum(r => r.CantidadFacturas);
            resumenCaja.FacturasAnuladas = regsCajas.Sum(r => r.FacturasAnuladas);
            resumenCaja.FacturadoContado = regsCajas.Sum(r => r.FacturadoContadoBS);
            resumenCaja.FacturadoXCobrar = regsCajas.Sum(r => r.FacturadoXCobrarBS);
            resumenCaja.FacturadoConAnticipos = regsCajas.Sum(r => r.FacturadoConAnticiposBS);
            resumenCaja.RecibidoXCobranzas = regsCajas.Sum(r => r.TotalCobranzasBS);
            resumenCaja.RecibidoXAnticipos = regsCajas.Sum(r => r.TotalAnticiposBS);
            resumenCaja.RecibidoConTarjeta = regsCajas.Sum(r => r.TarjetasBS);
            resumenCaja.TasaComisionTarjeta = (from Configuracion cfg in Configuracions where cfg.Empresa == empresaNombre select cfg).First().ComisionTarjeta;
            resumenCaja.IngresosNetos = resumenCaja.RecibidoTotal - Math.Round(resumenCaja.RecibidoConTarjeta * resumenCaja.TasaComisionTarjeta, 2);

            // datos de proceso final
            entity.FechaFinal = DateTime.Now;
            entity.Finalizado = true;
        }

        #endregion

        #region Metodos Comunes

        private void ActualizaConfiguracionInicial(SoftCliente entity)
        {
            if (Configuracions.Count() == 0)
            {
                Configuracion config = new Configuracion();
                config.InstalacionId = "instalacion_01";
                config.FacturacionImprimeOriginal = true;
                config.FacturacionNroCopias = 1;
                config.Leyenda = "ESTA FACTURA CONTRIBUYE AL DESARROLLO DEL PAÍS. EL USO ÍLICITO DE ÉSTA SERÁ SANCIONADO DE ACUERDO A LA LEY";
                config.ConceptoRestaurante = "Consumo";
                config.ConceptoLavanderia = "Prendas varias";
                config.ConceptoConferencias = "Llamadas varias";
                config.FileTemplateAlquiler = "Factura_ALQ002.dotx";
                config.FileTemplateComercial = "Factura_COM002.dotx";
                config.FileTemplateHotelera = "Factura_HOT002.dotx";
                config.FileTemplateServicios = "Factura_SER002.dotx";
                config.FileTemplateTuristica = "Factura_TUR002.dotx";
                config.TasaIVA = 0.13M;
                config.TasaIT = 0.03M;
                config.TipoCambio = 6.85M;
                config.TipoCambioOficial = 6.96M;
                config.ComisionTarjeta = 0.03M;
                config.ImprimeQR = true;
            }
        }

        private void ActualizaDefiniciones(Empresa entity)
        {
            // Actualiza sucursal 0 : Casa Matriz

            Sucursal matriz = (from s in Sucursals
                               where s.Empresa.Id == entity.Id && s.SucursalNro == 0
                               select s).SingleOrDefault();
            if (matriz == null)
                matriz = Sucursals.AddNew();

            matriz.Empresa = entity;
            matriz.Calle = entity.Calle;
            matriz.Ciudad = entity.Ciudad;
            matriz.Departamento = entity.Departamento;
            matriz.Domicilio = entity.Domicilio;
            matriz.Municipio = entity.Municipio;
            matriz.Nombre = "CASA MATRIZ";
            matriz.NombreImpuestos = "CASA MATRIZ";
            matriz.Pais = entity.Pais;
            matriz.SucursalNro = 0;
            matriz.Telefonos = entity.Telefonos;
            matriz.Ubicacion = entity.Ubicacion;
            matriz.Zona = entity.Zona;

            // Inserta autoimpresor 1

            if (Autoimpresores.Count() == 0)
            {
                Autoimpresor autoimp = Autoimpresores.AddNew();
                autoimp.Sucursal = matriz;
                autoimp.Nombre = "CAJA";
                autoimp.NroAutoImpresor = 1;
            }

            // Actualiza actividad económica 1

            ActividadEconomica aeco = (from a in ActividadEconomicas
                                       where a.Empresa.Id == entity.Id && a.Nro == 1
                                       select a).SingleOrDefault();
            if (aeco == null)
                aeco = ActividadEconomicas.AddNew();

            aeco.Empresa = entity;
            aeco.Abreviacion = entity.ActividadEconomica;
            aeco.Nombre = entity.ActividadEconomica;
            aeco.Nro = 1;

            // Inserta Tipos de Facturas

            if (FacturaTipos.Count() == 0)
            {
                FacturaTipo facTipo = FacturaTipos.AddNew();
                facTipo.Empresa = entity;
                facTipo.Codigo = "HOT";
                facTipo.Descripcion = "HOTELERA";
                facTipo.SinCreditoFiscal = false;

                facTipo = FacturaTipos.AddNew();
                facTipo.Empresa = entity;
                facTipo.Codigo = "TUR";
                facTipo.Descripcion = "TURISTICA";
                facTipo.SinCreditoFiscal = true;

                facTipo = FacturaTipos.AddNew();
                facTipo.Empresa = entity;
                facTipo.Codigo = "ALQ";
                facTipo.Descripcion = "ALQUILERES";
                facTipo.SinCreditoFiscal = false;

                facTipo = FacturaTipos.AddNew();
                facTipo.Empresa = entity;
                facTipo.Codigo = "SER";
                facTipo.Descripcion = "SERVICIOS";
                facTipo.SinCreditoFiscal = false;

                facTipo = FacturaTipos.AddNew();
                facTipo.Empresa = entity;
                facTipo.Codigo = "COM";
                facTipo.Descripcion = "COMERCIAL";
                facTipo.SinCreditoFiscal = false;

                facTipo = FacturaTipos.AddNew();
                facTipo.Empresa = entity;
                facTipo.Codigo = "TOD";
                facTipo.Descripcion = "TODAS";
                facTipo.SinCreditoFiscal = false;
            }

            // Inserta Tipos de Detalles de Facturas

            if (FacturaTipoDetalles.Count() == 0)
            {
                FacturaTipoDetalle tipoDet = FacturaTipoDetalles.AddNew();
                tipoDet.Empresa = entity;
                tipoDet.Codigo = "REG";
                tipoDet.Descripcion = "REGULAR";

                tipoDet = FacturaTipoDetalles.AddNew();
                tipoDet.Empresa = entity;
                tipoDet.Codigo = "HOS";
                tipoDet.Descripcion = "HOSPEDAJE";

                tipoDet = FacturaTipoDetalles.AddNew();
                tipoDet.Empresa = entity;
                tipoDet.Codigo = "RES";
                tipoDet.Descripcion = "RESTAURANTE";

                tipoDet = FacturaTipoDetalles.AddNew();
                tipoDet.Empresa = entity;
                tipoDet.Codigo = "LAV";
                tipoDet.Descripcion = "LAVANDERIA";

                tipoDet = FacturaTipoDetalles.AddNew();
                tipoDet.Empresa = entity;
                tipoDet.Codigo = "CON";
                tipoDet.Descripcion = "CONFERENCIAS";
            }

            // Inserta Medios de Pagos

            if (MedioPagos.Count() == 0)
            {
                MedioPago medio = MedioPagos.AddNew();
                medio.Empresa = entity;
                medio.Codigo = "EF";
                medio.Descripcion = "EFECTIVO";

                medio = MedioPagos.AddNew();
                medio.Empresa = entity;
                medio.Codigo = "CP";
                medio.Descripcion = "CHQ PROPIO";

                medio = MedioPagos.AddNew();
                medio.Empresa = entity;
                medio.Codigo = "CA";
                medio.Descripcion = "CHQ AJENO";

                medio = MedioPagos.AddNew();
                medio.Empresa = entity;
                medio.Codigo = "TC";
                medio.Descripcion = "TJTA CREDITO";

                medio = MedioPagos.AddNew();
                medio.Empresa = entity;
                medio.Codigo = "DC";
                medio.Descripcion = "DEPOSITO";
            }

            // Inserta turnos

            if (Turnos.Count() == 0)
            {
                Turno turno = Turnos.AddNew();
                turno.Empresa = entity;
                turno.Codigo = "MAN";
                turno.Nombre = "MAÑANA";
                turno.HoraInicio = new DateTime(1900, 1, 1, 7, 0, 0);
                turno.HoraFinal = new DateTime(1900, 1, 1, 15, 0, 0);
                turno.Orden = "01";

                turno = Turnos.AddNew();
                turno.Empresa = entity;
                turno.Codigo = "TAR";
                turno.Nombre = "TARDE";
                turno.HoraInicio = new DateTime(1900, 1, 1, 15, 0, 0);
                turno.HoraFinal = new DateTime(1900, 1, 1, 23, 0, 0);
                turno.Orden = "02";

                turno = Turnos.AddNew();
                turno.Empresa = entity;
                turno.Codigo = "NOC";
                turno.Nombre = "NOCHE";
                turno.HoraInicio = new DateTime(1900, 1, 1, 23, 0, 0);
                turno.HoraFinal = new DateTime(1900, 1, 1, 7, 0, 0);
                turno.Orden = "03";
            }

            // Inserta Causas de Anulación

            if (CausaAnulaciones.Count() == 0)
            {
                CausaAnulacion causa = new CausaAnulacion();
                causa.Empresa = entity;
                causa.Codigo = "CLI";
                causa.Descripcion = "ERROR EN DATOS DEL CLIENTE";

                causa = new CausaAnulacion();
                causa.Empresa = entity;
                causa.Codigo = "CON";
                causa.Descripcion = "ERROR EN LOS CONCEPTOS DE LA FACTURA";

                causa = new CausaAnulacion();
                causa.Empresa = entity;
                causa.Codigo = "MON";
                causa.Descripcion = "ERROR EN EL MONTO DE LA FACTURA";

                causa = new CausaAnulacion();
                causa.Empresa = entity;
                causa.Codigo = "TIP";
                causa.Descripcion = "ERROR EN EL TIPO DE FACTURA";

                causa = new CausaAnulacion();
                causa.Empresa = entity;
                causa.Codigo = "NOF";
                causa.Descripcion = "NO CORRESPONDIA FACTURAR";

                causa = new CausaAnulacion();
                causa.Empresa = entity;
                causa.Codigo = "FIM";
                causa.Descripcion = "FALLA DE LA IMPRESORA";
            }

            // Parámetros para mails
            if(Parametros.Count() == 0)
            {
                Parametro para = new Parametro();
                para.Categoria = "SMTP";
                para.Clave = "SERVER";
                para.Valor = "mail.hosting.com";

                para = new Parametro();
                para.Categoria = "SMTP";
                para.Clave = "FROM";
                para.Valor = "mail.hosting.com";

                para = new Parametro();
                para.Categoria = "SMTP";
                para.Clave = "USER_ID";
                para.Valor = "admin@hosting.com";

                para = new Parametro();
                para.Categoria = "SMTP";
                para.Clave = "PASSWORD";
                para.Valor = "password";

                para = new Parametro();
                para.Categoria = "SMTP";
                para.Clave = "ENABLE_SSL";
                para.Valor = "N";

                para = new Parametro();
                para.Categoria = "SMTP_ENABLED";
                para.Clave = "ENABLED";
                para.Valor = "N";

                para = new Parametro();
                para.Categoria = "SMTP_ENABLED";
                para.Clave = "CAJA_ENABLED";
                para.Valor = "N";

                para = new Parametro();
                para.Categoria = "SMTP_ENABLED";
                para.Clave = "APERTURA_ENABLED";
                para.Valor = "N";

                para = new Parametro();
                para.Categoria = "SMTP_ENABLED";
                para.Clave = "CIERRE_ENABLED";
                para.Valor = "N";
            }
        }

        private void ActualizaRegistroCaja(RegistroCaja registroCaja)
        {
            if (registroCaja != null)
            {
                //
                // Ventas (facturado)
                //

                if (registroCaja.HoraFinal == registroCaja.HoraInicio)
                    registroCaja.HoraFinal = DateTime.MaxValue;
                
                List<Factura> facturas = (from Factura f in FacturasPorTurno(registroCaja.Autoimpresor.Sucursal.Empresa.Id, 
                                                             registroCaja.Autoimpresor.Sucursal.Id, 
                                                             registroCaja.Autoimpresor.Id,
                                                             registroCaja.Usuario, registroCaja.HoraInicio,
                                                             registroCaja.HoraFinal)
                                          orderby f.FechaEmision
                                          select f).ToList();

                // Cantidad y Total facturado

                Factura facturaInicial = facturas.FirstOrDefault();
                Factura facturaFinal = facturas.LastOrDefault();
                registroCaja.CantidadFacturas = facturas.Count;
                registroCaja.FacturasAnuladas = facturas.Where(f => f.Estado == "A").Count();
                registroCaja.FacturaInicialNro = facturaInicial != null ? facturaInicial.Dosificacion.NroAutorizacion + " - " + facturaInicial.Nro : string.Empty;
                registroCaja.FacturaInicialFecha = facturaInicial != null ? facturaInicial.FechaEmision : new DateTime(1900, 1, 1);
                registroCaja.FacturaFinalNro = facturaFinal != null ? facturaFinal.Dosificacion.NroAutorizacion + " - " + facturaFinal.Nro : string.Empty;
                registroCaja.FacturaFinalFecha = facturaFinal != null ? facturaFinal.FechaEmision : new DateTime(1900, 1, 1);
                registroCaja.TotalFacturadoBS = facturas.Where(f => f.Estado == "V").Sum(f => f.Monto);

                // TotalFacturadoBS dividido en Al Contado, Con Anticipos y Por Cobrar 
                registroCaja.FacturadoContadoBS = facturas.Where(f => f.Estado == "V").Sum(f => f.MontoContado);
                registroCaja.FacturadoConAnticiposBS = facturas.Where(f => f.Estado == "V").Sum(f => f.MontoAnticipado);
                registroCaja.FacturadoXCobrarBS = facturas.Where(f => f.Estado == "V").Sum(f => f.MontoPorCobrar);

                // Recaudado ventas en BS
                registroCaja.EfectivoBS = facturas.Where(f => f.Estado == "V" && f.MedioPagoContadoBS == "EF").Sum(f => f.MontoContadoBS);
                registroCaja.ChequesPropiosBS = facturas.Where(f => f.Estado == "V" && f.MedioPagoContadoBS == "CP").Sum(f => f.MontoContadoBS);
                registroCaja.ChequesAjenosBS = facturas.Where(f => f.Estado == "V" && f.MedioPagoContadoBS == "CA").Sum(f => f.MontoContadoBS);
                registroCaja.TarjetasBS = facturas.Where(f => f.Estado == "V" && f.MedioPagoContadoBS == "TC").Sum(f => f.MontoContadoBS);
                registroCaja.DepositosBS = facturas.Where(f => f.Estado == "V" && f.MedioPagoContadoBS == "DC").Sum(f => f.MontoContadoBS);

                // Recaudado ventas en US
                registroCaja.EfectivoUS = facturas.Where(f => f.Estado == "V" && f.MedioPagoContadoUS == "EF").Sum(f => f.MontoContadoUS);
                registroCaja.ChequesPropiosUS = facturas.Where(f => f.Estado == "V" && f.MedioPagoContadoUS == "CP").Sum(f => f.MontoContadoUS);
                registroCaja.ChequesAjenosUS = facturas.Where(f => f.Estado == "V" && f.MedioPagoContadoUS == "CA").Sum(f => f.MontoContadoUS);
                registroCaja.TarjetasUS = facturas.Where(f => f.Estado == "V" && f.MedioPagoContadoUS == "TC").Sum(f => f.MontoContadoUS);
                registroCaja.DepositosUS = facturas.Where(f => f.Estado == "V" && f.MedioPagoContadoUS == "DC").Sum(f => f.MontoContadoUS);

                //
                // Cobranzas
                //

                List<ClienteOperacion> cobranzas = (from ClienteOperacion co in ClienteOperacions
                                                    where co.TipoOperacion == "CC" && co.CreadoPor == registroCaja.Usuario && co.Estado != "A" && co.Fecha >= registroCaja.HoraInicio
                                                           && (registroCaja.CajaAbierta ? true : co.Fecha <= registroCaja.HoraFinal)
                                                    select co).ToList();

                // Total Cobranzas expresadas en BS
                registroCaja.TotalCobranzasBS = cobranzas.Sum(co => co.Monto);

                // Recaudado cobranzas en BS
                registroCaja.EfectivoBS += cobranzas.Where(co => co.MedioPagoBS == "EF").Sum(co => co.MontoBS);
                registroCaja.ChequesPropiosBS += cobranzas.Where(co => co.MedioPagoBS == "CP").Sum(co => co.MontoBS);
                registroCaja.ChequesAjenosBS += cobranzas.Where(co => co.MedioPagoBS == "CA").Sum(co => co.MontoBS);
                registroCaja.TarjetasBS += cobranzas.Where(co => co.MedioPagoBS == "TC").Sum(co => co.MontoBS);
                registroCaja.DepositosBS += cobranzas.Where(co => co.MedioPagoBS == "DC").Sum(co => co.MontoBS);

                // Recaudado cobranzas en US
                registroCaja.EfectivoUS += cobranzas.Where(co => co.MedioPagoUS == "EF").Sum(co => co.MontoUS);
                registroCaja.ChequesPropiosUS += cobranzas.Where(co => co.MedioPagoUS == "CP").Sum(co => co.MontoUS);
                registroCaja.ChequesAjenosUS += cobranzas.Where(co => co.MedioPagoUS == "CA").Sum(co => co.MontoUS);
                registroCaja.TarjetasUS += cobranzas.Where(co => co.MedioPagoUS == "TC").Sum(co => co.MontoUS);
                registroCaja.DepositosUS += cobranzas.Where(co => co.MedioPagoUS == "DC").Sum(co => co.MontoUS);

                //
                // Anticipos
                //

                List<ClienteOperacion> anticipos = (from ClienteOperacion co in ClienteOperacions
                                                    where co.TipoOperacion == "OA" && co.CreadoPor == registroCaja.Usuario && co.Estado != "A"
                                                            && co.Fecha >= registroCaja.HoraInicio && (registroCaja.CajaAbierta ? true : co.Fecha <= registroCaja.HoraFinal)
                                                    select co).ToList();

                // Total anticipos expresados en BS
                registroCaja.TotalAnticiposBS = anticipos.Sum(a => a.Monto);

                // Recaudado anticipos en BS
                registroCaja.EfectivoBS += anticipos.Where(a => a.MedioPagoBS == "EF").Sum(a => a.MontoBS);
                registroCaja.ChequesPropiosBS += anticipos.Where(a => a.MedioPagoBS == "CP").Sum(a => a.MontoBS);
                registroCaja.ChequesAjenosBS += anticipos.Where(a => a.MedioPagoBS == "CA").Sum(a => a.MontoBS);
                registroCaja.TarjetasBS += anticipos.Where(a => a.MedioPagoBS == "TC").Sum(a => a.MontoBS);
                registroCaja.DepositosBS += anticipos.Where(a => a.MedioPagoBS == "DC").Sum(a => a.MontoBS);

                // Recaudado anticipos en US
                registroCaja.EfectivoUS += anticipos.Where(a => a.MedioPagoUS == "EF").Sum(a => a.MontoUS);
                registroCaja.ChequesPropiosUS += anticipos.Where(a => a.MedioPagoUS == "CP").Sum(a => a.MontoUS);
                registroCaja.ChequesAjenosUS += anticipos.Where(a => a.MedioPagoUS == "CA").Sum(a => a.MontoUS);
                registroCaja.TarjetasUS += anticipos.Where(a => a.MedioPagoUS == "TC").Sum(a => a.MontoUS);
                registroCaja.DepositosUS += anticipos.Where(a => a.MedioPagoUS == "DC").Sum(a => a.MontoUS);

                //
                // Totales recaudados
                //

                registroCaja.TotalRecaudacionBS = registroCaja.FacturadoContadoBS + registroCaja.TotalCobranzasBS + registroCaja.TotalAnticiposBS;
                registroCaja.RecaudacionBS = registroCaja.EfectivoBS + registroCaja.ChequesPropiosBS + registroCaja.ChequesAjenosBS + registroCaja.TarjetasBS + registroCaja.DepositosBS;
                registroCaja.RecaudacionUS = registroCaja.EfectivoUS + registroCaja.ChequesPropiosUS + registroCaja.ChequesAjenosUS + registroCaja.TarjetasUS + registroCaja.DepositosUS;
            }
        }

        partial void Query_ExecuteFailed(QueryExecuteFailedDescriptor queryDescriptor)
        {
            ApplicationException ex = new ApplicationException("ApplicationDataService Query_ExecuteFailed!", queryDescriptor.Error);
            ex.Data.Add("Name", queryDescriptor.Name);
            ex.Data.Add("ToString", queryDescriptor.ToString());
            throw ex;
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
