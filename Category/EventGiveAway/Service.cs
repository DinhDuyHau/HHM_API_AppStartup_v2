using Genbyte.Base.CoreLib;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventGiveAway.Model;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace EventGiveAway
{
    public class Service : CoreService
    {
        public EntityCollection<EventGiveAwayModel> GetEventGiveAway(string ma_cuahang, string ma_sukien, string ten_sukien, int pageIndex, int pageSize)
        {
            string sql = "exec Genbyte$Category$GetEventGiveAway @ma_cuahang, @ma_sukien, @ten_sukien, @pageIndex, @pageSize";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_cuahang",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_cuahang
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_sukien",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_sukien
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ten_sukien",
                SqlDbType = SqlDbType.NVarChar,
                Value = ten_sukien
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@pageIndex",
                SqlDbType = SqlDbType.Int,
                Value = pageIndex
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@pageSize",
                SqlDbType = SqlDbType.Int,
                Value = pageSize
            });
            DataSet ds = this.ExecSql2DataSet(sql, paras);
            EntityCollection<EventGiveAwayModel> entity = ds.Tables[0].ToList<EntityCollection<EventGiveAwayModel>>().FirstOrDefault();
            entity.Items = (List<EventGiveAwayModel>)ds.Tables[1].ToList<EventGiveAwayModel>();
            return entity;
        }
        public EventGiveAwayModel GetEventGiveAwayById (string ma_sukien)
        {
            string sql = $"select * from dmsukien where ma_sukien = '{ma_sukien}'";
            return base.ExecSql2List<EventGiveAwayModel>(sql).ToList().FirstOrDefault();
        }
        public List<EventGiveAwayDetailModel> GetEventGiveAwayDetail(string ma_sukien)
        {
            string sql = $"select * from dmsukienct where ma_sukien = '{ma_sukien}'";
            return base.ExecSql2List<EventGiveAwayDetailModel>(sql).ToList();
        }
    }
}
