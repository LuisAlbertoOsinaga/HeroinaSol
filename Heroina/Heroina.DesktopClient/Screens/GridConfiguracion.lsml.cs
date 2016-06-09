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
    public partial class GridConfiguracion
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

        partial void GridConfiguracion_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            FindControls();
        }

        partial void CerrarEdicion_Execute()
        {
            this.CloseModalWindow("DatosConfiguracion");
        }

        partial void Configuraciones_Loaded(bool succeeded)
        {
            Mensaje = Configuraciones.Count == 0 ? "No hay Configuraciones definidos para desplegar!" : string.Empty;
            btnEditar.IsEnabled = Configuraciones.Count > 0;
        }

        partial void Configuraciones_SelectionChanged()
        {
            btnEditar.IsEnabled = Configuraciones.SelectedItem != null;
        }

        partial void Editar_Execute()
        {
            this.OpenModalWindow("DatosConfiguracion");
        }

        partial void GridConfiguracion_Saved()
        {
            this.Refresh();
        }

        partial void SalvarEdicion_Execute()
        {
            this.CloseModalWindow("DatosConfiguracion");
            Save();
        }

        #endregion
    }
}
