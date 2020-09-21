using FirebirdSql.Data.FirebirdClient;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNumChecker
{
    class DB
    {
        public static FbConnection fb1 = new FbConnection(@"Server=192.168.10.17;User=SYSDBA;Password=masterkey;Database=C:\UCS\Delivery 3\YAKRB_23_CENTR\DB\YAKRB_23_CENTR.FDB");
        public static FbConnection fb2 = new FbConnection(@"Server=192.168.10.17;User=SYSDBA;Password=masterkey;Database=C:\UCS\Delivery 3\YAKRB_23_CENTR\DB\YAKRB_23_CENTR.FDB");
        public static FbConnection fb3 = new FbConnection(@"Server=192.168.10.77;User=SYSDBA;Password=masterkey;Database=C:\UCS\Delivery 3\MAMAS_23_SRV\DB\MAMAS_23_CENTR.FDB");
        public static FbConnection fb4 = new FbConnection(@"Server=192.168.10.7;User=SYSDBA;Password=masterkey;Database=C:\UCS\Delivery 3\Database\DLV_23_CENTR.FDB");
        public static FbConnection fb5 = new FbConnection(@"Server=192.168.10.235;User=SYSDBA;Password=masterkey;Database=C:\UCS\Delivery 3\REST_23_SRV\DB\REST_23.FDB");

        public SqlConnection sql_ya = new SqlConnection("server=192.168.100.70;Database=BG_REP_CENTR;uid=website;pwd=_PO9iu7yt753285");
        public SqlConnection sql_rb = new SqlConnection("server=192.168.100.70;Database=BG_REP_CENTR;uid=website;pwd=_PO9iu7yt753285");
        public SqlConnection sql_mamas = new SqlConnection("server=192.168.100.70;Database=BG_REP_CENTR;uid=website;pwd=_PO9iu7yt753285");
        SqlConnection sql_perci = new SqlConnection("server=192.168.100.70;Database=BG_REP_CENTR;uid=website;pwd=_PO9iu7yt753285");
        SqlConnection sql_rests = new SqlConnection("server=192.168.100.70;Database=BG_REP_CENTR;uid=website;pwd=_PO9iu7yt753285");
        public static MySqlConnection sql_exp = new MySqlConnection(ApiConnect.ReadS("mysqlconnstr"));

        //Получит кол-во курьеров с одного заведения
        public static string getdrvnum = @"select z.Name as 'Заведение',
                                            z.Drv_num as 'kurcount',
                                            z.Walk_num as 'walkcount',
                                            sc.Driver_orders as 'orders'
                                            from Zones as z
                                            left join Drvnum_scheme as sc on z.Scheme_id=sc.Scheme_id and z.Drv_num=sc.People_count
                                            where Name=@Zone";
        //Получить дефолтное кол-во курьеров на одном заведении
        string getdrvnum_def = @"select z.Name as 'Заведение',
                                            z.Drv_num_default as 'kurcount',
                                            z.Walk_num_default as 'walkcount',
                                            sc_d.Driver_orders as 'orders'
                                            from Zones as z
                                            left join Drvnum_scheme as sc_d on z.Scheme_id=sc_d.Scheme_id and z.Drv_num_default=sc_d.People_count
                                            where Name=@Zone";

        //Сетка заказов по интервалам доставки для курьеров для одного заведения за сегодня
        

       

        public static DataTable GetData(string id, FbConnection fbconn, string start_datetime, string end_datetime, string zone) // метод возвращающий кол-во заказов из базы firebird
        {
           
           string sqlcount_deliv = @"SELECT EXTRACT(HOUR FROM CAST(o.WAIT_DELIVERY_TIME AS time)) as ""Часы"",
                            SUM(CASE EXTRACT(MINUTE FROM CAST(o.WAIT_DELIVERY_TIME AS time)) WHEN 0 THEN 1 ELSE 0 end) AS "":00"",
                            SUM(CASE EXTRACT(MINUTE FROM CAST(o.WAIT_DELIVERY_TIME AS time)) WHEN 10 THEN 1 ELSE 0 end) AS "":10"",
                            SUM(CASE EXTRACT(MINUTE FROM CAST(o.WAIT_DELIVERY_TIME AS time)) WHEN 20 THEN 1 ELSE 0 end) AS "":20"",
                            SUM(CASE EXTRACT(MINUTE FROM CAST(o.WAIT_DELIVERY_TIME AS time)) WHEN 30 THEN 1 ELSE 0 end) AS "":30"",
                            SUM(CASE EXTRACT(MINUTE FROM CAST(o.WAIT_DELIVERY_TIME AS time)) WHEN 40 THEN 1 ELSE 0 end) AS "":40"",
                            SUM(CASE EXTRACT(MINUTE FROM CAST(o.WAIT_DELIVERY_TIME AS time)) WHEN 50 THEN 1 ELSE 0 end) AS "":50""
                            FROM dlv_orders as o
                            LEFT JOIN DLV_ADDRESSES AS a on o.ADDRESS_ID = a.address_id
                            LEFT JOIN DLV_STREETS AS s ON a.STREET_ID=s.STREET_ID
                            LEFT JOIN DLV_ZONES AS z ON z.ZONE_ID=a.zone_id
                            WHERE o.WAIT_DELIVERY_TIME > @date and o.WAIT_DELIVERY_TIME < @date2 AND o.DELETED=0 and o.DLV_TYPE<>1 and (z.ZONE_ID<>@zone or z.ZONE_ID<>@zone2 or z.ZONE_ID<>@zone3 or z.ZONE_ID IS NULL) AND (o.RESTAURANT_ID=@rest OR o.RESTAURANT_ID=@rest2) GROUP BY ""Часы""";

            string sqlexpr = sqlcount_deliv;

            DataSet ds = new DataSet();
            DataTable Data = new DataTable();
            FbParameter par1 = new FbParameter("@rest", id.Split(',')[0]);

            FbParameter par2 = new FbParameter("@date", start_datetime);

            FbParameter par7 = new FbParameter("@date2", end_datetime);

            FbParameter par3 = new FbParameter("@zone", zone.Split(',')[0]);

            FbParameter par4 = new FbParameter("@rest2", id.Split(',')[1]);

            FbParameter par5 = new FbParameter("@zone2", zone.Split(',')[1]);

            FbParameter par6 = new FbParameter("@zone3", zone.Split(',')[2]);
            
            try
            {
                //Label1.Visible = false;
                fbconn.Open();
                FbCommand cmd = new FbCommand(sqlexpr, fbconn);
                cmd.Parameters.Add(par1);
                cmd.Parameters.Add(par2);
                cmd.Parameters.Add(par3);
                cmd.Parameters.Add(par4);
                cmd.Parameters.Add(par5);
                cmd.Parameters.Add(par6);
                cmd.Parameters.Add(par7);

                cmd.CommandTimeout = (3500);

                FbDataAdapter fbadapter = new FbDataAdapter(cmd);

                fbadapter.Fill(ds);

                Data = ds.Tables[0];
                fbconn.Close();
            }
            catch (FbException ex)
            {
                fbconn.Close();
                Logger.Log.Info("Ups, there is a problem:");
                Logger.Log.Info(ex.ToString());
                //Label1.Visible = true;
                //Label1.Text = ex.ToString();
                //fbconn.Close();
                //Console.WriteLine(ex.ToString());
                
            }
            finally
            {
                fbconn.Close();
            }
            fbconn.Close();
            return Data;
            
        }

        public static List<string> GetDataSql(MySqlConnection sql_exp, string sqlquery, string zone) //метод возвращающий дату
        {
            List<string> res = new List<string>();


            try
            {
                sql_exp.Open();


                MySqlCommand command_arm = new MySqlCommand(sqlquery, sql_exp);
                MySqlParameter par = new MySqlParameter("@Zone", zone);
                command_arm.Parameters.Add(par);

                command_arm.CommandTimeout = (2000);
                MySqlDataReader reader = command_arm.ExecuteReader();
                
                while (reader.Read())
                {
                    res.Add(reader["kurcount"].ToString());
                    res.Add(reader["walkcount"].ToString());
                    res.Add(reader["orders"].ToString());

                }

                reader.Close();
                sql_exp.Close();
                return res;
            }
            catch (MySqlException ex)
            {
                Logger.Log.Info("Ups, there is a problem:");
                Logger.Log.Info(ex.ToString());
                sql_exp.Close();
                List<string> nullres = new List<string> {"0","0","0"};
                return nullres;
                     
            }
            finally
            {
                for (int i = 0; i < 3; i++)
                {
                    res.Add("0");
                }
                sql_exp.Close();
                
            }
            
            
        }

        public static List<string> SearchOverloadedTimes(List<string> drv, DataTable dt)
        {
            List<string> timesList = new List<string>();

            for(int i=1; i<dt.Rows.Count-1; i++)
            {
                for(int y=1; y<dt.Columns.Count-1; y++)
                {
                    if(Convert.ToInt32(dt.Rows[i][y]) >= Convert.ToUInt32(drv[2].ToString()))
                    {
                        timesList.Add(dt.Rows[i][0].ToString() + dt.Rows[0][y].ToString());
                    }
                   
                }
            }

            return timesList;
        }

    }
}
