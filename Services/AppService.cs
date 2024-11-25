using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;  // Add caching
using System.Threading.Tasks;
using ApiAppPay.Models;
using System;
using System.Collections.Generic;

public class AppService
{
    private readonly string _connectionString;
    private readonly IMemoryCache _cache;  // MemoryCache dependency

    public AppService(IConfiguration configuration, IMemoryCache memoryCache)
    {
        // ConnetionString from appsettings.json
        _connectionString = configuration.GetConnectionString("sdms");
        _cache = memoryCache;  // Inject memory cache
    }

    public async Task<Application> GetAppByIdAsync(long applicationId)
    {
        Application? application = null;

        try
        {
            // Check if the data is in cache first
            if (_cache.TryGetValue(applicationId, out application))
            {
                // Return from cache
                return application;
            }

            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT APPLICATION_ID, CREATED, USE_CASE, LICENSE_NUMBER, LICENSE_NUMBER_LATIN, BRAND, APP_CHASSIS_NUMBER, VEHICLE_TYPE  " +
                              "FROM SDMS_IRQDLVR.T_APPLICATION " +
                              "WHERE APPLICATION_ID = :APPLIC_ID";

                using (var command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(":APPLIC_ID", OracleDbType.Char).Value = applicationId;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            application = new Application
                            {
                                Application_ID = reader.GetString(reader.GetOrdinal("APPLICATION_ID")),
                                Created = reader.GetDateTime(reader.GetOrdinal("CREATED")),
                                UseCase = reader.IsDBNull(reader.GetOrdinal("USE_CASE"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("USE_CASE")),
                                LicenseNumber = reader.IsDBNull(reader.GetOrdinal("LICENSE_NUMBER"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("LICENSE_NUMBER")),
                                LicenseNumberLatin = reader.IsDBNull(reader.GetOrdinal("LICENSE_NUMBER_LATIN"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("LICENSE_NUMBER_LATIN")),
                                Brand = reader.IsDBNull(reader.GetOrdinal("BRAND"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("BRAND")),
                                AppChassisNumber = reader.IsDBNull(reader.GetOrdinal("APP_CHASSIS_NUMBER"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("APP_CHASSIS_NUMBER")),
                                VehicleType = reader.IsDBNull(reader.GetOrdinal("VEHICLE_TYPE"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("VEHICLE_TYPE")),
                                            };

                            // Cache the result for a specified time (e.g., 5 minutes)
                            _cache.Set(applicationId, application, TimeSpan.FromMinutes(5));
                        }
                    }
                }
            }
        }
        catch (OracleException ex)
        {
            throw new Exception("Database operation failed", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while fetching user data.", ex);
        }

        return application;
    }
}

