using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace schemaProject
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnEsitle_Click(object sender, EventArgs e)
        {
            try
            {
                SqlConnection HBaglanti = new SqlConnection();
                HBaglanti.ConnectionString = "Server=" + mtxtHedefServerName.Text +
                                            ";Database=" + mtxtHedefDatabaseName.Text +
                                            ";Trusted_Connection=true";
                SqlConnection KBaglanti = new SqlConnection();
                KBaglanti.ConnectionString = "Server=" + mtxtServerName.Text +
                                             ";Database=" + mtxtDatabaseName.Text +
                                             ";Trusted_Connection=true";

                using (KBaglanti)
                {
                    KBaglanti.Open();
                    //get db1 ınformation schema
                    DataTable InfoSchema = getInfoSchema(KBaglanti);
                    HBaglanti.Open();
                    EsitlemeIsl(InfoSchema, HBaglanti);
                }
                KBaglanti.Close();
                HBaglanti.Close();
                MetroFramework.MetroMessageBox.Show(this, "Esitleme Tamamlandi!");
            }
            catch (Exception)
            {
                MetroFramework.MetroMessageBox.Show(this, "Bir sorun olustu!");
            }
        }
        public void EsitlemeIsl(DataTable InfoSchema,SqlConnection HBaglanti)
        {
            foreach (DataRow row in InfoSchema.Rows)//tek tek tablo bilgilerini çeker
            {
                string tablename = row["TABLE_NAME"].ToString();
                string columnName = row["COLUMN_NAME"].ToString();
                string columnDefault = row["COLUMN_DEFAULT"].ToString();
                string isNull = row["IS_NULLABLE"].ToString();
                string dataType = row["DATA_TYPE"].ToString();

                SqlCommand setNull;
                SqlCommand defValue;

                if (isNull == "YES")
                {
                    setNull = new SqlCommand("ALTER TABLE @tablename alter column @columnName @dataType null", HBaglanti);
                    setNull.Parameters["@tablename"].Value = tablename;
                    setNull.Parameters["@columnName"].Value = columnName;
                    setNull.Parameters["@dataType"].Value = dataType;
                    setNull.ExecuteScalar();
                }
                else if (isNull == "NO")
                {
                    setNull = new SqlCommand("ALTER TABLE @tablename alter column @columnName @dataType not null", HBaglanti);
                    setNull.Parameters["@tablename"].Value = tablename;
                    setNull.Parameters["@columnName"].Value = columnName;
                    setNull.Parameters["@dataType"].Value = dataType;
                    setNull.ExecuteScalar();
                }
                if (columnDefault == "((0))")
                {
                    try
                    {
                        defValue = new SqlCommand("ALTER TABLE @tablename ADD CONSTRAINT @RandomString DEFAULT (0) FOR @columnName;", HBaglanti);
                        defValue.Parameters["@tablename"].Value = tablename;
                        defValue.Parameters["@RandomString"].Value = RandomString();
                        defValue.Parameters["@columnName"].Value = columnName;
                        defValue.ExecuteNonQuery();
                    }
                    catch (System.Data.SqlClient.SqlException)
                    {
                        continue;
                    }
                }
            }
        }
        public void dataTyIslem(SqlConnection connection)
        {
            SqlCommand command=new SqlCommand("BEGIN DECLARE @DataType VARCHAR(20), @ColumnName varchar(50), @TableName VARCHAR(50), @sql nvarchar(max)"+
            " DECLARE C1 CURSOR FOR select DATA_TYPE, COLUMN_NAME, TABLE_NAME from INFORMATION_SCHEMA.COLUMNS "+
                    "where TABLE_NAME IN "+
                    "(Select TABLE_NAME from @dbName.INFORMATION_SCHEMA.TABLES ) "+
                    "and DATA_TYPE = 'decimal'"+
            "OPEN C1; "+
            "FETCH NEXT FROM C1 "+
            "INTO @DataType, @ColumnName, @TableName; " +
            "WHILE @@FETCH_STATUS = 0 " +
            "BEGIN " +
            "set @sql = 'alter table ' + @TableName + ' alter column ' + @ColumnName + ' decimal(18,8)' " +
            "exec sp_executesql @sql " +
            "FETCH NEXT FROM C1 " +
            "INTO @DataType, @ColumnName, @TableName; " +
            "END " +
            "Close C1 "+
            "END",connection); //sadece veritabanı ismini alarak tüm decimal degerleri decimal(18,8) yapan komut
            command.Parameters["@dbName"].Value = getDbNme(connection);
            command.ExecuteNonQuery();
        }
        public string getDbNme(SqlConnection connection)
        {
           string conString= connection.ConnectionString;
            string[] words = conString.Split(';');

            return words[1].Substring(9);

        }
        public DataTable getInfoSchema(SqlConnection baglanti)
        {
            SqlCommand getInfoSchema = new SqlCommand("select * from INFORMATION_SCHEMA.COLUMNS ", baglanti);
            SqlDataReader reader = getInfoSchema.ExecuteReader();
            DataTable InfoSchema = new DataTable();
            InfoSchema.Load(reader);
            return InfoSchema;
        }
        private static Random random = new Random();
        public static string RandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return "DF_"+new string(Enumerable.Repeat(chars,5)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                SqlConnection HBaglanti = new SqlConnection();
                HBaglanti.ConnectionString = "Server=" + mtxtHedefServerName.Text +
                                            ";Database=" + mtxtHedefDatabaseName.Text +
                                            ";Trusted_Connection=true";
                dataTyIslem(HBaglanti);
                HBaglanti.Close();
                MetroFramework.MetroMessageBox.Show(this,"veritabanindaki tum decimal degerler decimal(18,8) ile degistirildi.");
            }
            catch (Exception)
            {

                MetroFramework.MetroMessageBox.Show(this,"Bir sorun olustu.");
            }
        }
    }
}
