using FirebirdSql.Data.FirebirdClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;

namespace OrderNumChecker
{
    public class HeartBeat
    {
        private readonly Timer _timer;

        public HeartBeat()
        {
            _timer = new Timer(Convert.ToInt32(ApiConnect.ReadS("repeatInterval"))) { AutoReset = false };

            _timer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();           
            Logger.InitLogger();
            //string[] lines = new string[] { DateTime.Now.ToString() };
            //File.AppendAllLines(@"Logs\log.txt", lines);
            Logger.Log.Info("----Start----");
            CheckRest("Армада", "arm", "1000011,1000011", DB.fb4);

            CheckRest("Пушкина", "psh", "1000016,1000016", DB.fb4);
            CheckRest("Мира", "mir", "1000015,1000015", DB.fb4);
            CheckRest("Правый", "yar", "1000024,1000024", DB.fb4);

            CheckRest("Взлетка", "vzl", "1000014,1000014", DB.fb4);
            CheckRest("Карамзина", "kmz", "1044370,1044370", DB.fb4);

            CheckRest("СиБ", "sib", "1000013,1000013", DB.fb5);
            CheckRest("БиБ", "bib", "1000010,1000010", DB.fb5);
            CheckRest("Крем", "krm", "1000029,1000029", DB.fb5);
            CheckRest("Формаджи", "fmg", "1000009,1000009", DB.fb5);

            CheckRest("Мамас", "mms", "1000022,1000022", DB.fb3);

            CheckRest("Якитория", "yaki", "1000012,1021966", DB.fb1);
            CheckRest("Ромбаба", "rb", "1032974,1032974", DB.fb1);
            CheckRest("Коко", "koko", "1000012,1021966", DB.fb1);

            Logger.Log.Info("----End----");
            //CheckRest("СиБ", "sib", "1000013,1000013");
            //CheckRest("Мамас", "mms", "1000022,1000022");
            //CheckRest("Маерчака", "mch", "1000011,1000011",DB.fb4);
            _timer.Start();
        }

        void CheckRest(string restname, string shortrestname, string restid, FbConnection fbconn) //restnam - Название ресторана из БД например "Армада", shortrestname - название для api например 'arm', restid - ид ресторана из Кипера 
        {
            //Logger.Log.Info("This is log4net string");
            DataTable dt = new DataTable();
            List<string> drv = new List<string>();
            List<string> settimesList = new List<string>();
            List<string> loadedtimeList = new List<string>();
            List<string> resulttimeList = new List<string>();

            Logger.Log.Info(" ");
            Logger.Log.Info("////////////////////// " + restname.ToUpper() + " //////////////////////");
            //Logger.Log.Info("Получаем данные о заказах с FireBird для " + restname + "...");

            try
            {
                dt = DB.GetData(restid, fbconn, DateTime.Now.AddMinutes(30).ToString(), DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), "9999,9999,9999");//Получить кол-во заказов по времени с разбивкой по часам и 10 минуткам

                if (dt.Rows.Count > 0)
                {
                    //Logger.Log.Info("заказы с Firebird для "+ restname + "... Success!");
                    //Logger.Log.Info("Получаем данные о количестве водителей с SQL для " + restname + "...");

                    drv = DB.GetDataSql(DB.sql_exp, DB.getdrvnum, restname);
                    if (drv.Count > 3)
                    {
                        //Logger.Log.Info("Водители с SQL для " + restname + "... Success!");//Получить кол-во заказов которое можно принимать с учетом кол-ва водителей

                        Logger.Log.Info("   ");
                        //выводим кол-во заказов на текущий момент времени
                        //Logger.Log.Info()

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            Logger.Log.Info(dt.Rows[i].ItemArray.ToArray());

                        }
                        //Logger.Log.Info("   ");
                        //Выводим кол-во курьеров и сколько можно принять заказов
                        Logger.Log.Info("Кол-во курьеров и максимально количество заказов:" + String.Join(",", drv.ToArray()));
                        Logger.Log.Info(" Загружаем уже выставленные стопы  ");
                        //Загружаем уже выставленные стопы
                        var jTimes = ApiConnect.GetRestTimes(shortrestname);
                        Logger.Log.Info(" Cтопы загружены, идет анализ  ");
                        if (!(jTimes is null))
                        {
                            try
                            {
                                foreach (var record in jTimes.Children())
                                {
                                    var itemProperties = record.Children<JProperty>();
                                    var timeElement = itemProperties.FirstOrDefault(x => x.Name == "Time");
                                    var timeElementValue = timeElement.Value;
                                    loadedtimeList.Add(timeElementValue.ToString());
                                }
                                Logger.Log.Info("Выставленные стопы: " + String.Join(",", loadedtimeList.ToArray()));


                                //Сравниваем кол-во заказов на одно время с возможным максимальным кол-вом заказов и заполняем список с забитыми временами
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    for (int y = 1; y < dt.Columns.Count; y++)
                                    {

                                        if (Convert.ToInt32(dt.Rows[i][y]) >= Convert.ToUInt32(drv[2].ToString()) - 0)
                                        {
                                            settimesList.Add(dt.Rows[i][0].ToString() + dt.Columns[y].ColumnName.ToString());
                                            Logger.Log.Info(restname + ", на " + dt.Rows[i][0].ToString() + dt.Columns[y].ColumnName.ToString() + " " + dt.Rows[i][y].ToString() + " заказов. Время добавлено в список для проверки");
                                        }
                                        else
                                        {
                                            //Logger.Log.Info("Армада, на " + dt.Rows[i][0].ToString() + dt.Columns[y].ColumnName.ToString() + " " + dt.Rows[i][y].ToString() + " заказов");
                                        }

                                    }
                                }

                                //Сравниваем уже вычисленные стопы с выставленными
                                resulttimeList = settimesList.Except(loadedtimeList, StringComparer.InvariantCultureIgnoreCase)
                                                .ToList();
                                Logger.Log.Info("Выставляем времена на стоп: " + String.Join(",", resulttimeList.ToArray()));
                                if (resulttimeList.Count > 0)
                                {
                                    foreach (string t in resulttimeList)
                                    {
                                        Task<HttpResponseMessage> ResObj = ApiConnect.SetTime(restname, t, DateTime.Now.ToString("yyyy-MM-dd"), Convert.ToInt64(restid.Split(',')[0]));
                                        //Logger.Log.Info(ResObj.Result.);
                                        //Logger.Log.Info(ResObj.Result.Headers.GetValues("X-Error-Code").FirstOrDefault());
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Log.Info(ex.ToString());
                            }
                            Logger.Log.Info(" ");
                        }
                        else
                        {
                            Logger.Log.Info("Stops API didn't return the result, Exiting....");
                        }
                    }
                    else
                    {
                        Logger.Log.Info("Couldn't get driver number");
                    }
                }
                else
                {
                    Logger.Log.Info("Из базы Firebird не получено данных, либо отсутствуют заказы, Exiting....");
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Info(ex.ToString());
            }
            //System.Threading.Thread.Sleep(1500);
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

    }
}
