using Genbyte.Sys.AppAuth.Entities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imei
{
    public class Converter
    {
        public static List<Dictionary<string, object>> TableToDictionary(DataTable table)
        {
            List<Dictionary<string, object>> dic_list = new List<Dictionary<string, object>>();
            if (table == null)
                return dic_list;

            foreach (DataRow dr in table.Rows)
            {
                Dictionary<string, object> dicRow = new Dictionary<string, object>();
                foreach (DataColumn col in dr.Table.Columns)
                {
                    dicRow.Add(col.ColumnName, dr[col]);
                }
                dic_list.Add(dicRow);
            }

            return dic_list;
        }
    }
}
