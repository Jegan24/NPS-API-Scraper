using NP_API_To_DB.DAOs;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace NP_API_To_DB
{
    class Program
    {
        const int totalNumberOfNationalParks = 498;
        static void Main(string[] args)
        {
            List<NationalParkServiceJsonData> data = new List<NationalParkServiceJsonData>();
            ApiDAO apiDao;
            SqlDAO sqlDao;
            string connectionString = ConfigurationManager.ConnectionStrings["NPGeek"].ConnectionString;
            string apiKey = ConfigurationManager.ConnectionStrings["npsApiKey"].ConnectionString;
            apiDao = new ApiDAO(apiKey);
            sqlDao = new SqlDAO(connectionString);
            Console.WriteLine($"Database has {sqlDao.GetDBParkCount()} parks stored");
            bool auto = !GetYesOrNo("Manual fill? (more stable)");
            try
            {
                if (auto)
                {
                    Auto(sqlDao, apiDao, data);
                }
                else
                {
                    Manual(sqlDao, apiDao, data);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("An error was encountered, probably got timed out by NPS.");
                Console.WriteLine($"{ex.Message}\n{ex.StackTrace}\n{ex.InnerException}\n{ex.Source}");
            }


        }

        static bool GetYesOrNo(string prompt)
        {
            bool result = false;
            string response = "";
            bool hasResponded = false;
            while (!(response.ToUpper().Substring(0) == "Y" || response.ToUpper().Substring(0) == "N"))
            {
                if (hasResponded)
                {
                    Console.WriteLine("Answer the question.");
                }
                Console.Write(prompt + " Y/N: ");
                response = Console.ReadLine();
                hasResponded = true;
            }
            result = response.Substring(0).ToUpper() == "Y";
            return result;
        }

        static void Auto(SqlDAO sqlDao, ApiDAO apiDao, List<NationalParkServiceJsonData> data)
        {
            int parkCount = sqlDao.GetDBParkCount();
            int actualMax = int.MaxValue;
            bool alreadyFull = true;
            DateTime totalStart = DateTime.Now;
            DateTime totalEnd;
            Console.WriteLine("BEGINNING AUTOFILL. YOLO");
            while (parkCount < totalNumberOfNationalParks)
            {
                alreadyFull = false;
                int beforeCount = sqlDao.GetDBParkCount();
                DateTime start = DateTime.Now;
                DateTime end;
                Console.WriteLine($"Pulling from NPS...");
                data = new List<NationalParkServiceJsonData>(apiDao.GetParkData(25, sqlDao.GetDBParkCount()));
                end = DateTime.Now;
                Console.WriteLine($"Done. Execution took {(end - start).TotalSeconds} seconds.\nBeginning upload to SQL...");
                start = DateTime.Now;
                sqlDao.InsertDataIntoSql(data);
                parkCount = sqlDao.GetDBParkCount();
                end = DateTime.Now;
                if (beforeCount == parkCount)
                {
                    actualMax = parkCount;
                    Console.WriteLine($"Done. Execution took {(end - start).TotalSeconds} seconds. Database did not increase in size, assuming we have every national park now.\n Actual total: {actualMax} (Write this down. WRITE THIS DOWN)");
                    break;
                }
                Console.WriteLine($"Done. Execution took {(end - start).TotalSeconds} seconds. {parkCount}/{totalNumberOfNationalParks} have been processed.");
            }
            totalEnd = DateTime.Now;
            if (alreadyFull)
            {
                Console.WriteLine($"The database is already full dummy, if you really want it to run you can recreate the database and run this again.");
            }
            else
            {
                Console.WriteLine($"\n\n\n\nAuto fill completed with out errors? Wow I'm good. Execution took {(totalEnd - totalStart).Minutes} minutes.");
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        static void Manual(SqlDAO sqlDao, ApiDAO apiDao, List<NationalParkServiceJsonData> data)
        {
            DateTime start = DateTime.Now;
            DateTime end;
            int querySize = 0;
            bool runAgain = false;
            int parkCount;
            do
            {
                parkCount = sqlDao.GetDBParkCount();
                if (parkCount >= totalNumberOfNationalParks)
                {
                    Console.WriteLine("Database already has every national park. Program will now close");
                    Console.ReadKey();
                    return;
                }
                Console.WriteLine($"Database has {parkCount}/{totalNumberOfNationalParks} parks stored");
                while (querySize < 1)
                {
                    Console.WriteLine("Enter number of entries to retrieve, (exceeding 75 causes instability)");
                    if (int.TryParse(Console.ReadLine(), out querySize))
                    {
                        if (querySize < 1)
                        {
                            Console.WriteLine("Number must be positive.");
                        }

                        if (querySize > 100)
                        {

                            bool confirm = GetYesOrNo("Are you sure?");
                            if (!confirm)
                            {
                                querySize = 0;
                            }
                        }

                    }
                }
                Console.WriteLine("Getting data from NPS... this may take some time");
                data = new List<NationalParkServiceJsonData>(apiDao.GetParkData(querySize, sqlDao.GetDBParkCount()));
                end = DateTime.Now;
                Console.WriteLine($"Done. Execution took {(end - start).TotalSeconds} seconds.\nBeginning upload to SQL... this may also take some time");
                start = DateTime.Now;
                sqlDao.InsertDataIntoSql(data);
                end = DateTime.Now;
                Console.WriteLine($"Done. Execution took {(end - start).TotalSeconds} seconds.");
                runAgain = GetYesOrNo("Run again?");
            }
            while (runAgain);
        }
    }
}
