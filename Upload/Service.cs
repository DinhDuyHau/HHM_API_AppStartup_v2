using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Upload.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using Genbyte.Base.CoreLib;
using System.Data.SqlClient;
using System.Data;
using Genbyte.Sys.Common;

namespace Upload
{
    public class Service : CoreService
    {
        private readonly FileService _fileService;
        public Service()
        {
            _fileService = new FileService();
        }
        public string saveImageCustomer(CustomerUpload customer)
        {
            if (!IsSQLInjectionValid(customer.ma_kh) && !IsSQLInjectionValid(customer.image.FileName))
                throw new CustomException(ApiReponseMessage.Error_InputData);

            if (customer.image != null && customer.image.ContentType != "image/jpeg" && customer.image.ContentType != "image/png")
                throw new CustomException("incorrect_file_format");

            //Thực hiện lấy thông tin của khách hàng gửi lên
            string sql = "select ma_kh, image from dmkh where ma_kh = @ma_kh";
            List<SqlParameter> paras = new List<SqlParameter>();

            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_kh",
                SqlDbType = SqlDbType.Char,
                Value = customer.ma_kh
            });

            var dtSet = this.ExecSql2DataSet(sql, paras);
            if (dtSet == null || dtSet.Tables.Count <= 0 || dtSet.Tables[0].Rows.Count <= 0) throw new CustomException("customer_not_exsits");
            string oldPath = dtSet.Tables[0].Rows[0]["image"].ToString();



            string folder = "/image/customer";
            string newPath = folder + "/" + customer.image.FileName;
            // Thực hiện thay đổi kích thước ảnh và lưu lại ảnh
            _fileService.ResizeImage(customer.image, folder, 1024, 768);

