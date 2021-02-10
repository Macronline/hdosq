using System.Data;
using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using H2Q.BC.DataAccess;
using System.Data.SqlClient;
using System.Collections;
using H2Q.BC.Base;

namespace H2Q.BC.Security
{
    public enum eeResultLogin
    {
        UserNoExiste,
        LoginExitoMismaIP,
        LoginExitoDistintaIP
    }
    
    [Serializable]
    public class Usuario : Singular
    {
        public string Id;
        public string UserNameLogin;
        public string Password;
        public string NombreLargo;
        public bool IsAdmin = false;
        public bool IsActive = false;
        public string MailBox;
        public bool H2QAccess = false;
        public bool MunicipioAccess = false;
        public bool UCivilAccess = false;

        public Rol Rol = null;

        
        public eeResultLogin LoginUserCliente(string NameUser, string Pass, string CorreoUser, string UserIP, string UserIP2, ref bool IsMismIP)
        {
            bool UsuarioFinalizoSesionAnterior = false;
            int IdSesionPorFinalizarFechaUsuario = -1;
            eeResultLogin Res = eeResultLogin.UserNoExiste;

            string SQL = "READ_USER_LOGIN";
            DALCSQLServer DALC = this.GetCommonDalc();
            ArrayList parametros = new ArrayList();
            SqlParameter param = new SqlParameter("@NameUser", NameUser);
            parametros.Add(param);
            string PassEncripted = Security.Seguridad.EncriptarPass(Pass);
            param = new SqlParameter("@Password", PassEncripted);
            parametros.Add(param);
            this.Datos = DALC.ExecuteStoredProcedure("USER_READ_LOGIN", parametros);
            if (this.Datos.Rows.Count == 0)
            {
                return Res;
            }
            
            this.Id = this.Datos.Rows[0]["Id"].ToString();
            this.NombreLargo = this.Datos.Rows[0]["NombreLargo"].ToString();

            //Se lee la entidad completa
            this.ReadById();

            if (this.Persona.DREntity["IdCliente"] == DBNull.Value)
            {
                return Res;
            }

            if (! bool.Parse(this.Persona.DREntity["CuentaExterna"].ToString()))
            {
                return Res;
            }

            ////Se verifica si tiene Rol de Cliente y a tarea específica
            //Seguridad seg = new Seguridad();
            //if (! seg.UserHaveAccess(this, "ACCESS_MODULO_CLIENTES"))
            //{
            //    return Res;
            //}
            
            Res = eeResultLogin.LoginExitoMismaIP;
            DALC = null;
            return Res;
        }

        

        //Referencia al objeto Persona, la seguridad se maneja por el Id de Persona
        public Persona Persona = null;

        public Usuario()
        {
            Rol = new Rol(this);
            DataExplorer de = new DataExplorer();
            this.DREntity.ItemArray = de.InitDataRow(this).ItemArray;
        }
        public Usuario(Usuario User)
        {
            base.User = User;
            Rol = new Rol(this);
            DataExplorer de = new DataExplorer();
            this.DREntity.ItemArray = de.InitDataRow(this).ItemArray;
        }

        public void ActivarDesactivar(string idUser, bool Activar)
        {
            DALCSQLServer DALC = GetCommonDalc();
            ArrayList parametros = new ArrayList();
            SqlParameter param = new SqlParameter("@IdUser", idUser);
            parametros.Add(param);
            param = new SqlParameter("@Activar", Activar);
            parametros.Add(param);
            param = new SqlParameter("@IdUserUpdate", this.User.Id);
            parametros.Add(param);
            param = new SqlParameter("@FechaUpdate", DateTime.Now);
            parametros.Add(param);
            DALC.ExecuteNonQuery("UPD_USUARIO_ACTIVAR_DESACTIVAR", ref parametros);
        }
        
