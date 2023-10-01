using System; //使用命名空間 匯入函式庫
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace Hello_World
{
    /*
     * Staff            員工資料
     * Manager : Staff  經理
     * Admin : Staff    管理者
     * FileReader       讀取文件的
     * PaySlip          工資單
     * Program
     */

    class Program
    {
        static void Main(string[] args)
        {
            List<Staff> Staffs = new List<Staff>();
            FileReader fileReader = new FileReader();
            int month = 0, year = 0;

            while (year == 0)
            {
                Console.WriteLine("Please enter the year:");
                try
                {
                    year = Convert.ToInt32(Console.ReadLine());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + "Please try again");
                }
            }

            while (month == 0)
            {
                Console.WriteLine("Please enter the month:");
                try
                {
                    month = Convert.ToInt32(Console.ReadLine());

                    if (month < 1 || month > 12)
                    {
                        Console.WriteLine("Month must be from 1 to 12. Please try again");
                        month = 0;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + "Please try again");
                }
            }

            Staffs = fileReader.ReadFile();

            for (int i = 0; i < Staffs.Count; i++)
            {
                try
                {
                    Console.WriteLine($"Enter Hours worked for {Staffs[i].NameofStaff}");
                    Staffs[i].HourWorked = Convert.ToInt32(Console.ReadLine());
                    Staffs[i].CalulatePay();
                    Console.WriteLine(Staffs[i]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    i--;
                }
            }

            PaySlip ps = new PaySlip(month, year);
            ps.GeneratePaySlip(Staffs);
            ps.GenerateSummary(Staffs);

            Console.Read();
        }
    }

    class Staff
    {
        private float hourlyRate;                       //時薪
        private int hWorked;                            //工作時數
        public float TotalPay { get; protected set; }   //總支付
        public float BasicPay { get; private set; }     //底薪
        public string NameofStaff { get; private set; } //員工名稱
        public int HourWorked
        {
            get { return hWorked; }
            set
            {
                if (value > 0)
                {
                    hWorked = value;
                }
                else
                {
                    hWorked = 0;
                }
            }
        }
        public Staff(string name, float rate)
        {
            NameofStaff = name;
            hourlyRate = rate;
        }
        /*如果子類別有相同的方法，使用子類別的方法*/
        public virtual void CalulatePay()
        {
            Console.WriteLine("CalulatePay Pay...");

            BasicPay = hWorked * hourlyRate;
            TotalPay = BasicPay;
        }
        public override string ToString()
        {
            return $"Name of Staff: {NameofStaff}\n" +
                   $"Hourly rate: {hourlyRate}\n" +
                   $"Hours worked: {HourWorked}\n" +
                   $"Basic pay: {BasicPay}\n" +
                   $"Total pay: {TotalPay}";
        }
    }

    class Manager : Staff
    {
        private const float managerHourlyRate = 50; //經理時薪

        public int Allowance { get; private set; }
        public Manager(string name) : base(name, managerHourlyRate)
        {

        }

        public override void CalulatePay()
        {
            base.CalulatePay();

            if (HourWorked > 160)
            {
                Allowance = 1000;
                TotalPay = BasicPay + Allowance;
            }
        }

        public override string ToString()
        {
            return $"Name of Staff: {NameofStaff}\n" +
                   $"Hourly rate: {managerHourlyRate}\n" +
                   $"Hours worked: {HourWorked}\n" +
                   $"Basic pay: {BasicPay}\n" +
                   $"Allowance: {Allowance}\n" +
                   $"Total pay: {TotalPay}";
        }
    }

    class Admin : Staff
    {
        private const float overtimeRate = 15.5f;   //超時津貼
        private const float adminHourlyRate = 30f;  //管理員時薪
        public float Overtime { get; private set; }
        public Admin(string name) : base(name, adminHourlyRate) { }
        public override void CalulatePay()
        {
            base.CalulatePay();
            if (HourWorked > 160)
            {
                Overtime = overtimeRate * (HourWorked - 160);
                TotalPay = Overtime + BasicPay;
            }
        }
        public override string ToString()
        {
            return $"Name of Staff: {NameofStaff}\n" +
                   $"Admin hourly rate: {adminHourlyRate}\n" +
                   $"Hours worked: {HourWorked}\n" +
                   $"Basic pay: {BasicPay}\n" +
                   $"overtimeRate: {Overtime}\n" +
                   $"Total pay: {TotalPay}";
        }
    }

    class FileReader
    {
        public List<Staff> ReadFile()
        {
            List<Staff> Staffs = new List<Staff>();
            string path = "C:\\Users\\WeiWei\\Desktop\\練習區\\C#練習\\小作品\\人資計算\\Staff.txt";
            string[] result = new string[2];
            string[] separator = { "," };   //數據分割符號

            if (File.Exists(path))
            {
                /*使用using 怕失敗的時候程式不會卡死*/
                using (StreamReader fileThings = new StreamReader(path))
                {
                    while (!fileThings.EndOfStream)
                    {
                        result = fileThings.ReadLine().Split(separator, StringSplitOptions.RemoveEmptyEntries); /*StringSplitOptions.RemoveEmptyEntries 取消陣列中的所有空白*/
                        if (result[1] == "Manager")
                        {
                            Staffs.Add(new Manager(result[0]));
                        }
                        else if (result[1] == "Admin")
                        {
                            Staffs.Add(new Admin(result[0]));
                        }
                    }
                    fileThings.Close();
                }
            }
            else
            {
                Console.WriteLine("Error: File does not exists");
            }
            return Staffs;
        }
    }

    class PaySlip
    {
        private int month;
        private int year;

        enum MonthsOfYear
        {
            JAN = 1, FEB = 2, MAR = 3, APR = 4,
            MAY = 5, JUN = 6, JUL = 7, AUG = 8,
            SEP = 9, OCT = 10, NOV = 11, DEC = 12
        }
        public PaySlip(int payMonth, int payYear)
        {
            month = payMonth;
            year = payYear;
        }
        public void GeneratePaySlip(List<Staff> Staff)
        {
            string path;

            foreach (Staff s in Staff)
            {
                path = s.NameofStaff + ".txt";

                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine($"PAYSKIP FOR {(MonthsOfYear)month} {year}");
                    sw.WriteLine("=========================================");
                    sw.WriteLine($"Name of Staff: {s.NameofStaff}");
                    sw.WriteLine($"Hours Worked: {s.HourWorked}");
                    sw.WriteLine("");
                    sw.WriteLine($"Basic Pay: {s.BasicPay:C}");

                    if (s.GetType() == typeof(Manager))
                    {
                        sw.WriteLine($"Allowance: {((Manager)s).Allowance:C}");
                    }
                    else if (s.GetType() == typeof(Admin))
                    {
                        sw.WriteLine($"Overtime: {((Admin)s).Overtime:C}");
                    }
                    sw.WriteLine("");
                    sw.WriteLine("=========================================");
                    sw.WriteLine($"Total Pay: {s.TotalPay}");
                    sw.WriteLine("=========================================");

                    sw.Close();
                }
            }
        }
        public void GenerateSummary(List<Staff> Staffs)
        {
            var result =
                from s in Staffs
                where s.HourWorked < 10
                orderby s.NameofStaff ascending
                select new { s.NameofStaff, s.HourWorked };

            string path = "summary.txt";

            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("Staff with less than 10 working hours");
                sw.WriteLine("");

                foreach (var r in result)
                {
                    sw.WriteLine($"Name of Staff: {r.NameofStaff}, Hours Worked: {r.HourWorked}");
                }

                sw.Close();
            }
        }

        public override string ToString()
        {
            return $"month = {month} year = {year}";
        }
    }
}