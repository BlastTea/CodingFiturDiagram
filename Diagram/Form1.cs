using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using Logitop.Services;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Transactions;

namespace Diagram
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            DateTime firstDate;
            DateTime lastDate;

            ObservableCollection<DateTimePoint> dates = new ObservableCollection<DateTimePoint>();

            //DataTable data = DbHelper.GetInstance().ExecuteQuery("SELECT * FROM public.transaksi JOIN detail_transaksi ON detail_transaksi.id_transaksi = transaksi.id_transaksi JOIN laptop ON detail_transaksi.id_laptop = laptop.id_laptop ORDER BY id_detail_transaksi");
            DataTable data = DbHelper.GetInstance().ExecuteQuery("SELECT * FROM public.transaksi");

            firstDate = (DateTime)data.Rows[0]["tanggal_transaksi"];
            lastDate = (DateTime)data.Rows[data.Rows.Count - 1]["tanggal_transaksi"];

            TimeSpan duration = lastDate - firstDate;

            foreach (DataRow row in data.Rows)
            {
                DateTime tanggal = (DateTime)row["tanggal_transaksi"];
                int total = (int)row["bayar"];

                // kondisi jika durasi hari lebih dari 30 hari
                if (duration.Days >= 30)
                {
                    if (dates.Count == 0)
                    {
                        DateTime now = DateTime.Now;
                        for (int i = firstDate.Month; i <= firstDate.Month + (duration.Days % 365) / 30; i++)
                        {
                            dates.Add(new DateTimePoint(new DateTime(now.Year, i, now.Day), 0));
                        }
                    }
                    dates.Where((e) => e.DateTime.Month == tanggal.Month && e.DateTime.Year == tanggal.Year).Single().Value += total;
                }
                // jika durasi lebih dari 1 hari
                else if (duration.Days >= 1)
                {
                    if (dates.Count == 0)
                    {
                        DateTime now = DateTime.Now;
                        for (int i = 0; i <= 30; i++)
                        {
                            dates.Add(new DateTimePoint(new DateTime(now.Year, now.Month, i), 0));
                        }
                    }
                    dates.Where((e) => e.DateTime.Day == tanggal.Day).Single().Value += total;
                }
                // jika kondisi kurang dari atau sama dengan 1 hari
                else
                {
                    if (dates.Count == 0)
                    {
                        DateTime now = DateTime.Now;
                        for (int i = 0; i <= 23; i++)
                        {
                            dates.Add(new DateTimePoint(new DateTime(now.Year, now.Month, now.Day, i, 0, 0), 0));
                        }
                    }
                    dates.Where((e) => e.DateTime.Hour == tanggal.Hour).Single().Value += total;
                }
            }

            chart1.Title = new LabelVisual
            {
                Text = "Laporan Pembayaran Transaksi",
                TextSize = 25,
                Padding = new LiveChartsCore.Drawing.Padding(15),
                Paint = new SolidColorPaint(SKColors.DarkSlateGray)
            };

            chart1.Series = new ISeries[] {
                new ColumnSeries<DateTimePoint>
                {
                    TooltipLabelFormatter = (chartPoint) => $"{new DateTime((long) chartPoint.SecondaryValue):HH:00}: {chartPoint.PrimaryValue:N0}",
                    Name = "Bayar",
                    Values = dates
                }
            };

            chart1.XAxes = new Axis[] {
                new Axis
                {
                    Labeler = value => new DateTime((long) value).ToString("HH:00"),
                    LabelsRotation = 80,

                    // when using a date time type, let the library know your unit 
                    //UnitWidth = TimeSpan.FromDays(1).Ticks, 

                    // if the difference between our points is in hours then we would:
                     UnitWidth = TimeSpan.FromHours(1).Ticks,

                    // since all the months and years have a different number of days
                    // we can use the average, it would not cause any visible error in the user interface
                    // Months: TimeSpan.FromDays(30.4375).Ticks
                    // Years: TimeSpan.FromDays(365.25).Ticks

                    // The MinStep property forces the separator to be greater than 1 day.
                    MinStep = TimeSpan.FromHours(1.2).Ticks,
                }
            };
        }
    }
}