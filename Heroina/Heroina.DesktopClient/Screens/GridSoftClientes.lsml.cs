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
    public partial class GridSoftClientes
    {
        #region Propiedades

        IContentItemProxy btnEditar;

        #endregion

        #region Metodos Auxiliares

        void FindControls()
        {
            btnEditar = this.FindControl("Editar");
        }

        #endregion

        #region Metodos Generados

        partial void GridSoftClientes_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            FindControls();
        }

        partial void CerrarEdicion_Execute()
        {
            this.CloseModalWindow("DatosSoftCliente");
        }

        partial void Editar_Execute()
        {
            this.OpenModalWindow("DatosSoftCliente");
        }

        partial void SalvarEdicion_Execute()
        {
            this.CloseModalWindow("DatosSoftCliente");
            Save();
        }

        partial void SoftClientes_Loaded(bool succeeded)
        {
            Mensaje = SoftClientes.Count == 0 ? "No hay SoftClientes definidos para desplegar!" : string.Empty;
            btnEditar.IsEnabled = SoftClientes.Count > 0;
        }

        partial void SoftClientes_SelectionChanged()
        {
            btnEditar.IsEnabled = SoftClientes.SelectedItem != null;
        }

        #endregion
    }
}
