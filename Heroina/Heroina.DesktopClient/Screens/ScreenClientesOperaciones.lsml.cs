using System;
using System.Linq;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using Microsoft.LightSwitch;
using Microsoft.LightSwitch.Framework.Client;
using Microsoft.LightSwitch.Presentation;
using Microsoft.LightSwitch.Presentation.Extensions;
namespace LightSwitchApplication
{
    public partial class ScreenClientesOperaciones
    {
        #region Propiedades

        IContentItemProxy DVFecha;
        IContentItemProxy DVFechaInicial;
        IContentItemProxy DVFechaFinal;

        IContentItemProxy comboEmpresas;
        IContentItemProxy comboOperacionRelacionada;

        #endregion

        #region Metodos Auxiliares
        
        void Bindings()
        {
            comboEmpresas.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty, "Screen.Empresas", System.Windows.Data.BindingMode.OneWay);
            comboEmpresas.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, "Screen.Empresas.SelectedItem", System.Windows.Data.BindingMode.TwoWay);
        }

        void Enablings()
        {
            comboEmpresas.IsEnabled = Application.Current.User.HasPermission(Permissions.CambiarEmpresa);
            comboEmpresas.IsVisible = SoftCliente.NroEmpresas > 1;
        }

        void FindControls()
        {
            comboEmpresas = this.FindControl("ComboEmpresas");
            comboOperacionRelacionada = this.FindControl("ComboOperacionRelacionada");
        }

        void InitDataWorkspace()
        {
            #region Configuracion

            Mensaje = string.Empty;

            string id = ServicesClient.ServicioFacturacion.GetInstalacionId();
            if (string.IsNullOrWhiteSpace(id))
            {
                Mensaje = "No hay archivo de configuración!";
                return;
            }
            Cfg = (from c in DataWorkspace.ApplicationData.Configuracions
                   where c.InstalacionId == id
                   select c).FirstOrDefault();
            if (Cfg == null)
            {
                Mensaje = "No hay registro de configuración!";
                return;
            }

            #endregion

            #region Soft Cliente

            SoftCliente = (from s in DataWorkspace.ApplicationData.SoftClientes
                           where s.SoftProductoId == Cfg.SoftProductoId
                           select s).FirstOrDefault();
            if (SoftCliente == null)
            {
                Mensaje = "No hay registro de cliente!";
                return;
            }

            SoftProducto = Utilidades.SoftProducto(SoftCliente);

            #endregion
        }

        #endregion

        #region Metodos Generados

        partial void ScreenClientesOperaciones_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            InitDataWorkspace();
            FindControls();
            Bindings();
            Enablings();

            DVFecha = this.FindControl("DVFecha");
            DVFechaInicial = this.FindControl("DVFechaInicial");
            DVFechaFinal = this.FindControl("DVFechaFinal");

            RangoFechas = "D";
            Fecha = DateTime.Now;
            FechaInicial = Fecha;
            FechaFinal = Fecha;
        }

        partial void ClienteOperaciones_Loaded(bool succeeded)
        {
            Mensaje = ClienteOperaciones.Count == 0 ? "No hay Operaciones de Clientes" : string.Empty;

            IContentItemProxy btnOpenInfo = this.FindControl("OpenInfo");
            btnOpenInfo.IsEnabled = ClienteOperaciones.Count != 0;
        }

        partial void CloseCliente_Execute()
        {
            this.CloseModalWindow("ModalCliente");
        }

        partial void CloseFactura_Execute()
        {
            this.CloseModalWindow("ModalFactura");
        }

        partial void CloseInfo_Execute()
        {
            this.CloseModalWindow("ModalInfo");
        }

        partial void Empresas_SelectionChanged()
        {
            EmpresaNombre = Empresas.SelectedItem != null && Empresas.Count == 1 ? Empresas.SelectedItem.Nombre : SoftCliente.ClienteNombre;
            comboEmpresas.IsVisible = Empresas.Count > 1;
        }

        partial void Generar_Execute()
        {
            if (RangoFechas == "D")
            {
                FechaInicial = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 0, 0, 0);
                FechaFinal = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 23, 59, 59);
            }
            else if (RangoFechas == "S")
            {
                FechaInicial = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 0, 0, 0);
                FechaFinal = new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 23, 59, 59);
                while (FechaInicial.DayOfWeek != DayOfWeek.Monday)
                    FechaInicial = FechaInicial.AddDays(-1);
                while (FechaFinal.DayOfWeek != DayOfWeek.Sunday)
                    FechaFinal = FechaFinal.AddDays(1);
            }
            else if (RangoFechas == "M")
            {
                FechaInicial = new DateTime(Fecha.Year, Fecha.Month, 1, 0, 0, 0);
                FechaFinal = FechaInicial.AddMonths(1).AddDays(-1);
                FechaFinal = new DateTime(FechaFinal.Year, FechaFinal.Month, FechaFinal.Day, 23, 59, 59);
            }
            else if (RangoFechas == "A")
            {
                FechaInicial = new DateTime(Fecha.Year, 1, 1, 0, 0, 0);
                FechaFinal = new DateTime(Fecha.Year, 12, 31, 23, 59, 59);
            }
            else if (RangoFechas == "T")
            {
                FechaInicial = new DateTime(2000, 1, 1, 0, 0, 0);
                FechaFinal = new DateTime(3000, 12, 31, 23, 59, 59);
            }
            else    // RangoFechas == "F"
            {
                FechaInicial = FechaInicial;
                FechaFinal = FechaFinal;
            }
        }

        partial void OpenCliente_Execute()
        {
            if (Empresas.SelectedItem == null || ClienteOperaciones.SelectedItem == null || ClienteOperaciones.SelectedItem.Cliente == null)
                return;

            Cliente = (from Cliente c in DataWorkspace.ApplicationData.Clientes
                       where c.Empresa.Id == Empresas.SelectedItem.Id
                            && c.NIT == ClienteOperaciones.SelectedItem.Cliente.NIT
                       select c).FirstOrDefault();
            this.OpenModalWindow("ModalCliente");
        }

        partial void OpenFactura_Execute()
        {
            if (Empresas.SelectedItem == null || ClienteOperaciones.SelectedItem == null || string.IsNullOrWhiteSpace(ClienteOperaciones.SelectedItem.FacturaNro))
                return;

            string[] partes = ClienteOperaciones.SelectedItem.FacturaNro.Split('-');
            string factNro = partes[0];
            string factNroAuto = partes.Length > 1 ? partes[1] : string.Empty;

            Application.ShowScreenFacturaDef(factNroAuto, factNro, HabilitarAnularDesanular: false, HabilitarEdicion: false);
        }

        partial void OpenInfo_Execute()
        {
            if (ClienteOperaciones.SelectedItem != null)
            {
                comboOperacionRelacionada.SetBinding(System.Windows.Controls.ComboBox.ItemsSourceProperty,
                                                        "Screen.OperacionRelacionada",
                                                        System.Windows.Data.BindingMode.OneWay);
                comboOperacionRelacionada.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty,
                                                        "Screen.OperacionRelacionada.SelectedItem",
                                                        System.Windows.Data.BindingMode.TwoWay);
                this.OpenModalWindow("ModalInfo");
            }
        }

        partial void RangoFechas_Changed()
        {
            if (RangoFechas == "D" || RangoFechas == "S" || RangoFechas == "M" || RangoFechas == "A")
            {
                DVFecha.IsVisible = true;
                DVFechaInicial.IsVisible = false;
                DVFechaFinal.IsVisible = false;
            }
            else if (RangoFechas == "F")
            {
                DVFecha.IsVisible = false;
                DVFechaInicial.IsVisible = true;
                DVFechaFinal.IsVisible = true;
            }
            else    // RangoFechas == "T"
            {
                DVFecha.IsVisible = false;
                DVFechaInicial.IsVisible = false;
                DVFechaFinal.IsVisible = false;
            }
        }

        #endregion
    }
}
