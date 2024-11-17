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

                string query = "SELECT APPLICATION_ID, ENROLLMENT_SITE, USE_CASE, GOVERNORATE, LICENSE_NUMBER, LICENSE_NUMBER_LATIN, USAGE " +
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
                                EnrollmentSite = reader.IsDBNull(reader.GetOrdinal("ENROLLMENT_SITE"))? null // add nullable in db (maybe return null value)

                                    : reader.GetString(reader.GetOrdinal("ENROLLMENT_SITE")),
                                UseCase = reader.IsDBNull(reader.GetOrdinal("USE_CASE"))? null  // add nullable in db (maybe return null value)

                                    : reader.GetString(reader.GetOrdinal("USE_CASE")),
                                Governorate = reader.IsDBNull(reader.GetOrdinal("GOVERNORATE"))? null // add nullable in db (maybe return null value)

                                    : reader.GetString(reader.GetOrdinal("GOVERNORATE")),
                                LicenseNumber = reader.IsDBNull(reader.GetOrdinal("LICENSE_NUMBER"))
                                    ? null                                                               // add nullable in db (maybe return null value)

                                    : reader.GetString(reader.GetOrdinal("LICENSE_NUMBER")),
                                LicenseNumberLatin = reader.IsDBNull(reader.GetOrdinal("LICENSE_NUMBER_LATIN"))
                                    ? null                                                                 // add nullable in db (maybe return null value)

                                    : reader.GetString(reader.GetOrdinal("LICENSE_NUMBER_LATIN")),
                                Usage = reader.IsDBNull(reader.GetOrdinal("USAGE"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("USAGE")),
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

    // Implement Pagination
    public async Task<List<Application>> GetAppsAsync(int pageNumber, int pageSize)
    {
        List<Application> applications = new List<Application>();

        try
        {
            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                    SELECT * FROM (
                        SELECT APPLICATION_ID, ENROLLMENT_SITE, USE_CASE, GOVERNORATE, LICENSE_NUMBER, LICENSE_NUMBER_LATIN, USAGE,
                               ROW_NUMBER() OVER (ORDER BY APPLICATION_ID) AS RowNum
                        FROM SDMS_IRQDLVR.T_APPLICATION
                    ) AS PagedData
                    WHERE RowNum BETWEEN :StartRow AND :EndRow";

                using (var command = new OracleCommand(query, connection))
                {
                    // Calculate the row range based on the page number and page size
                    int startRow = (pageNumber - 1) * pageSize + 1;
                    int endRow = pageNumber * pageSize;

                    command.Parameters.Add(":StartRow", OracleDbType.Int32).Value = startRow;
                    command.Parameters.Add(":EndRow", OracleDbType.Int32).Value = endRow;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            applications.Add(new Application
                            {
                                Application_ID = reader.GetString(reader.GetOrdinal("APPLICATION_ID")),
                                EnrollmentSite = reader.IsDBNull(reader.GetOrdinal("ENROLLMENT_SITE"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("ENROLLMENT_SITE")),
                                UseCase = reader.IsDBNull(reader.GetOrdinal("USE_CASE"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("USE_CASE")),
                                Governorate = reader.IsDBNull(reader.GetOrdinal("GOVERNORATE"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("GOVERNORATE")),
                                LicenseNumber = reader.IsDBNull(reader.GetOrdinal("LICENSE_NUMBER"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("LICENSE_NUMBER")),
                                LicenseNumberLatin = reader.IsDBNull(reader.GetOrdinal("LICENSE_NUMBER_LATIN"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("LICENSE_NUMBER_LATIN")),
                                Usage = reader.IsDBNull(reader.GetOrdinal("USAGE"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("USAGE")),
                            });
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

        return applications;
    }
}

