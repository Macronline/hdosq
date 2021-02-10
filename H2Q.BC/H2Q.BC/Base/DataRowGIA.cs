using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data;
using H2Q.BC.Base;
using H2Q.BC.DataAccess;

namespace H2Q.BC.Base
{
    [Serializable]
    public class DataRowGIA
    {
        public DataTable DT;
        public Singular ObjParent;
        public DataRowState RowState;

        public DataRowGIA(Singular type)
        {
            this.DT = (DataTable)null;
            this.ObjParent = (Singular)null;
            this.RowState = DataRowState.Unchanged;
//            base.\u002Ector();
            this.ObjParent = type;
        }

        public object this[string columnName]
        {
            get
            {
                return this.DT.Rows[0][columnName];
            }
            set
            {
                this.DT.Rows[0][columnName] = value;
            }
        }

        public object this[int columnIndex]
        {
            get
            {
                return this.DT.Rows[0][columnIndex];
            }
            set
            {
                this.DT.Rows[0][columnIndex] = value;
            }
        }

        public void SetDataRow(object[] itemArray)
        {
            if (this.DT != null)
            {
                if (this.DT.Columns.Count != itemArray.Length && this.ObjParent.Datos != null)
                    this.DT = this.ObjParent.Datos.Clone();
            }
            else if (this.ObjParent.Datos != null)
                this.DT = this.ObjParent.Datos.Clone();
            if (this.DT.Rows.Count == 0)
                this.DT.Rows.Add(this.DT.NewRow());
            this.DT.Rows[0].ItemArray = itemArray;
        }

        public object[] ItemArray
        {
            get
            {
                return this.DT.Rows[0].ItemArray;
            }
            set
            {
                if (this.DT == null)
                    this.DT = this.ObjParent.Datos == null ? new DataExplorer().GetDataTableModelEmpty(this.ObjParent) : this.ObjParent.Datos.Clone();
                if (this.DT.Columns.Count != value.Length && this.ObjParent.Datos != null)
                    this.DT = this.ObjParent.Datos.Clone();
                if (this.DT.Rows.Count == 0)
                    this.DT.Rows.Add(this.DT.NewRow());
                this.DT.Rows[0].ItemArray = value;
                if (this.DT.Rows[0][0] == DBNull.Value)
                    this.RowState = DataRowState.Added;
                else
                    this.RowState = DataRowState.Unchanged;
            }
        }
    }
}