        public eeResultLogin Login(string NameUser, string Pass, string CorreoUser, string UserIP, string UserIP2, ref bool IsMismIP)
        {
            bool UsuarioFinalizoSesionAnterior = false;
            int IdSesionPorFinalizarFechaUsuario = -1;
            eeResultLogin Res = eeResultLogin.UserNoExiste;

            string SQL = "READ_USER_LOGIN";
            //"SELECT * FROM TBL_BUS_Usuario WHERE UserNameLogin = '" + NameUser + "' AND Password = '" + Pass +¿
            DALCSQLServer DALC = this.GetCommonDalc();
            ArrayList parametros = new ArrayList();
            SqlParameter param = new SqlParameter("@NameUser", NameUser);
            parametros.Add(param);
            string PassEncripted = Security.Seguridad.EncriptarPass(Pass);
            param = new SqlParameter("@Password", PassEncripted);
            parametros.Add(param);
            this.Datos = DALC.ExecuteStoredProcedure("USER_READ_LOGIN", parametros);
            if (this.Datos.Rows.Count == 0)
            {
                return Res;
            }

            this.Id = this.Datos.Rows[0]["Id"].ToString();
            this.NombreLargo = this.Datos.Rows[0]["NombreLargo"].ToString();
            this.H2QAccess =  bool.Parse(this.Datos.Rows[0]["H2QAccess"].ToString());
            this.MunicipioAccess = bool.Parse(this.Datos.Rows[0]["MunicipioAccess"].ToString());
            this.UCivilAccess = bool.Parse(this.Datos.Rows[0]["UCivilAccess"].ToString());

            this.ReadById();
            Res = eeResultLogin.LoginExitoMismaIP;
            DALC = null;
            return Res;

        }

        public bool ReadCKOfUser(string ClientQueryString, ref string CK1, ref string CK2, ref string IdUser)
        {
            bool Result = false;
            string Token = "";
            char[] split = { char.Parse("=") };
            string[] arreglo = ClientQueryString.Trim().Replace("%3d", "=").Split(split);
            if (arreglo[0] == "s") Token = arreglo[1];
            //string SQL = "SELECT * FROM TBL_BUS_SesionHistory " +
            //    " WHERE Token = '" + Token + "' AND ";
            //DALCSQLServer DALC = GetCommonDalc();
            //SQL += "  FinalizaUsuario IS NULL ";

            DALCSQLServer DALC = this.GetCommonDalc();
            ArrayList parametros = new ArrayList();
            SqlParameter param = null;
            param = new SqlParameter("@Token", Token);
            parametros.Add(param);
            this.Datos = DALC.ExecuteStoredProcedure("READ_CKOfUser", parametros);

            //DataTable DT = new DataTable();
            //DT = DALC.ExecuteSQLDirect(SQL);
            // if (DT.Rows.Count > 0)
            DataTable DT = new DataTable();
            DT = this.Datos;
            if (DT.Rows.Count > 0)
            {
                CK1 = DT.Rows[0]["CK1"].ToString();
                CK2 = DT.Rows[0]["CK2"].ToString();
                IdUser = DT.Rows[0]["IdUsuario"].ToString();
                this.Id = IdUser;   //Se transmite el IdUser
                this.ReadById();    //Se lee el registro del Usuario
                Result = true;
            }
            return Result;
        }

        public bool ReadById()
        {
            bool Res = false;
            DALCSQLServer DALC = this.GetCommonDalc();
            ArrayList parametros = new ArrayList();
            SqlParameter param = null;
            if (Id != null)
                param = new SqlParameter("@Id", Id);
            else
                param = new SqlParameter("@Id", DREntity["Id"]);
            parametros.Add(param);
            this.Datos = DALC.ExecuteStoredProcedure("USER_READ_BY_ID", parametros);
            if (this.Datos.Rows.Count > 0)
            {
                this.Id = this.Datos.Rows[0]["Id"].ToString();
                this.UserNameLogin = this.Datos.Rows[0]["UserNameLogin"].ToString();
                this.NombreLargo = this.Datos.Rows[0]["NombreLargo"].ToString();
                this.DREntity.ItemArray = this.Datos.Rows[0].ItemArray;
                this.Persona = new Persona(this);
                this.Persona.ReadByIdUsuario(this.Id);

                ////Se leen las vigencias.
                //this.UsrVigencia.ReadVigenciasUsuario(this.Id);

                //this.Cliente.Id = this.Datos.Rows[0]["IdCliente"].ToString();
                //this.Cliente.Read();

                Res = true;
            }
            DALC = null;
            return Res;
        }

