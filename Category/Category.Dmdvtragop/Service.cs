using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Genbyte.Component.Category;
using Genbyte.Sys.Common;
using Genbyte.Sys.Common.Models;

namespace Category.Dmdvtragop
{
    public class Service : IComponentService
    {
        public AccessRight CateRight { get; set; }

        public Service()
        {
            CateRight = new AccessRight();
            CateRight.AllowRead = true;
            CateRight.AllowCreate = true;
            CateRight.AllowUpdate = true;
            CateRight.AllowDelete = false;

        }

        #region CREATE
        /// <summary>
        /// Thực hiện trước khi dữ liệu được insert vào db (check dữ liệu hợp lệ, check tồn tại khóa chính,...)
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true: cho phép thực hiện insert</returns>
        public bool Inserting(Dictionary<string, object> data)
        {
            //Khởi tạo service xử lý dữ liệu
            DataService data_service = new DataService();

            //convert dữ liệu đầu vào sang thực thể theo khai báo của danh mục
            EntityItem entity_item = Converter.DictionaryToObject<EntityItem>(data);

            //check null
            if (entity_item == null)
                throw new Exception(ApiReponseMessage.isNullResult);

            //check sql injection
            if (!data_service.IsSQLInjectionValid(entity_item.ma_dvtg))
                throw new Exception(ApiReponseMessage.Error_InputData);

            //check tồn tại dữ liệu trong db theo khóa chính
            string sql = "select 1 from dmdvtragop where ma_dvtg = @ma_dvtg";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_dvtg",
                SqlDbType = SqlDbType.Char,
                Value = entity_item.ma_dvtg
            });
            DataSet ds = data_service.ExecSql2DataSet(sql, paras);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                return false;

            //hợp lệ => trả về true
            return true;
        }

        /// <summary>
        /// Thực hiện sau khi dữ liệu đã được insert vào db
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>giá trị result là object đã được insert</returns>
        public CommonObjectModel Inserted(object entity)
        {
            return new CommonObjectModel()
            {
                message = "",
                success = true,
                result = entity
            };
        }
        #endregion

        #region READ

        /**
         * Lấy toàn bộ dữ liệu danh mục (không phân trang)
         * entities: input object có type là IList<T>
         */
        public CommonObjectModel GetAll(object entities, string order_by = "")
        {
            //Có thể thực hiện xử lý dữ liệu đã lấy từ db tại backend trước khi trả về client
            //IList<EntityItem> items = (IList<EntityItem>)entities;

            CommonObjectModel result = new CommonObjectModel()
            {
                message = "",
                success = true,
                result = entities
            };
            return result;
        }

        /** 
         * Lấy top bản ghi của danh mục (không phân trang) 
         * entities: input object có type là IList<T>
         */
        public CommonObjectModel GetTop(object entities, string order_by = "", int row_number = 1)
        {
            //Có thể thực hiện xử lý dữ liệu đã lấy từ db tại backend trước khi trả về client
            //code here...

            CommonObjectModel result = new CommonObjectModel()
            {
                message = "",
                success = true,
                result = entities
            };
            return result;
        }

        /** 
         * Lấy danh sách dữ liệu của danh mục có xử lý phân trang
         * entities: input object có type là EntityCollection<T>
         */
        public CommonObjectModel GetByPaging(object entities, string order_by = "", int page_index = 1, int page_size = 0)
        {
            //Có thể thực hiện xử lý dữ liệu đã lấy từ db tại backend trước khi trả về client
            //code here...

            CommonObjectModel result = new CommonObjectModel()
            {
                message = "",
                success = true,
                result = entities
            };
            return result;
        }

        /** 
         * Tìm kiếm dữ liệu của danh mục theo điều kiện lọc từ client (có phân trang)
         * entities: input object có type là EntityCollection<T>
         */
        public CommonObjectModel Find(object entities, List<FieldModel> filter, string order_by = "", int page_index = 1, int page_size = 0)
        {
            //Có thể thực hiện xử lý dữ liệu đã lấy từ db tại backend trước khi trả về client
            //code here...

            CommonObjectModel result = new CommonObjectModel()
            {
                message = "",
                success = true,
                result = entities
            };
            return result;
        }

        /** 
         * Lấy dữ liệu của danh mục theo mã 
         * entities: input object có type là T
         */
        public CommonObjectModel GetById(object entity, FieldModel filter)
        {
            CommonObjectModel result = new CommonObjectModel()
            {
                message = "",
                success = true,
                result = entity
            };
            return result;
        }

        #endregion

        #region UPDATE

        /// <summary>
        /// Thực hiện trước khi xử lý update dữ liệu vào db (check dữ liệu hợp lệ, check tồn tại khóa chính, check toàn vẹn dữ liệu...)
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true: cho phép thực hiện update</returns>
        public bool Updating(Dictionary<string, object> data)
        {
            bool is_valid = false;

            //kiểm tra tồn tại record theo khóa chính
            EntityItem entity_item = Converter.DictionaryToObject<EntityItem>(data);
            if (entity_item == null)
                return false;

            DataService db_service = new DataService();
            string sql = "select * from dmdvtragop where ma_dvtg = @ma_dvtg";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_dvtg",
                SqlDbType = SqlDbType.Char,
                Value = entity_item.ma_dvtg
            });
            List<EntityItem> check_list = db_service.ExecSql2List<EntityItem>(sql, paras);
            is_valid = check_list != null && check_list.Count > 0;

            return is_valid;
        }

        /// <summary>
        /// Thực hiện sau khi đã update dữ liệu vào db
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>giá trị result là object đã được update</returns>
        public CommonObjectModel Updated(object entity)
        {
            return new CommonObjectModel()
            {
                message = "",
                success = true,
                result = entity
            };
        }

        #endregion

        #region DELETE

        /// <summary>
        /// Thực hiện trước khi dữ liệu được xóa tại db (check ràng buộc dữ liệu,...)
        /// </summary>
        /// <returns>true: cho phép thực hiện xóa, false: không cho phép thực hiện xóa (kèm theo message thông báo)</returns>
        public CommonObjectModel Deleting(List<FieldModel> model)
        {
            return new CommonObjectModel()
            {
                message = "",
                success = true,
                result = null
            };
        }

        /// <summary>
        /// Thực hiện sau khi dữ liệu đã được xóa tại db
        /// </summary>
        /// <returns>true: xóa thành công, false: không thực hiện xóa (kèm theo message thông báo)</returns>
        public CommonObjectModel Deleted(List<FieldModel> model)
        {
            return new CommonObjectModel()
            {
                message = "",
                success = true,
                result = null
            };
        }

        #endregion
    }
}