            // Lưu thành công thực hiện xóa ảnh cũ
            if (oldPath != "" && oldPath != null && oldPath != newPath)
            {
                oldPath = _fileService.getRootPath() + "/" + oldPath;
                if (File.Exists(oldPath))
                {
                    File.Delete(oldPath);
                }
            }
            //Cập nhật lại ảnh trong danh mục khách hàng
            sql = "update dmkh set image = @image where ma_kh = @ma_kh";
            paras = new List<SqlParameter>();

            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_kh",
                SqlDbType = SqlDbType.Char,
                Value = customer.ma_kh
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@image",
                SqlDbType = SqlDbType.NVarChar,
                Value = newPath
            });

            this.ExecuteNonQuery(sql, paras);

            return newPath;
        }
        public object saveWholesaleContract(WholeSaleUpload saleUpload)
        {
            // Check
            if (!IsSQLInjectionValid(saleUpload.so_ct) && !IsSQLInjectionValid(saleUpload.cq_file.FileName) && !IsSQLInjectionValid(saleUpload.co_file.FileName))
                throw new CustomException(ApiReponseMessage.Error_InputData);

            if (saleUpload.co_file != null && saleUpload.co_file.ContentType != "image/jpeg" && saleUpload.co_file.ContentType != "image/png"
                   && saleUpload.co_file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                   && saleUpload.co_file.ContentType != "application/pdf")
                throw new CustomException("incorrect_file_format");
            if (saleUpload.cq_file != null && saleUpload.cq_file.ContentType != "image/jpeg" && saleUpload.cq_file.ContentType != "image/png"
                   && saleUpload.cq_file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                   && saleUpload.cq_file.ContentType != "application/pdf")
                throw new CustomException("incorrect_file_format");

            string wholeSaleTable = "m592img$";
            string expression = saleUpload.ngay_ct.ToString("yyyyMM");
            wholeSaleTable += expression;

            // Thực hiện lưu file 
            string folder = "/file/WholeSale";
            _fileService.SaveFile(folder, saleUpload.co_file);
            _fileService.SaveFile(folder, saleUpload.cq_file);


            // Thực hiện lấy path file cũ về
            string sql = $"select so_ct, cq_file, co_file from {wholeSaleTable} where so_ct = '{saleUpload.so_ct}'";
            DataSet dtSet = this.ExecSql2DataSet(sql);

            //Nếu không có thì thêm mới
            if (dtSet == null || dtSet.Tables.Count <= 0 || dtSet.Tables[0].Rows.Count <= 0)
            {
                string co_file = "";
                string cq_file = "";
                if (saleUpload.cq_file != null)
                {
                    cq_file = folder + "/" + saleUpload.cq_file.FileName;
                }
                if (saleUpload.co_file != null)
                {
                    co_file = folder + "/" + saleUpload.co_file.FileName;
                }
                sql = $"insert into {wholeSaleTable} (stt_rec, ma_ct, ngay_ct, so_ct, stt_rec0, co_file, cq_file) " +
                    $"values ('{saleUpload.so_ct}', '{"BHB"}', '{saleUpload.ngay_ct.ToString("yyyy-MM-dd")}', '{saleUpload.so_ct}', '{"001"}', '{co_file}', '{cq_file}')";
                this.ExecuteNonQuery(sql);
                return new
                {
                    co_file = co_file,
                    cq_file = cq_file,
                };
            }
            else
            {
                string oldCQPath = dtSet.Tables[0].Rows[0]["cq_file"].ToString();
                string oldCOPath = dtSet.Tables[0].Rows[0]["co_file"].ToString();
                if (saleUpload.cq_file != null && oldCQPath != null && oldCQPath != "" && oldCQPath != folder + "/" + saleUpload.cq_file.FileName)
                {
                    oldCQPath = _fileService.getRootPath() + "/" + oldCQPath;
                    if (File.Exists(oldCQPath))
                    {
                        File.Delete(oldCQPath);
                    }
                }

                if (saleUpload.co_file != null && oldCOPath != null && oldCOPath != "" && oldCOPath != folder + "/" + saleUpload.co_file.FileName)
                {
                    oldCOPath = _fileService.getRootPath() + "/" + oldCOPath;
                    if (File.Exists(oldCOPath))
                    {
                        File.Delete(oldCOPath);
                    }
                }
                string newCQPath = "";
                string newCOPath = "";
                //Cập nhật lại path vào database
                if (saleUpload.cq_file != null && saleUpload.co_file != null)
                {
                    newCQPath = folder + "/" + saleUpload.cq_file.FileName;
                    newCOPath = folder + "/" + saleUpload.co_file.FileName;
                    sql = $"update {wholeSaleTable} set co_file = '{newCOPath}', cq_file = '{newCQPath}' where so_ct = '{saleUpload.so_ct}'";
                }
                else if (saleUpload.cq_file != null)
                {
                    newCQPath = folder + "/" + saleUpload.cq_file.FileName;
                    newCOPath = oldCOPath;
                    sql = $"update {wholeSaleTable} set cq_file = '{newCQPath}' where so_ct = '{saleUpload.so_ct}'";
                }
                else if (saleUpload.co_file != null)
                {
                    newCOPath = folder + "/" + saleUpload.co_file.FileName;
                    newCQPath = oldCQPath;
                    sql = $"update {wholeSaleTable} set co_file = '{newCOPath}' where so_ct = '{saleUpload.so_ct}'";
                }
                this.ExecuteNonQuery(sql);
                return new
                {
                    co_file = newCQPath,
                    cq_file = newCOPath,
                };
            }
        }
        public void saveWholesaleContract2(WholeSaleUpload2 saleUpload)
        {
            if (!IsSQLInjectionValid(saleUpload.stt_rec) && !IsSQLInjectionValid(saleUpload.cq_file.FileName) && !IsSQLInjectionValid(saleUpload.co_file.FileName))
                throw new CustomException(ApiReponseMessage.Error_InputData);

            if (saleUpload.co_file != null && saleUpload.co_file.ContentType != "image/jpeg" && saleUpload.co_file.ContentType != "image/png"
                   && saleUpload.co_file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                   && saleUpload.co_file.ContentType != "application/pdf")
                throw new CustomException("incorrect_file_format");
            if (saleUpload.cq_file != null && saleUpload.cq_file.ContentType != "image/jpeg" && saleUpload.cq_file.ContentType != "image/png"
                   && saleUpload.cq_file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                   && saleUpload.cq_file.ContentType != "application/pdf")
                throw new CustomException("incorrect_file_format");

            string wholeSaleTable = "m592img$";
            string expression = saleUpload.ngay_ct.ToString("yyyyMM");
            wholeSaleTable += expression;

            // Thực hiện lấy path file cũ về
            string sql = $"select stt_rec, cq_file, co_file from {wholeSaleTable} where stt_rec = '{saleUpload.stt_rec}'";
            DataSet dtSet = this.ExecSql2DataSet(sql);
            if (dtSet == null || dtSet.Tables.Count <= 0 || dtSet.Tables[0].Rows.Count <= 0) throw new CustomException(ApiReponseMessage.Error_notExist);
            string oldCQPath = dtSet.Tables[0].Rows[0]["cq_file"].ToString();
            string oldCOPath = dtSet.Tables[0].Rows[0]["co_file"].ToString();

            string folder = "/file/WholeSale";
            _fileService.SaveFile(folder, saleUpload.co_file);
            _fileService.SaveFile(folder, saleUpload.cq_file);

            if (saleUpload.cq_file != null && oldCQPath != null && oldCQPath != "" && oldCQPath != folder + "/" + saleUpload.cq_file.FileName)
            {
                oldCQPath = _fileService.getRootPath() + "/" + oldCQPath;
                if (File.Exists(oldCQPath))
                {
                    File.Delete(oldCQPath);
                }
            }

            if (saleUpload.co_file != null && oldCOPath != null && oldCOPath != "" && oldCOPath != folder + "/" + saleUpload.co_file.FileName)
            {
                oldCOPath = _fileService.getRootPath() + "/" + oldCOPath;
                if (File.Exists(oldCOPath))
                {
                    File.Delete(oldCOPath);
                }
            }

            //Cập nhật lại ảnh trong danh mục khách hàng
            if (saleUpload.cq_file != null && saleUpload.co_file != null)
            {
                string newCQPath = folder + "/" + saleUpload.cq_file.FileName;
                string newCOPath = folder + "/" + saleUpload.co_file.FileName;
                sql = $"update {wholeSaleTable} set co_file = '{newCOPath}', cq_file = '{newCQPath}' where stt_rec = '{saleUpload.stt_rec}'";
            }
            else if (saleUpload.cq_file != null)
            {
                string newCQPath = folder + "/" + saleUpload.cq_file.FileName;
                sql = $"update {wholeSaleTable} set cq_file = '{newCQPath}' where stt_rec = '{saleUpload.stt_rec}'";
            }
            else if (saleUpload.co_file != null)
            {
                string newCOPath = folder + "/" + saleUpload.co_file.FileName;
                sql = $"update {wholeSaleTable} set co_file = '{newCOPath}' where stt_rec = '{saleUpload.stt_rec}'";
            }

            this.ExecuteNonQuery(sql);
        }
        public void deleteWholesaleContract(string so_ct, DateTime ngay_ct)
        {
            if (!IsSQLInjectionValid(so_ct))
                throw new CustomException(ApiReponseMessage.Error_InputData);
            string wholeSaleTable = "m592img$";
            string expression = ngay_ct.ToString("yyyyMM");
            wholeSaleTable += expression;

            // Thực hiện lấy path file cũ về
            string sql = $"select so_ct, cq_file, co_file from {wholeSaleTable} where so_ct = '{so_ct}'";
            DataSet dtSet = this.ExecSql2DataSet(sql);
            if (dtSet == null || dtSet.Tables.Count <= 0 || dtSet.Tables[0].Rows.Count <= 0) throw new CustomException(ApiReponseMessage.Error_notExist);
            string oldCQPath = dtSet.Tables[0].Rows[0]["cq_file"].ToString();
            string oldCOPath = dtSet.Tables[0].Rows[0]["co_file"].ToString();

            string folder = "/file/WholeSale";

            if (oldCQPath != null && oldCQPath != "")
            {
                oldCQPath = _fileService.getRootPath() + "/" + oldCQPath;
                if (File.Exists(oldCQPath))
                {
                    File.Delete(oldCQPath);
                }
            }

            if (oldCOPath != null && oldCOPath != "")
            {
                oldCOPath = _fileService.getRootPath() + "/" + oldCOPath;
                if (File.Exists(oldCOPath))
                {
                    File.Delete(oldCOPath);
                }
            }

            // Thực hiện xóa dữ liệu trong bảng 
            sql = $"delete {wholeSaleTable} where so_ct = '{so_ct}'";
            this.ExecuteNonQuery(sql);
        }
    }
}