        public void RenovarSesionInDB(string Token, ref string CK1, ref string CK2)
        {
            DALCSQLServer DALC = GetCommonDalc();
            string SQL = "SELECT * FROM TBL_BUS_SesionHistory " +
                " WHERE Token = '" + Token + "' AND ";
            //SQL += DALCSQLServer.FormatFechaToDBAndTime(DateTime.Now) + " >= SesionInicio AND ";
            //SQL += DALCSQLServer.FormatFechaToDBAndTime(DateTime.Now) + " <= SesionTermino AND "; 
            SQL += "  FinalizaUsuario IS NULL ";
            DataTable DT = new DataTable();
            DT = DALC.ExecuteSQLDirect(SQL);
            if (DT.Rows.Count > 0)
            {
                CK1 = DT.Rows[0]["CK1"].ToString();
                CK2 = DT.Rows[0]["CK2"].ToString();
                SQL = "UPDATE TBL_BUS_SesionHistory SET SesionTermino = DATEADD('n', 20, NOW()) " +
                    " WHERE Token = '" + Token + "' AND ";
                //SQL += DALCSQLServer.FormatFechaToDBAndTime(DateTime.Now) + " >= SesionInicio AND ";
                //SQL += DALCSQLServer.FormatFechaToDBAndTime(DateTime.Now) + " <= SesionTermino ";
                SQL += "  FinalizaUsuario IS NULL ";
                DALC.ExecuteSQLNonResult(SQL);

            }
        }
        ////public bool IsAuthorized(string NameTask)
        ////{
        ////    //Se debe preguntar si el usuario tiene acceso a esa funcionalidad
        ////    bool Res = false;
        ////    Seguridad SEC = new Seguridad();
        ////    Res = SEC.UserHaveAccess(this, NameTask);
        ////    SEC = null;
        ////    return Res;
        ////}

        //public double CreateSesionHistory(string Token, DateTime VigDesde, string IP, string IP2, string CK1, string CK2)
        //{
        //    int TiempoSesionUsr = int.Parse(ConfigurationManager.AppSettings["TiempoSesionUsr"].ToString());
        //    double ID = 0;
        //    DALCSQLServer DALC = GetCommonDalc();
        //    ArrayList parametros = new ArrayList();
        //    SqlParameter param = new SqlParameter("@IdUsuario", this.Id);
        //    parametros.Add(param);
        //    param = new SqlParameter("@SesionInicio", VigDesde);
        //    parametros.Add(param);
        //    param = new SqlParameter("@SesionTermino", VigDesde.AddMinutes(40));
        //    parametros.Add(param);
        //    param = new SqlParameter("@Token", Token);
        //    parametros.Add(param);
        //    param = new SqlParameter("@Ip", IP);
        //    parametros.Add(param);
        //    param = new SqlParameter("@Ip2", IP2);
        //    parametros.Add(param);
        //    param = new SqlParameter("@CK1", CK1);
        //    parametros.Add(param);
        //    param = new SqlParameter("@CK2", CK2);
        //    parametros.Add(param);
        //    ID = DALC.ExecuteSQLScalar("INS_SESSIONHISTORY", parametros);
        //    return ID;

        //}

        /// <summary>
	    /// Método que indica si el rut del usuario coincide con el correo electrónico
	    /// registrado para ese rut en la bd.
	    /// </summary>
	    /// <param name="Rut"></param>
	    /// <param name="CorreoPersonal"></param>
	    public void EsCorreoPersonalValido(string Rut, string CorreoPersonal)
        {

        }

