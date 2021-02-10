using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Collections;
using H2Q.BC.Security;

namespace H2Q.BC.DataAccess
{
    public enum eeTypeConnection
    {
        eBDMaster
    }

    [Serializable]
    public class DALCSQLServer
    {
        private static SortedList<string, SqlConnection> HashConn = new SortedList<string, SqlConnection>();

        //private static SqlConnection SqlCon = null;
        private static SqlConnection DBConAgnostic = null;
        private static SqlConnection DBConMaster = null;
        //private static SqlConnection DBConDataClient = null;

        private static SqlCommand DBCommandToTextDirect = null;
        private static SqlCommand DBCommandToStoreProcedure = null;
        private static SqlCommand DBCommandExecuteNonQuery = null;

        public string _TypeConnection = eeTypeConnection.eBDMaster.ToString();
        public string IdClient = "";
        private string StriConnGlobal = "";

        public DALCSQLServer()
        {
        }
        public DALCSQLServer(Usuario User)
        {
            //this.SetTypeConnection(User);
        }

        //private void SetTypeConnection(User User)
        //{
        //    if (User.IdClient != null)
        //    {
        //        _TypeConnection = User.IdClient;
        //    }
        //}

        public static string GetValueForBoolValue(bool Value)
        {
            if (Value)
                return "1";
            else
                return "0";
        }

        internal static string FormatFechaToDB(object dt)
        {
            string res = "";
            DateTime DateTime;
            if (DateTime.TryParse(dt.ToString(), out DateTime))
            {
                if (ConfigurationManager.AppSettings["FormatoFecha"].ToString() == "yyyymmdd")
                    res = "'" + DateTime.Year.ToString() + "-" + DateTime.Month.ToString("00") + "-" +
                        DateTime.Day.ToString("00") + "T00:00:00'";
                else
                    res = "'" + DateTime.Year.ToString() + "-" + DateTime.Day.ToString("00") + "-" +
                        DateTime.Month.ToString("00") + "T00:00:00'";
            }
            return res;
        }

        public static string FormatFechaToDB(DateTime DateTime)
        {
            string res = "";
            if (ConfigurationManager.AppSettings["FormatoFecha"].ToString() == "yyyymmdd")
                res = "'" + DateTime.Year.ToString() + "-" + DateTime.Month.ToString("00") + "-" +
                    DateTime.Day.ToString("00") + "T00:00:00'";
            else
                res = "'" + DateTime.Year.ToString() + "-" + DateTime.Day.ToString("00") + "-" +
                    DateTime.Month.ToString("00") + "T00:00:00'";
            return res;
        }
        public static string FormatFechaToDBAndTime(DateTime DateTime)
        {
            string res = "";
            if (ConfigurationManager.AppSettings["FormatoFecha"].ToString() == "yyyymmdd")
                res = "'" + DateTime.Year.ToString() + "-" + DateTime.Month.ToString("00") + "-" +
                    DateTime.Day.ToString("00") + "T" + DateTime.Hour.ToString("00") + ":" +
                    DateTime.Minute.ToString("00") + ":" + DateTime.Second.ToString("00") + "'";
            else
                res = "'" + DateTime.Year.ToString() + "-" + DateTime.Day.ToString("00") + "-" +
                    DateTime.Month.ToString("00") + "'";
            return res;
        }

        public SqlConnection DBConnectionForBDMaster
        {
            get { return DALCSQLServer.DBConMaster; }
            set { DALCSQLServer.DBConMaster = value; }
        }

        public void SetTypeConnection(string _TypeConnection)
        {
            this._TypeConnection = _TypeConnection;
        }

        public SqlDbType getSqlDBType(Type dataType)
        {
            if (dataType.FullName == "System.String")
                return SqlDbType.VarChar;
            if (dataType.FullName == "System.Int64")
                return SqlDbType.BigInt;
            if (dataType.FullName == "System.DateTime")
                return SqlDbType.BigInt;
            if (dataType.FullName == "System.Decimal")
                return SqlDbType.Decimal;

            return SqlDbType.VarChar;
        }

