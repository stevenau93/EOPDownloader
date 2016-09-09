using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace EPODownloader
{
    class Email
    {
        public static int InsertEmail(EmailItem item)
        {
            SqlTransaction transaction = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(Config.ConnectionString))
                {
                    
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    string sql = "INSERT INTO dbo.Email (message_id,subject,content,date,cc,path)"
                     + "  VALUES(@message_id,@subject,@content,@date,@cc,@path);SELECT @@IDENTITY";
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    cmd.Transaction = transaction;
                    cmd.Parameters.AddWithValue("@message_id", item.MessageID);
                    cmd.Parameters.AddWithValue("@date", item.Date);

                    if (String.IsNullOrEmpty(item.Subject))
                    {
                        cmd.Parameters.AddWithValue("@subject", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@subject", item.Subject);
                    }

                    if (String.IsNullOrEmpty(item.Content))
                    {
                        cmd.Parameters.AddWithValue("@content", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@content", item.Content);
                    }

                    if (item.Cc.Count > 0)
                    {
                        cmd.Parameters.AddWithValue("@cc", string.Join(",", item.Cc));
                        item.Cc.Clear();
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@cc", DBNull.Value);
                    }

                    if (String.IsNullOrEmpty(item.Path))
                    {
                        cmd.Parameters.AddWithValue("@path", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@path", item.Path);
                    }

                    int id = Convert.ToInt32(cmd.ExecuteScalar());
                    transaction.Commit();
                    return id;
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return 0;
            }
        }

        public static bool InsertAttachment(AttachmentItem item)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Config.ConnectionString))
                {
                    connection.Open();
                    string sql = "INSERT INTO dbo.Attachment(email_id,attachment_name,content_type,size,temp_name,path)"
                        + "VALUES(@email_id,@attachment_name,@content_type,@size,@temp_name,@path)";
                    SqlCommand cmd = new SqlCommand(sql, connection);

                    cmd.Parameters.AddWithValue("@email_id", item.EmailID);

                    if (String.IsNullOrEmpty(item.GUID))
                    {
                        cmd.Parameters.AddWithValue("@temp_name", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@temp_name", item.GUID);
                    }


                    if (String.IsNullOrEmpty(item.AttachmentName))
                    {
                        cmd.Parameters.AddWithValue("@attachment_name", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@attachment_name", item.AttachmentName);
                    }


                    if (String.IsNullOrEmpty(item.ContentType))
                    {
                        cmd.Parameters.AddWithValue("@content_type", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@content_type", item.ContentType);
                    }


                    if (String.IsNullOrEmpty(Convert.ToString(item.Size)))
                    {
                        cmd.Parameters.AddWithValue("@size", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@size", item.Size);
                    }
                    if (String.IsNullOrEmpty(item.Path))
                    {
                        cmd.Parameters.AddWithValue("@path", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@path", item.Path);
                    }
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool GUIDIsExist(string guid)
        {
            using (SqlConnection connection = new SqlConnection(Config.ConnectionString))
            {
                connection.Open();
                string sql = @"SELECT id
                              FROM Attachment
                              WHERE temp_name = @temp_name";
                SqlCommand cmd = new SqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@temp_name", guid);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    return true;
                }
            }
            return false;
        }
    }

    class EmailItem
    {
        public string MessageID;
        public DateTime Date;
        public string Subject;
        public string Content;
        public List<string> Cc = new List<string>();
        public string Path;
    }

    public class AttachmentItem
    {
        public int EmailID;
        public string AttachmentName;
        public string ContentType;
        public string CharSet;
        public string GUID;
        public long Size;
        public string Path;
    }
}