        /// <summary>
        /// Método que lee los usuarios activos y disponibles del sistema para los accesos
        /// a una Carpeta (si va -1 implica carpeta nueva)
        /// </summary>
        /// <param name="IdCarpeta"></param>
        public void ReadAllUsuariosActivosDisponiblesParaCarpeta(int IdCarpeta)
        {
            DALCSQLServer DALC = this.GetCommonDalc();
            ArrayList parametros = new ArrayList();
            SqlParameter param = null;
            param = new SqlParameter("@IdCarpeta", IdCarpeta);
            parametros.Add(param);
            this.Datos = DALC.ExecuteStoredProcedure("READ_ALL_USUARIOS_DISP_PARA_CARPETA", parametros);

        }

        /// <summary>
        /// Método que lee los usuarios activos y disponibles del sistema para los accesos
        /// a una Carpeta (si va -1 implica carpeta nueva)
        /// </summary>
        /// <param name="IdCarpeta"></param>
        public void ReadAllUsuariosActivos(bool SoloActivos)
        {
            DALCSQLServer DALC = this.GetCommonDalc();
            ArrayList parametros = new ArrayList();
            SqlParameter param = new SqlParameter("@SoloActivos", SoloActivos);
            parametros.Add(param);
            this.Datos = DALC.ExecuteStoredProcedure("READ_ALL_USUARIOS_ACTIVOS", parametros);

        }
        /// <summary>
        /// Método que crea un nuevo registro de Usuario. Asigna rut como username y la
        /// clave autogenerada. Puede ser calculada inteligentemente para que el usuario no
        /// tenga que ingresar directamente a cambiar clave.
        /// </summary>
        public double Create()
        {
            double ID = 0;
            DALCSQLServer DALC = GetCommonDalc();
            ArrayList parametros = new ArrayList();
            SqlParameter param = new SqlParameter("@UserNameLogin", this.DREntity["UserNameLogin"].ToString());
            parametros.Add(param);
            string PassEncripted = Security.Seguridad.EncriptarPass(this.DREntity["Password"].ToString());
            param = new SqlParameter("@Password", PassEncripted);
            parametros.Add(param);

            param = new SqlParameter("@H2QAccess", this.DREntity["H2QAccess"].ToString());
            parametros.Add(param);
            param = new SqlParameter("@MunicipioAccess", this.DREntity["MunicipioAccess"].ToString());
            parametros.Add(param);
            param = new SqlParameter("@UCivilAccess", this.DREntity["UCivilAccess"].ToString());
            parametros.Add(param);

            if (this.User != null)
                param = new SqlParameter("@IdUserCreate", this.User.Id);
            else
                param = new SqlParameter("@IdUserCreate", DBNull.Value);
            parametros.Add(param);
            param = new SqlParameter("@FechaCreate", DateTime.Now);
            parametros.Add(param);
            ID = DALC.ExecuteSQLScalar("INS_USUARIO", parametros);

            DataTable DTAux = DALC.ExecuteSQLDirect("SELECT MAX(Id) AS MAXIMO FROM TBL_BUS_Usuario");
            ID =double.Parse( DTAux.Rows[0]["MAXIMO"].ToString());
            return ID;
        }

        /// <summary>
        /// Método que genera una password para el usuario.
        /// </summary>
        private void GenerarPassword()
        {

        }

        /// <summary>
        /// Este método retorna True si el usuario no ha cambiado su clave
        /// </summary>
        /// <param name="Rut"></param>
        public void EsClavePorDefecto(string Rut)
        {

        }

