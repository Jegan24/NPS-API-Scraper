using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace NP_API_To_DB.DAOs
{
    public class SqlDAO
    {
        private string connectionString;
        public SqlDAO(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public int GetDBParkCount()
        {
            int count = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM national_park", connection);
                count = Convert.ToInt32(command.ExecuteScalar());
            }
            return count;
        }
        public void InsertDataIntoSql(IEnumerable<NationalParkServiceJsonData> parksData)
        {
            // we use the same connection for every task to avoid excessive amounts of opening/closing connections
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (NationalParkServiceJsonData parkData in parksData)
                {
                    int newId = 0;
                    string cmdText = "AddNationalPark";
                    SqlCommand command = new SqlCommand();
                    command.CommandText = cmdText;
                    command.Connection = connection;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    AddNationalParkParameters(parkData, command);


                    newId = Convert.ToInt32(command.ExecuteScalar());

                    // Continue if success
                    if (newId > 0)
                    {
                        // Insert Park Entrance Fees
                        if (parkData.entranceFees != null && parkData.entranceFees.Length > 0)
                        {
                            AddEntranceFees(newId, parkData.entranceFees, connection);
                        }

                        // Insert Park Operating Hours
                        if (parkData.operatingHours != null && parkData.operatingHours.Length > 0)
                        {
                            foreach (Operatinghour operatinghour in parkData.operatingHours)
                            {
                                AddOperatingHours(newId, operatinghour, connection);
                            }

                        }

                        // Insert Park Images
                        if (parkData.images != null && parkData.images.Length > 0)
                        {
                            AddImages(newId, parkData.images, connection);
                        }

                    }

                }
            }


        }

        private void AddEntranceFees(int parkId, Entrancefee[] entranceFees, SqlConnection connection)
        {
            foreach (Entrancefee entrancefee in entranceFees)
            {
                string cmdText = "AddEntranceFees";
                SqlCommand command = new SqlCommand();
                command.CommandText = cmdText;
                command.Connection = connection;
                command.CommandType = System.Data.CommandType.StoredProcedure;
                AddEntranceFeeParameters(parkId, entrancefee, command);
                command.ExecuteNonQuery();
            }
        }

        private void AddEntranceFeeParameters(int parkId, Entrancefee entrancefee, SqlCommand command)
        {
            command.Parameters.AddWithValue("@national_park_id", parkId);
            Decimal cost;
            if (!(Decimal.TryParse(entrancefee.cost, out cost)))
            {
                cost = 0;
            }
            command.Parameters.AddWithValue("@cost", cost);
            command.Parameters.AddWithValue("@title", entrancefee.title);
            command.Parameters.AddWithValue("@description", entrancefee.description);
        }

        private void AddOperatingHours(int parkId, Operatinghour operatingHours, SqlConnection connection)
        {
            List<GenericHours> FlattenedHours = new List<GenericHours>(FlattenHours(operatingHours));
            foreach (GenericHours hours in FlattenedHours)
            {
                string cmdText = "AddOperatingHours";
                SqlCommand command = new SqlCommand();
                command.CommandText = cmdText;
                command.Connection = connection;
                command.CommandType = System.Data.CommandType.StoredProcedure;
                AddOperatingHoursParameters(parkId, hours, command);
                command.ExecuteNonQuery();
            }
        }

        private void AddOperatingHoursParameters(int parkId, GenericHours hours, SqlCommand command)
        {
            command.Parameters.AddWithValue("@national_park_id", parkId);
            if (!hours.isException)
            {
                command.Parameters.AddWithValue("@description", hours.description);
            }
            command.Parameters.AddWithValue("@name", hours.name);
            command.Parameters.AddWithValue("@sunday", hours.sunday);
            command.Parameters.AddWithValue("@monday", hours.monday);
            command.Parameters.AddWithValue("@tuesday", hours.tuesday);
            command.Parameters.AddWithValue("@wednesday", hours.wednesday);
            command.Parameters.AddWithValue("@thursday", hours.thursday);
            command.Parameters.AddWithValue("@friday", hours.friday);
            command.Parameters.AddWithValue("@saturday", hours.saturday);
            command.Parameters.AddWithValue("@is_exception", hours.isException);
            if (hours.isException)
            {
                command.Parameters.AddWithValue("@start_date", hours.startDate);
                command.Parameters.AddWithValue("@end_date", hours.endDate);
            }
        }

        private void AddImages(int parkId, Image[] images, SqlConnection connection)
        {
            foreach (Image image in images)
            {
                string cmdText = "AddImage";
                SqlCommand command = new SqlCommand();
                command.CommandText = cmdText;
                command.Connection = connection;
                command.CommandType = System.Data.CommandType.StoredProcedure;
                AddImageParameters(parkId, image, command);
                command.ExecuteNonQuery();
            }
        }

        private void AddImageParameters(int parkId, Image image, SqlCommand command)
        {
            command.Parameters.AddWithValue("@national_park_id", parkId);
            command.Parameters.AddWithValue("@url", image.url);
            command.Parameters.AddWithValue("@title", image.title);
            command.Parameters.AddWithValue("@caption", image.caption);
            command.Parameters.AddWithValue("@alt_text", image.altText);
            command.Parameters.AddWithValue("@credit", image.credit);
        }
        private void AddNationalParkParameters(NationalParkServiceJsonData parkData, SqlCommand command)
        {
            command.Parameters.AddWithValue("@fullname", parkData.fullName);
            command.Parameters.AddWithValue("@park_code", parkData.parkCode);
            command.Parameters.AddWithValue("@designation", parkData.designation);
            command.Parameters.AddWithValue("@name", parkData.name);
            command.Parameters.AddWithValue("@state", parkData.states);
            command.Parameters.AddWithValue("@longitude", parkData.longitude);
            command.Parameters.AddWithValue("@latitude", parkData.latitude);
            command.Parameters.AddWithValue("@description", parkData.description);
            command.Parameters.AddWithValue("@directions", parkData.directionsInfo);
            command.Parameters.AddWithValue("@directions_url", parkData.directionsUrl);
            command.Parameters.AddWithValue("@url", parkData.url);
        }

        private IEnumerable<GenericHours> FlattenHours(Operatinghour operatinghour)
        {
            List<GenericHours> hours = new List<GenericHours>();
            hours.Add(new GenericHours(operatinghour));

            if (operatinghour.exceptions != null && operatinghour.exceptions.Length > 0)
            {
                foreach (Exception exception in operatinghour.exceptions)
                {
                    hours.Add(new GenericHours(exception));
                }
            }

            return hours;
        }
    }

    class GenericHours
    {
        public string? wednesday { get; set; }
        public string? monday { get; set; }
        public string? thursday { get; set; }
        public string? sunday { get; set; }
        public string? tuesday { get; set; }
        public string? friday { get; set; }
        public string? saturday { get; set; }
        public DateTime? startDate { get; set; }
        public string? name { get; set; }
        public DateTime? endDate { get; set; }
        public string? description { get; set; }
        public bool isException { get; }
        public GenericHours(Exception exception)
        {
            this.wednesday = exception.exceptionHours.wednesday;
            this.monday = exception.exceptionHours.monday;
            this.thursday = exception.exceptionHours.thursday;
            this.sunday = exception.exceptionHours.sunday;
            this.tuesday = exception.exceptionHours.tuesday;
            this.friday = exception.exceptionHours.friday;
            this.saturday = exception.exceptionHours.saturday;
            this.name = exception.name;
            this.startDate = DateTime.Parse(exception.startDate);
            this.endDate = DateTime.Parse(exception.endDate);
            this.isException = true;
        }

        public GenericHours(Operatinghour operatinghour)
        {
            this.wednesday = operatinghour.standardHours.wednesday;
            this.monday = operatinghour.standardHours.monday;
            this.thursday = operatinghour.standardHours.thursday;
            this.sunday = operatinghour.standardHours.sunday;
            this.tuesday = operatinghour.standardHours.tuesday;
            this.friday = operatinghour.standardHours.friday;
            this.saturday = operatinghour.standardHours.saturday;
            this.name = operatinghour.name;
            this.description = operatinghour.description;
            this.isException = false;
        }
    }
}
