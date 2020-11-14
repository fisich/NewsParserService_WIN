using HtmlAgilityPack;
using NewsParsingUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;


namespace NewsCollectorService
{
    public partial class MyService : ServiceBase
    {
        HtmlWeb web;
        Timer timer;
        List<IBaseNewsParser> parsers;
        PostgreSQLManagement dataBase;
        public MyService()
        {
            InitializeComponent();
            this.ServiceName = "News collector";
            this.EventLog.Source = this.ServiceName;
            this.EventLog.Log = "Application";
            this.AutoLog = true;
            web = new HtmlWeb();
            web.PreRequest += request =>
            {
                request.CookieContainer = new System.Net.CookieContainer();
                return true;
            };
            parsers = new List<IBaseNewsParser> { new IgromaniaNewsParser(), new HabrNewsParser(),
            new IzvestiyaNewsParser(), new KinoNewsParser(), new NJCarNewsParser() };
            dataBase = new PostgreSQLManagement();
        }

        protected override void OnStart(string[] args)
        {
            timer = new Timer();
            timer.Interval = 3600000; // 1 hour
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
            ParseData();
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            ParseData();
        }

        private void ParseData()
        {
            foreach (IBaseNewsParser parser in parsers)
            {
                if (parser.StartParsing())
                {
                    this.EventLog.WriteEntry("Сбор данных с портала " + parser.GetName() + " завершен успешно ", EventLogEntryType.SuccessAudit);

                }
                else
                {
                    this.EventLog.WriteEntry("Проблемы с подключением к порталу " + parser.GetName(), EventLogEntryType.FailureAudit);
                    continue;
                }
                PostgreSQLState state = dataBase.InsertNewsItems(parser);
                if (state == PostgreSQLState.ExecutionCompleted)
                {
                    this.EventLog.WriteEntry("Данные с портала " + parser.GetName() + " успешно сохранены", EventLogEntryType.Information);
                }
                else
                {
                    this.EventLog.WriteEntry("Ошибка сохранения данных с портала " + parser.GetName() + "\n" + dataBase.GetLastQuery(), EventLogEntryType.Error);
                }
            }
        }

        protected override void OnStop()
        {
            timer.Stop();
        }
    }
}