        /// <summary>
        /// Este método retorna True si la clave actual coincide y aún es la password
        /// vigente.
        /// </summary>
        /// <param name="PasswordActual"></param>
        public bool EsContraseñaActual(string PasswordActual, double Id)
        {
            bool res = false;
            DALCSQLServer DALC = this.GetCommonDalc();
            ArrayList parametros = new ArrayList();
            SqlParameter param = null;
            string PassEncripted = Security.Seguridad.EncriptarPass(PasswordActual);
            param = new SqlParameter("@PasswordActual", PassEncripted);
            parametros.Add(param);
            param = new SqlParameter("@Id", Id);
            parametros.Add(param);
            DataTable DT = null;
            DT = DALC.ExecuteStoredProcedure("IS_USUARIO_PWD", parametros);
            res = (DT.Rows.Count != 0);
            return res;
        }

        /// 
        /// <param name="PassNueva"></param>
        /// <param name="PassActual"></param>
        public void UpdateContraseña(string PassNueva, string PassActual)
        {

        }

        /// <summary>
        /// Este método retorna True si el Rut coincide con la cuenta de correo personal y
        /// si la cuenta del usuario está activa. False en caso contrario.
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="CorreoPersonal"></param>
        private bool EsCuentaPersonalConRutYUsuarioValido(string UserName, string CorreoPersonal)
        {

            return false;
        }

        /// <summary>
        /// Método que soporta la recuperación de la contraseña
        /// </summary>
        /// <param name="NameUser"></param>
        /// <param name="CorreoParticular"></param>
        public void RecuperarContraseña(string NameUser, string CorreoParticular)
        {

        }

        /// <summary>
        /// Método que cierra la sesión del usuario
        /// </summary>
        public void Logoff()
        {

        }

        public void Update()
        {
            DALCSQLServer DALC = GetCommonDalc();
            ArrayList parametros = new ArrayList();
            SqlParameter param = new SqlParameter("@Id", this.DREntity["Id"].ToString());
            parametros.Add(param);
            param = new SqlParameter("@UserNameLogin", this.DREntity["UserNameLogin"].ToString());
            parametros.Add(param);
            string PassEncripted = Security.Seguridad.EncriptarPass(this.DREntity["Password"].ToString());
            param = new SqlParameter("@Password", PassEncripted);
            parametros.Add(param);
            param = new SqlParameter("@Activo", this.DREntity["Activo"]);
            parametros.Add(param);
            param = new SqlParameter("@IdUserUpdate", this.User.Id);
            parametros.Add(param);
            param = new SqlParameter("@FechaUpdate", DateTime.Now);
            parametros.Add(param);
            DALC.ExecuteNonQuery("UPD_USUARIO", ref parametros);
        }

        public bool ExisteUsuarioByUserName(string UserNameLogin)
        {
            bool res = false;
            DALCSQLServer DALC = this.GetCommonDalc();
            ArrayList parametros = new ArrayList();
            SqlParameter param = null;
            param = new SqlParameter("@UserNameLogin", UserNameLogin);
            parametros.Add(param);
            DataTable DT = null;
            DT = DALC.ExecuteStoredProcedure("READ_USUARIO_BY_USERNAME", parametros);
            res = (DT.Rows.Count != 0);
            return res;
        }

        public bool EsNameUserAndCorreoMCM(string CuentaCorreoMCM, string RutNameUsuario)
        {
            bool res = false;
            DALCSQLServer DALC = this.GetCommonDalc();
            ArrayList parametros = new ArrayList();
            SqlParameter param = null;
            param = new SqlParameter("@UserNameLogin", RutNameUsuario);
            parametros.Add(param);
            param = new SqlParameter("@CuentaCorreoMCM", CuentaCorreoMCM);
            parametros.Add(param);
            DataTable DT = null;
            DT = DALC.ExecuteStoredProcedure("EXIST_USUARIO_BY_CORREO_MCM", parametros);
            res = (DT.Rows.Count != 0);
            if (res)
            //    this.DREntity.ItemArray = DT.Rows[0].ItemArray;
            { 
                if (this.DREntity.DT.Columns.Count != DT.Rows[0].ItemArray.Length)
                    this.DREntity.DT = DT.Clone();
                this.DREntity.ItemArray = DT.Rows[0].ItemArray;
            }
            return res;
        }

