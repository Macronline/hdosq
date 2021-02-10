using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data;
using H2Q.BC.DataAccess;
using H2Q.BC.Security;

namespace H2Q.BC.Base
{
    [Serializable]
    public class Singular
    {
        public bool ForCreate = true;
        public bool ForUpdate = false;
        public bool ForDelete = false;
        public Usuario User = null;
        public DALCSQLServer DALC = null;
        public DataTable Datos = null;
        public DataRowGIA DREntity;

        public Singular()
        {
            this.ForCreate = true;
            this.ForUpdate = false;
            this.ForDelete = false;
            this.User = null;
            this.DALC = null;
            this.Datos =null;
            this.DREntity = new DataRowGIA(this);
        }

        public DALCSQLServer GetCommonDalc()
        {
            return new DALCSQLServer(this.User);
        }

        public void SetUser(Usuario User)
        {
            this.User = User;
        }

        public DataRow GetDataRow(string ColumnFilter, string ValueFilter)
        {
            DataRow DR = null;
            if (this.Datos != null)
            {
                this.Datos.DefaultView.RowFilter = String.Format("{0}='{1}'", ColumnFilter, ValueFilter);
                if (this.Datos.DefaultView.Count == 1)
                {
                    DR = this.Datos.DefaultView[0].Row;
                }
                this.Datos.DefaultView.RowFilter = "";
            }
            return DR;
        }
        public DataRow GetDataRow(int Index)
        {
            DataRow DR = null;
            if (this.Datos != null && Index < this.Datos.Rows.Count)
            {
                DR = this.Datos.DefaultView[Index].Row;
            }
            return DR;
        }
        public string GetAttribute(string Name)
        {
            return this.Datos.Rows[0][Name].ToString();
        }

        public void AddNewRow()
        {
            DataRow DR = this.Datos.NewRow();
            for (int i = 0; i < this.Datos.Columns.Count; i++)
            {
                if (this.Datos.Columns[i].DataType.Name == "Int64") DR[i] = DBNull.Value;
                if (this.Datos.Columns[i].DataType.Name == "String") DR[i] = DBNull.Value;
                if (this.Datos.Columns[i].DataType.Name == "DateTime") DR[i] = DBNull.Value;
                if (this.Datos.Columns[i].DataType.Name == "Boolean") DR[i] = true;
            }
            this.Datos.Rows.Add(DR);
            this.DREntity.SetDataRow(DR.ItemArray);
            this.DREntity.RowState = DataRowState.Added;

        }

        public void AddNewRowToFirst()
        {
            DataRow row = this.Datos.NewRow();
            for (int index = 0; index < this.Datos.Columns.Count; ++index)
            {
                if (this.Datos.Columns[index].DataType.Name == "Int64")
                    row[index] = DBNull.Value;
                if (this.Datos.Columns[index].DataType.Name == "String")
                    row[index] = DBNull.Value;
                if (this.Datos.Columns[index].DataType.Name == "DateTime")
                    row[index] = DBNull.Value;
                if (this.Datos.Columns[index].DataType.Name == "Boolean")
                    row[index] = true;
            }
            this.Datos.Rows.InsertAt(row, 0);
            this.DREntity.SetDataRow(row.ItemArray);
            this.DREntity.RowState = DataRowState.Added;
        }

    }
}
