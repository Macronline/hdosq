using H2Q.BC.Base;
using H2Q.BC.DataAccess;
using H2Q.BC.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H2Q.BC
{
    [Serializable]

    public class Persona : Singular
    {
        public Persona()
        {
            DataExplorer de = new DataExplorer();
            this.DREntity.ItemArray = de.InitDataRow(this).ItemArray;
        }

        public Persona(Usuario User)
        {
            base.User = User;
            DataExplorer de = new DataExplorer();
            this.DREntity.ItemArray = de.InitDataRow(this).ItemArray;
        }
        public void ReadAll()
        {
            DALCSQLServer DALC = this.GetCommonDalc();
            ArrayList parametros = new ArrayList();
            this.Datos = DALC.ExecuteStoredProcedure("READ_ALL_PERSONAS", parametros);
        }

        internal void ReadByIdUsuario(string IdUsuario)
        {
            DALCSQLServer DALC = this.GetCommonDalc();
            ArrayList parametros = new ArrayList();
            SqlParameter param = new SqlParameter("@IdUsuario", IdUsuario);
            parametros.Add(param);
            this.Datos = DALC.ExecuteStoredProcedure("READ_PERSONA_BY_IDUSUARIO", parametros);
            if (this.Datos.Rows.Count == 1)
            {
                this.DREntity.ItemArray = this.Datos.Rows[0].ItemArray;
            }
        }

        public double Create()
        {
            double ID = 0;
            DALCSQLServer DALC = GetCommonDalc();
            ArrayList parametros = new ArrayList();
            SqlParameter param = new SqlParameter("@Rut", this.DREntity["Rut"].ToString());
            parametros.Add(param);
            param = new SqlParameter("@Nombre1", this.DREntity["Nombre1"].ToString());
            parametros.Add(param);
            param = new SqlParameter("@Nombre2", this.DREntity["Nombre2"].ToString());
            parametros.Add(param);
            param = new SqlParameter("@Apellido1", this.DREntity["Apellido1"].ToString());
            parametros.Add(param);
            param = new SqlParameter("@Apellido2", this.DREntity["Apellido2"].ToString());
            parametros.Add(param);
            param = new SqlParameter("@FechaNac", this.DREntity["FechaNac"].ToString());
            parametros.Add(param);
            param = new SqlParameter("@DireccionParticular", this.DREntity["DireccionParticular"].ToString());
            parametros.Add(param);
            param = new SqlParameter("@TelefonoMovil", this.DREntity["TelefonoMovil"].ToString());
            parametros.Add(param);
            param = new SqlParameter("@TelefonoCasa", this.DREntity["TelefonoCasa"].ToString());
            parametros.Add(param);
            param = new SqlParameter("@ECorreoPersonal", this.DREntity["ECorreoPersonal"].ToString());
            parametros.Add(param);
            param = new SqlParameter("@ECorreoEmpresa", this.DREntity["ECorreoEmpresa"].ToString());
            parametros.Add(param);
            param = new SqlParameter("@IdParamEstadoCivil", this.DREntity["IdParamEstadoCivil"].ToString());
            parametros.Add(param);
            param = new SqlParameter("@IdUsuario", this.DREntity["IdUsuario"].ToString());
            parametros.Add(param);
            if (this.User != null)
                param = new SqlParameter("@IdUserCreate", this.User.Id);
            else
                param = new SqlParameter("@IdUserCreate", DBNull.Value);
            parametros.Add(param);
            param = new SqlParameter("@FechaCreate", DateTime.Now);
            parametros.Add(param);
            ID = DALC.ExecuteSQLScalar("INS_PERSONA", parametros);
            return ID;
        }
    }
}