        public void UpdateContraseñaDefault(double IdUsuario)
        {
            DALCSQLServer DALC = GetCommonDalc();
            ArrayList parametros = new ArrayList();
            SqlParameter param = new SqlParameter("@Id", IdUsuario);
            parametros.Add(param);
            string PassEncripted = Security.Seguridad.EncriptarPass("123456MCM");
            param = new SqlParameter("@Password", PassEncripted);
            parametros.Add(param);
            DALC.ExecuteNonQuery("UPD_USUARIO_PASSWORD_RESET", ref parametros);
        }

        public void UpdateContraseña(double IdUsuario, string PassNew)
        {
            DALCSQLServer DALC = GetCommonDalc();
            ArrayList parametros = new ArrayList();
            SqlParameter param = new SqlParameter("@Id", IdUsuario);
            parametros.Add(param);
            string PassEncripted = Security.Seguridad.EncriptarPass(PassNew);
            param = new SqlParameter("@Password", PassEncripted);
            parametros.Add(param);
            DALC.ExecuteNonQuery("UPD_USUARIO_PASSWORD_RESET", ref parametros);
        }
        public bool EsSuperUsuario()
        {
            this.Datos = this.GetCommonDalc().ExecuteStoredProcedure("SECURITY_READ_ALL_ES_SUPERUSUARIO", new ArrayList()
          {
             new SqlParameter("@IdUsuario",  this.Id)
          });
            return this.Datos.Rows.Count != 0;
        }
        public void CheckSpaceBD()
        {
            this.GetCommonDalc();
            ArrayList arrayList = new ArrayList();
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["strStateDB"].ToString()))
            {
                string cmdText = "DELETE FROM dbo.ASPStateTempSessions  where Created < GETDATE() - 1";
                DataTable dataTable1 = new DataTable();
                SqlCommand sqlCommand = new SqlCommand(cmdText, connection);
                connection.Open();
                sqlCommand.ExecuteNonQuery();
                connection.Close();
                DataTable dataTable2 = new DataTable();
                new SqlDataAdapter(new SqlCommand("EXEC sp_spaceused", connection)).Fill(dataTable2);
                if (dataTable2.Rows.Count <= 0)
                    return;
                string str = dataTable2.Rows[0]["database_size"].ToString();
                double result = 0.0;
                if (double.TryParse(str.Substring(0, str.IndexOf(".")), out result) && result > double.Parse(ConfigurationManager.AppSettings["PesoStateDB"].ToString()))
                {
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();
                    new SqlCommand("DELETE FROM ASPStateTempSessions where Created < GETDATE() - 1", connection).ExecuteNonQuery();
                    connection.Close();
                }
            }
        }
        public double CreateSesionHistory(string Token, DateTime VigDesde, string IP, string IP2, string CK1, string CK2)
        {
            int TiempoSesionUsr = int.Parse(ConfigurationManager.AppSettings["TiempoSesionUsr"].ToString());
            double ID = 0;
            DALCSQLServer DALC = GetCommonDalc();
            ArrayList parametros = new ArrayList();
            SqlParameter param = new SqlParameter("@IdUsuario", this.Id);
            parametros.Add(param);
            param = new SqlParameter("@SesionInicio", VigDesde);
            parametros.Add(param);
            param = new SqlParameter("@SesionTermino", VigDesde.AddMinutes(40));
            parametros.Add(param);
            param = new SqlParameter("@Token", Token);
            parametros.Add(param);
            param = new SqlParameter("@Ip", IP);
            parametros.Add(param);
            param = new SqlParameter("@Ip2", IP2);
            parametros.Add(param);
            param = new SqlParameter("@CK1", CK1);
            parametros.Add(param);
            param = new SqlParameter("@CK2", CK2);
            parametros.Add(param);
            ID = DALC.ExecuteSQLScalar("INS_SESSIONHISTORY", parametros);
            return ID;

        }
    }//end User
}
