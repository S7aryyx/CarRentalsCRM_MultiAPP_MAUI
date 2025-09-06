using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MI_Cars.Models;
using Npgsql;

namespace MI_Cars.Services
{
    public class PostgresService
    {
        private readonly string connectionString =
            "Host=localhost;Username=postgres;Password=***;Database=mi_cars_crm_01;";

        // ------------------- Машины -------------------
        public async Task<List<Car>> GetCarsAsync()
        {
            var list = new List<Car>();
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            var sql = "SELECT car_id, brand, model, plate_number, color FROM cars";
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new Car
                {
                    CarId = reader.GetInt32(0),
                    Brand = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Model = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    PlateNumber = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Color = reader.IsDBNull(4) ? "" : reader.GetString(4)
                });
            }

            return list;
        }

        public async Task<int> AddCarAsync(Car car)
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO cars (brand, model, plate_number, color) 
                        VALUES (@brand,@model,@plate,@color) RETURNING car_id";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("brand", car.Brand ?? "");
            cmd.Parameters.AddWithValue("model", car.Model ?? "");
            cmd.Parameters.AddWithValue("plate", car.PlateNumber ?? "");
            cmd.Parameters.AddWithValue("color", car.Color ?? "");

            var id = (int)await cmd.ExecuteScalarAsync();
            return id;
        }
        public async Task DeleteCarAsync(string plateNumber)
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            var sql = "DELETE FROM cars WHERE plate_number=@plate";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("plate", plateNumber);

            await cmd.ExecuteNonQueryAsync();
        }


        // ------------------- Клиенты -------------------
        public async Task<List<Client>> GetClientsAsync()
        {
            var list = new List<Client>();
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            var sql = "SELECT client_id, full_name, phone FROM clients";
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new Client
                {
                    ClientId = reader.GetInt32(0),
                    FullName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Phone = reader.IsDBNull(2) ? "" : reader.GetString(2)
                });
            }

            return list;
        }

        // ------------------- Аренды -------------------
        public async Task<int> AddRentalAsync(Rental rental)
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            var sql = @"INSERT INTO rentals (car_id, client_id, start_date, end_date, daily_rate)
                        VALUES (@car, @client, @start, @end, @rate) 
                        RETURNING rental_id";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("car", rental.CarId);
            cmd.Parameters.AddWithValue("client", rental.ClientId);
            cmd.Parameters.AddWithValue("start", rental.StartDate);
            cmd.Parameters.AddWithValue("end", rental.EndDate);
            cmd.Parameters.AddWithValue("rate", rental.DailyRate);

            var id = (int)await cmd.ExecuteScalarAsync();
            return id;
        }

        public async Task<List<Rental>> GetRentalsAsync()
        {
            var list = new List<Rental>();
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT r.rental_id, r.car_id, r.client_id, r.start_date, r.end_date, r.daily_rate,
                       c.client_id, c.full_name, c.phone
                FROM rentals r
                JOIN clients c ON r.client_id = c.client_id";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var rental = new Rental
                {
                    RentalId = reader.GetInt32(0),
                    CarId = reader.GetInt32(1),
                    ClientId = reader.GetInt32(2),
                    StartDate = reader.GetDateTime(3),
                    EndDate = reader.GetDateTime(4),
                    DailyRate = reader.GetDecimal(5),
                    Client = new Client
                    {
                        ClientId = reader.GetInt32(6),
                        FullName = reader.IsDBNull(7) ? "" : reader.GetString(7),
                        Phone = reader.IsDBNull(8) ? "" : reader.GetString(8)
                    }
                };

                list.Add(rental);
            }

            return list;
        }

        // ------------------- Фото -------------------
        public async Task<List<Photo>> GetPhotosByRentalAsync(int rentalId)
        {
            var list = new List<Photo>();
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            var sql = @"SELECT photo_id, rental_id, photo_path, photo_type 
                        FROM rental_photos 
                        WHERE rental_id=@rentalId";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("rentalId", rentalId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Photo
                {
                    PhotoId = reader.GetInt32(0),
                    RentalId = reader.GetInt32(1),
                    Path = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    PhotoType = reader.IsDBNull(3) ? "" : reader.GetString(3)
                });
            }

            return list;
        }
    }
}
