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
using Microsoft.VisualBasic.FileIO;

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
            DateTime now = DateTime.Now;
            string fileName = customer.ma_kh.Trim() + "-" + $"{now.Hour:D2}{now.Minute:D2}{now.Second:D2}" + _fileService.getExtension(customer.image);
            string newPath = folder + "/" + fileName;
            // Thực hiện thay đổi kích thước ảnh và lưu lại ảnh
            _fileService.ResizeImage(customer.image, folder, 1024, 768, fileName);

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
        #region WholesaleContract
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
            DateTime now = DateTime.Now;
            string file_name_co = saleUpload.so_ct.Trim() + "-co_file-" + $"{now.Hour:D2}{now.Minute:D2}{now.Second:D2}" + _fileService.getExtension(saleUpload.co_file);
            string file_name_cq = saleUpload.so_ct.Trim() + "-cq_file-" + $"{now.Hour:D2}{now.Minute:D2}{now.Second:D2}" + _fileService.getExtension(saleUpload.cq_file);
            _fileService.SaveFile(folder, saleUpload.co_file, file_name_co);
            _fileService.SaveFile(folder, saleUpload.cq_file, file_name_cq);


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
                    cq_file = folder + "/" + file_name_cq;
                }
                if (saleUpload.co_file != null)
                {
                    co_file = folder + "/" + file_name_co;
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
                if (saleUpload.cq_file != null && oldCQPath != null && oldCQPath != "")
                {
                    oldCQPath = _fileService.getRootPath() + "/" + oldCQPath;
                    if (File.Exists(oldCQPath))
                    {
                        File.Delete(oldCQPath);
                    }
                }

                if (saleUpload.co_file != null && oldCOPath != null && oldCOPath != "")
                {
                    oldCOPath = _fileService.getRootPath() + "/" + oldCOPath;
                    if (File.Exists(oldCOPath))
                    {
                        File.Delete(oldCOPath);
                    }
                }
                //Cập nhật lại path vào database
                if (saleUpload.cq_file != null && saleUpload.co_file != null)
                {
                    sql = $"update {wholeSaleTable} set co_file = '{folder + "/" + file_name_co}', cq_file = '{folder + "/" + file_name_cq}' where so_ct = '{saleUpload.so_ct}'";
                }
                else if (saleUpload.cq_file != null)
                {
                    file_name_co = oldCOPath;
                    sql = $"update {wholeSaleTable} set cq_file = '{folder + "/" + file_name_cq}' where so_ct = '{saleUpload.so_ct}'";
                }
                else if (saleUpload.co_file != null)
                {
                    file_name_cq = oldCQPath;
                    sql = $"update {wholeSaleTable} set co_file = '{folder + "/" + file_name_co}' where so_ct = '{saleUpload.so_ct}'";
                }
                this.ExecuteNonQuery(sql);
                return new
                {
                    co_file = folder + "/" + file_name_co,
                    cq_file = folder + "/" + file_name_cq,
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
        #endregion
        #region EventGift
        public object saveEventGift(EventGiftUpload saleUpload)
        {
            // Check
            if (!IsSQLInjectionValid(saleUpload.so_ct) && !IsSQLInjectionValid(saleUpload.image.FileName))
                throw new CustomException(ApiReponseMessage.Error_InputData);

            if (saleUpload.image != null && saleUpload.image.ContentType != "image/jpeg" && saleUpload.image.ContentType != "image/png")
                throw new CustomException("incorrect_file_format");

            string EventGiftTable = "m588img$";
            string expression = saleUpload.ngay_ct.ToString("yyyyMM");
            string extension = Path.GetExtension(saleUpload.image.FileName);
            if(extension == "")
            {
                try
                {
                    extension = saleUpload.image.ContentType.Split('/')[1];
                }catch(Exception ex)
                {
                    extension = ".png";
                }
            }
            DateTime now = DateTime.Now;
            string fileName = saleUpload.so_ct +"-"+ $"{now.Hour:D2}{now.Minute:D2}{now.Second:D2}" + extension;
            EventGiftTable += expression;

            // Thực hiện lưu file 
            string folder = "/file/EventGift";
            _fileService.SaveFile(folder, saleUpload.image, fileName);

            // Thực hiện lấy path file cũ về
            string sql = $"select so_ct, image from {EventGiftTable} where so_ct = '{saleUpload.so_ct}'";
            DataSet dtSet = this.ExecSql2DataSet(sql);

            //Nếu không có thì thêm mới
            if (dtSet == null || dtSet.Tables.Count <= 0 || dtSet.Tables[0].Rows.Count <= 0)
            {
                string file = "";
                if (saleUpload.image != null)
                {
                    file = folder + "/" + fileName;
                }
                sql = $"insert into {EventGiftTable} (stt_rec, ma_ct, ngay_ct, so_ct, stt_rec0, image) " +
                    $"values ('{saleUpload.so_ct}', '{"PXK"}', '{saleUpload.ngay_ct.ToString("yyyy-MM-dd")}', '{saleUpload.so_ct}', '{"001"}', '{file}')";
                this.ExecuteNonQuery(sql);
                return new
                {
                    file = file,
                };
            }
            else
            {
                string oldPath = dtSet.Tables[0].Rows[0]["image"].ToString();

                if (saleUpload.image != null && oldPath != null && oldPath != "" && oldPath != folder + "/" + fileName)
                {
                    oldPath = _fileService.getRootPath() + "/" + oldPath;
                    if (File.Exists(oldPath))
                    {
                        File.Delete(oldPath);
                    }
                }
                string newPath = "";
                //Cập nhật lại path vào database
                if (saleUpload.image != null)
                {
                    newPath = folder + "/" + fileName;
                    sql = $"update {EventGiftTable} set image = '{newPath}' where so_ct = '{saleUpload.so_ct}'";
                }
                this.ExecuteNonQuery(sql);
                return new
                {
                    file = newPath,
                };
            }
        }
        public void deleteEventGift(string so_ct, DateTime ngay_ct)
        {
            if (!IsSQLInjectionValid(so_ct))
                throw new CustomException(ApiReponseMessage.Error_InputData);
            string EventGiftTable = "m588img$";
            string expression = ngay_ct.ToString("yyyyMM");
            EventGiftTable += expression;

            // Thực hiện lấy path file cũ về
            string sql = $"select so_ct, image from {EventGiftTable} where so_ct = '{so_ct}'";
            DataSet dtSet = this.ExecSql2DataSet(sql);
            if (dtSet == null || dtSet.Tables.Count <= 0 || dtSet.Tables[0].Rows.Count <= 0) throw new CustomException(ApiReponseMessage.Error_notExist);
            string oldPath = dtSet.Tables[0].Rows[0][""].ToString();

            string folder = "/file/EventGift";

            if (oldPath != null && oldPath != "")
            {
                oldPath = _fileService.getRootPath() + "/" + oldPath;
                if (File.Exists(oldPath))
                {
                    File.Delete(oldPath);
                }
            }

            // Thực hiện xóa dữ liệu trong bảng 
            sql = $"delete {EventGiftTable} where so_ct = '{so_ct}'";
            this.ExecuteNonQuery(sql);
        }
        #endregion
    }
}