        public string ExecuteSQLScalar(string SQLString)
        {
            string Result = "";
            try
            {
                if (HashConn.ContainsKey(_TypeConnection))
                    DBConAgnostic = HashConn[_TypeConnection];
                else
                    if (_TypeConnection == eeTypeConnection.eBDMaster.ToString())
                    //Se instancia MasterDB
                    InstanceConnectionMaster();
                else
                    InstanceConnectionClient();

                //Se instancia si es null
                if (DBCommandExecuteNonQuery == null) InstanceDBCommandToStoreProcedure(CommandType.Text, ref DBCommandExecuteNonQuery, DBConAgnostic);
                DBCommandExecuteNonQuery.CommandText = SQLString;

                if (DBConAgnostic.State == ConnectionState.Closed) DBConAgnostic.Open();

                Result = DBCommandExecuteNonQuery.ExecuteScalar().ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
            return Result;
        }
        public byte[] ExecuteSQLScalarBinary(string SQLString)
        {
            byte[] Result = null;
            try
            {
                if (HashConn.ContainsKey(_TypeConnection))
                    DBConAgnostic = HashConn[_TypeConnection];
                else
                    if (_TypeConnection == eeTypeConnection.eBDMaster.ToString())
                    //Se instancia MasterDB
                    InstanceConnectionMaster();
                else
                    InstanceConnectionClient();

                //Se instancia si es null
                if (DBCommandExecuteNonQuery == null) InstanceDBCommandToStoreProcedure(CommandType.Text, ref DBCommandExecuteNonQuery, DBConAgnostic);
                DBCommandExecuteNonQuery.CommandText = SQLString;
                if (DBConAgnostic.State == ConnectionState.Closed) DBConAgnostic.Open();
                Result = (byte[])DBCommandExecuteNonQuery.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
            return Result;
        }
        public DataTable ExecuteSQL(string SQLString)
        {
            DataTable DS = new DataTable();
            SqlDataAdapter DataAdap = null;
            try
            {
                if (HashConn.ContainsKey(_TypeConnection))
                    DBConAgnostic = HashConn[_TypeConnection];
                else
                    if (_TypeConnection == eeTypeConnection.eBDMaster.ToString())
                    //Se instancia MasterDB
                    InstanceConnectionMaster();
                else
                    InstanceConnectionClient();

                //DBCommandToTextDirect.CommandText = SQLString;

                SqlCommand comm = new SqlCommand();
                InstanceDBCommandToStoreProcedure(CommandType.Text, ref comm, DBConAgnostic);
                comm.CommandText = SQLString;
                
                DataAdap = new SqlDataAdapter(comm);
                DataAdap.Fill(DS);
                DataAdap = null;
                comm = null;
                return DS;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                DataAdap = null;
            }
        }


        public int ExecuteNonQuery(string NameStoredProcedure, ref ArrayList Parameters)
        {
            int Result;
            try
            {
                //if (DBConMaster == null)
                //    InstanceConnectionMaster();

                //Chequea la conexion para el usuario
                SetConexionActivaPreExecutionConsulta();

                using (SqlConnection sqlConnection = new SqlConnection(this.GetStringConnection()))
                {
                    SqlCommand sqlCommand = new SqlCommand(NameStoredProcedure);
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["CommandTimeoutExecuteBD"].ToString());
                    for (int index = 0; index < Parameters.Count; ++index)
                        sqlCommand.Parameters.Add(Parameters[index]);
                    if (sqlConnection.State == ConnectionState.Closed)
                        sqlConnection.Open();
                    Result = sqlCommand.ExecuteNonQuery();
                    if (sqlConnection.State == ConnectionState.Open)
                        sqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Result;
        }

        public DataTable ExecuteStoredProcedure(string NameSP)
        {
            DataTable DT;
            try
            {
                DataSet DS = new DataSet();
                if (DBConMaster == null)
                    InstanceConnectionMaster();

                //Se instancia si es null
                if (DBCommandToStoreProcedure == null) InstanceDBCommandToStoreProcedure();

                DBCommandToStoreProcedure.CommandText = NameSP;
                SqlDataAdapter DataAdap = new SqlDataAdapter(DBCommandToStoreProcedure);
                DataAdap.Fill(DS);
                DT = DS.Tables[0];
                DS.Tables.Remove(DT);
                DataAdap = null;
                DS = null;
                return DT;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public DataTable ExecuteSQLDirect(string SQLString, ArrayList Parameters)
        {
            DataSet DS = new DataSet();
            try
            {
                InstanceDBCommandToTextDirect();

                //Chequea la conexion para el usuario
                SetConexionActivaPreExecutionConsulta();
                
                DBCommandToTextDirect.Parameters.Clear();
                for (int i = 0; i < Parameters.Count; i++)
                {
                    DBCommandToTextDirect.Parameters.Add(Parameters[i]);
                }

                DBCommandToTextDirect.CommandText = SQLString;
                SqlDataAdapter DataAdap = new SqlDataAdapter(DBCommandToTextDirect);
                DataAdap.Fill(DS);
                DataAdap = null;
                DataTable DT = DS.Tables[0];
                DS.Tables.RemoveAt(0);
                return DT;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                DS = null;
            }
        }
        public DataTable ExecuteSQLDirect(string SQLString)
        {
            DataSet DS = new DataSet();
            try
            {
                //InstanceDBCommandToTextDirect();

                //Chequea la conexion para el usuario
                SetConexionActivaPreExecutionConsulta();

                SqlCommand comm = new SqlCommand();
                comm.CommandText = SQLString;
                comm.Connection = DBConAgnostic;
                comm.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["CommandTimeoutExecuteBD"].ToString());
                SqlDataAdapter DataAdap = new SqlDataAdapter(comm);
                DataAdap.Fill(DS);
                DataAdap = null;
                comm = null;
                DataTable DT = DS.Tables[0];
                DS.Tables.RemoveAt(0);
                return DT;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                DS = null;
            }
        }

        private void SetConexionActivaPreExecutionConsulta()
        {
            if (HashConn.ContainsKey(_TypeConnection))
            {
                DBConAgnostic = HashConn[_TypeConnection];
                DBCommandToTextDirect.Connection = DBConAgnostic;
                DBCommandToStoreProcedure.Connection = DBConAgnostic;
                //DBCommandExecuteNonQuery.Connection = DBConAgnostic;
            }
            else
            {
                if (_TypeConnection == eeTypeConnection.eBDMaster.ToString())
                {
                    //Se instancia MasterDB si no esta ya instanciada en el Pool
                    if (!HashConn.ContainsKey(eeTypeConnection.eBDMaster.ToString()))
                        InstanceConnectionMaster();

                    DBCommandToTextDirect.Connection = DBConAgnostic;
                    DBCommandToStoreProcedure.Connection = DBConAgnostic;
                }
                else
                {
                    InstanceConnectionClient();

                    DBCommandToTextDirect.Connection = DBConAgnostic;
                    DBCommandToStoreProcedure.Connection = DBConAgnostic;

                }
            }
        }
        public DataTable ExecuteStoredProcedure(string NameSP, ArrayList Parameters)
        {
            DataTable DT = null;
            SqlConnection connSQL = null;
            try
            {
                DataSet DS = new DataSet();
                //if (DBConMaster == null) InstanceConnectionMaster();

                //Chequea la conexion para el usuario
                //SetConexionActivaPreExecutionConsulta();
                string StriConnAux = ConfigurationManager.AppSettings["StriConnAux"];
                connSQL = new SqlConnection(StriConnAux);

                connSQL.Open();
                SqlCommand DBCommandToStoreProcedureLocal = new SqlCommand(NameSP, connSQL);
                DBCommandToStoreProcedureLocal.CommandType = CommandType.StoredProcedure;
                DBCommandToStoreProcedureLocal.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["CommandTimeoutExecuteBD"].ToString());
                for (int i = 0; i < Parameters.Count; i++)
                {
                    DBCommandToStoreProcedureLocal.Parameters.Add(Parameters[i]);
                }

                SqlDataAdapter DataAdap = new SqlDataAdapter(DBCommandToStoreProcedureLocal);
                DataAdap.Fill(DS);
                DataAdap = null;
                DBCommandToStoreProcedureLocal = null;
                DT = DS.Tables[0];
                DS.Tables.Remove(DT);
                DS = null;
                connSQL.Close();
                connSQL = null;
                return DT;
            }
            catch (Exception ex)
            {
                if (connSQL != null && connSQL.State == ConnectionState.Open)
                    connSQL.Close();
                connSQL = null;
                throw ex;
            }
        }
        public DataTable ExecuteStoredProcedure(string NameSP, SortedList Parameters)
        {
            DataTable DT;
            try
            {
                DataSet DS = new DataSet();

                if (DBConMaster == null)
                    InstanceConnectionMaster();

                DBCommandToStoreProcedure.Parameters.Clear();
                for (int i = 0; i < Parameters.Count; i++)
                {
                    DBCommandToStoreProcedure.Parameters.Add(Parameters.GetByIndex(i));
                }

                DBCommandToStoreProcedure.CommandText = NameSP;
                SqlDataAdapter DataAdap = new SqlDataAdapter(DBCommandToStoreProcedure);
                DataAdap.Fill(DS);
                DT = DS.Tables[0];
                DS.Tables.Remove(DT);
                DataAdap = null;
                DS = null;
                return DT;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool ExecuteSQLNonResult(string SQLString)
        {
            try
            {
                if (HashConn.ContainsKey(_TypeConnection))
                    DBConAgnostic = HashConn[_TypeConnection];
                else
                    if (_TypeConnection == eeTypeConnection.eBDMaster.ToString())
                    //Se instancia MasterDB
                    InstanceConnectionMaster();
                else
                    InstanceConnectionClient();

                //Se instancia si es null
                if (DBCommandExecuteNonQuery == null) InstanceDBCommandToStoreProcedure(CommandType.Text, ref DBCommandExecuteNonQuery, DBConAgnostic);
                DBCommandExecuteNonQuery.CommandText = SQLString;

                if (DBConAgnostic.State == ConnectionState.Closed) DBConAgnostic.Open();

                int Affected = DBCommandExecuteNonQuery.ExecuteNonQuery();
                DBConAgnostic.Close();
                return (Affected != 0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ExecuteSQLNonResultDirecto(string SQLString)
        {
            try
            {
                SqlCommand localCommand = new SqlCommand(SQLString, DBConAgnostic);
                if (DBConAgnostic.State == ConnectionState.Closed) DBConAgnostic.Open();
                localCommand.ExecuteNonQuery();
                DBConAgnostic.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool ExecuteSQLNonResult(string SQLString, SqlTransaction oTx)
        {
            try
            {
                if (HashConn.ContainsKey(_TypeConnection))
                    DBConAgnostic = HashConn[_TypeConnection];
                else
                    if (_TypeConnection == eeTypeConnection.eBDMaster.ToString())
                    //Se instancia MasterDB
                    InstanceConnectionMaster();
                else
                    InstanceConnectionClient();

                //Se instancia si es null
                if (DBCommandExecuteNonQuery == null) InstanceDBCommandToStoreProcedure(CommandType.Text, ref DBCommandExecuteNonQuery, DBConAgnostic);
                DBCommandExecuteNonQuery.CommandText = SQLString;
                DBCommandExecuteNonQuery.Transaction = oTx;

                if (DBConAgnostic.State == ConnectionState.Closed) DBConAgnostic.Open();

                int Affected = DBCommandExecuteNonQuery.ExecuteNonQuery();
                //DBCon.Close();
                return (Affected != 0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        ////////private void InstanceConnectionDBClient()
        ////////{
        ////////    string sDbCatalog =ConfigurationManager.AppSettings["DBCatalog"];
        ////////    string WinForm = ConfigurationManager.AppSettings["WinForm"];
        ////////    string Desa = ConfigurationManager.AppSettings["Desa"];

        ////////    if (Desa == "False")
        ////////        sDbCatalog = System.Threading.Thread.GetDomain().BaseDirectory + @"\App_Data\" + sDbCatalog + ".mdb";
        ////////    else
        ////////        sDbCatalog = @"D:\DesaNET\Macronline\DataBases\AgendaDigital\" + sDbCatalog + ".mdb";

        ////////    string StriConn =@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + 
        ////////        sDbCatalog + ";User Id=admin;Password=;";

        ////////    DBConDataClient = new SqlConnection(StriConn);
        ////////    DBConDataClient.Open();
        ////////    //Add a la coleccione de Conexxiones
        ////////    HashConn.Add(eeTypeConnection.eBDEmp.ToString(), DBConMaster);

        ////////    if (DBCommandToStoreProcedure == null)
        ////////    {
        ////////        InstanceDBCommandToStoreProcedure();
        ////////    }
        ////////}
        private void InstanceConnectionClient()
        {
            string sDbCatalog = "";
            if (ConfigurationManager.AppSettings["Application"].ToString() == "GesDoc")
            {
                sDbCatalog = ConfigurationManager.AppSettings["DBCatalog"];
            }

            string Desa = ConfigurationManager.AppSettings["Desa"];
            string sDbServer = ConfigurationManager.AppSettings["dbserver"];
            string sDbUser = ConfigurationManager.AppSettings["dbuser"];
            string sDbPassword = ConfigurationManager.AppSettings["dbpassword"];
            string sTrusted = ConfigurationManager.AppSettings["trusted"];

            try
            {
                //Apunta Conexion a BD de Cliente
                sDbCatalog = sDbCatalog.Replace("Sec", "") + this._TypeConnection;
                //sDbCatalog = "MacroGesDocFINAL";
                //string StriConn = @"Data Source=" + sDbServer + ";" +
                //"Initial Catalog=" + sDbCatalog + ";" +
                //"User ID=" + sDbUser + ";" +
                //"Password=" + sDbPassword + ";Trusted_Connection= " + sTrusted + ";";

                string StriConn = @"Data Source=" + sDbServer + ";" +
                "Initial Catalog=" + sDbCatalog + ";Integrated Security=SSPI;";// +
                //"User ID=" + sDbUser + ";" +
                //"Password=" + sDbPassword + ";"; //Trusted_Connection= " + sTrusted + ";";

                DBConAgnostic = new SqlConnection(StriConn);
                DBConAgnostic.Open();
                //Add a la coleccione de Conexxiones
                if (!HashConn.ContainsKey(this._TypeConnection))
                    HashConn.Add(this._TypeConnection, DBConAgnostic);

                if (DBCommandToStoreProcedure == null)
                {
                    InstanceDBCommandToStoreProcedure();
                }
                else
                {
                    DBCommandToTextDirect.Connection = DBConAgnostic;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void InstanceConnectionMaster()
        {
            string sDbServer = ConfigurationManager.AppSettings["dbserver"];
            string sDbCatalog = ConfigurationManager.AppSettings["dbcatalog"];
            string sDbUser = ConfigurationManager.AppSettings["dbuser"];
            string sDbPassword = ConfigurationManager.AppSettings["dbpassword"];
            string sTrusted = ConfigurationManager.AppSettings["trusted"];
            string StriConnAux = ConfigurationManager.AppSettings["StriConnAux"];

            ////    string StriConn = @"Data Source=" + sDbServer + ";" +
            ////"Initial Catalog=" + sDbCatalog + ";Integrated Security=SSPI;MultipleActiveResultSets=true;";// + 
            ////    //"User ID=" + sDbUser + ";" + 
            ////    //"Password=" + sDbPassword + ";"; //Trusted_Connection= " + sTrusted + ";";
            string StriConn = StriConnAux;



            try
            {
                DBConAgnostic = new SqlConnection(StriConn);
                DBConAgnostic.Open();
                //Add a la coleccione de Conexxiones
                if (!HashConn.ContainsKey(eeTypeConnection.eBDMaster.ToString()))
                    HashConn.Add(eeTypeConnection.eBDMaster.ToString(), DBConAgnostic);
                DBConMaster = DBConAgnostic;

                //Se instancian los command
                if (DBCommandToStoreProcedure == null) InstanceDBCommandToStoreProcedure();
                if (DBCommandToTextDirect == null) InstanceDBCommandToTextDirect();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void InstanceDBCommandToTextDirect()
        {
            DBCommandToTextDirect = new SqlCommand();
            DBCommandToTextDirect.CommandType = CommandType.Text;
            DBCommandToTextDirect.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["CommandTimeoutExecuteBD"].ToString());
        }

        private void InstanceDBCommandToStoreProcedure(CommandType Type, ref SqlCommand Com, SqlConnection DBConAgnostic)
        {
            Com = new SqlCommand();
            Com.CommandType = Type;
            Com.Connection = DBConAgnostic;
            Com.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["CommandTimeoutExecuteBD"].ToString());
        }

        private void InstanceDBCommandToStoreProcedure()
        {
            DBCommandToStoreProcedure = new SqlCommand();
            DBCommandToStoreProcedure.CommandType = CommandType.StoredProcedure;
            DBCommandToStoreProcedure.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["CommandTimeoutExecuteBD"].ToString());
        }

        public void ExecuteSqlCommand(SqlCommand cmd)
        {
            cmd.Connection = DBConMaster;
            if (DBConMaster.State == ConnectionState.Closed)
                DBConMaster.Open();
            cmd.ExecuteNonQuery();
            DBConMaster.Close();
        }

        void ExecuteCommand(ArrayList Parametros, string SQL)
        {
            if (HashConn.ContainsKey(_TypeConnection))
                DBConAgnostic = HashConn[_TypeConnection];
            else
                if (_TypeConnection == eeTypeConnection.eBDMaster.ToString())
                //Se instancia MasterDB
                InstanceConnectionMaster();
            else
                InstanceConnectionClient();

            SqlCommand cmd = new SqlCommand(SQL);
            cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["CommandTimeoutExecuteBD"].ToString());
            foreach (SqlParameter item in Parametros)
            {
                cmd.Parameters.Add(item);
            }

            cmd.Connection = DBConAgnostic;
            if (DBConAgnostic.State == ConnectionState.Closed) DBConAgnostic.Open();
            cmd.ExecuteNonQuery();
            DBConAgnostic.Close();
        }

        public void TestConnection()
        {
            try
            {
                string StringConnection = ConfigurationManager.AppSettings["StriConnAux"];
                SqlConnection Connection = new SqlConnection(StringConnection);
                Connection.Open();
                Connection.Close();
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        internal static string FormatDecimal(object Numero)
        {
            double num = 0;
            double.TryParse(Numero.ToString(), out num);
            return num.ToString();
        }
        internal string ExecuteSQLScalarStringSQLDirect(string NameSP, ArrayList Parameters)
        {
            string Result = "";
            try
            {
                DataSet DS = new DataSet();
                //Chequea la conexion para el usuario
                SetConexionActivaPreExecutionConsulta();

                if (DBCommandExecuteNonQuery == null) InstanceDBCommandToStoreProcedure(CommandType.Text, ref DBCommandExecuteNonQuery, DBConAgnostic);
                if (DBConAgnostic.State == ConnectionState.Closed) DBConAgnostic.Open();

                DBCommandExecuteNonQuery.Parameters.Clear();
                for (int i = 0; i < Parameters.Count; i++)
                {
                    DBCommandExecuteNonQuery.Parameters.Add(Parameters[i]);
                }
                DBCommandExecuteNonQuery.CommandType = CommandType.Text;
                DBCommandExecuteNonQuery.CommandText = NameSP;
                if (DBConAgnostic.State == ConnectionState.Closed) DBConAgnostic.Open();
                DBCommandExecuteNonQuery.ExecuteScalar();
                if (((SqlParameter)Parameters[Parameters.Count - 1]).Value != DBNull.Value)
                    Result = ((SqlParameter)Parameters[Parameters.Count - 1]).Value.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Result;
        }


        internal string ExecuteSQLScalarString(string NameSP, ArrayList Parameters)
        {
            string Result = "";
            try
            {
                DataSet DS = new DataSet();
                //Chequea la conexion para el usuario
                SetConexionActivaPreExecutionConsulta();

                if (DBCommandExecuteNonQuery == null) InstanceDBCommandToStoreProcedure(CommandType.Text, ref DBCommandExecuteNonQuery, DBConAgnostic);
                if (DBConAgnostic.State == ConnectionState.Closed) DBConAgnostic.Open();

                DBCommandExecuteNonQuery.Parameters.Clear();
                for (int i = 0; i < Parameters.Count; i++)
                {
                    DBCommandExecuteNonQuery.Parameters.Add(Parameters[i]);
                }
                DBCommandExecuteNonQuery.CommandType = CommandType.StoredProcedure;
                DBCommandExecuteNonQuery.CommandText = NameSP;
                if (DBConAgnostic.State == ConnectionState.Closed) DBConAgnostic.Open();
                DBCommandExecuteNonQuery.ExecuteScalar();
                if (((SqlParameter)Parameters[Parameters.Count - 1]).Value != DBNull.Value)
                    Result = ((SqlParameter)Parameters[Parameters.Count - 1]).Value.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Result;
        }

        internal double ExecuteSQLScalar(string NameSP, ArrayList Parameters)
        {
            double Result = 0;
            try
            {
                DataSet DS = new DataSet();
                //Chequea la conexion para el usuario
                SetConexionActivaPreExecutionConsulta();

                if (DBCommandExecuteNonQuery == null) InstanceDBCommandToStoreProcedure(CommandType.Text, ref DBCommandExecuteNonQuery, DBConAgnostic);
                if (DBConAgnostic.State == ConnectionState.Closed) DBConAgnostic.Open();

                DBCommandExecuteNonQuery.Parameters.Clear();
                for (int i = 0; i < Parameters.Count; i++)
                {
                    DBCommandExecuteNonQuery.Parameters.Add(Parameters[i]);
                }
                DBCommandExecuteNonQuery.CommandType = CommandType.StoredProcedure;
                DBCommandExecuteNonQuery.CommandText = NameSP;
                if (DBConAgnostic.State == ConnectionState.Closed) DBConAgnostic.Open();
                DBCommandExecuteNonQuery.ExecuteScalar();
                //Result = double.Parse(DBCommandExecuteNonQuery.ExecuteScalar().ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Result;
        }

        public string GetStringConnection()
        {
            if (this.StriConnGlobal.Trim().Length == 0)
            {
                //string appSetting1 = ConfigurationManager.AppSettings["dbserver"];
                //string appSetting2 = ConfigurationManager.AppSettings["dbcatalog"];
                //string appSetting3 = ConfigurationManager.AppSettings["dbuser"];
                //string appSetting4 = ConfigurationManager.AppSettings["dbpassword"];
                //string appSetting5 = ConfigurationManager.AppSettings["trusted"];
                //this.StriConnGlobal = "Data Source=" + appSetting1 + ";Initial Catalog=" + appSetting2 + ";Integrated Security=SSPI;";
                this.StriConnGlobal = ConfigurationManager.AppSettings["StriConnAux"];
                //string StriConn = StriConnAux;
            }
            return this.StriConnGlobal;
        }
    }

}
