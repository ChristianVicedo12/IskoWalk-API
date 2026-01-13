using MySqlConnector;
using Dapper;
using IskoWalkAPI.Models;

namespace IskoWalkAPI.Services;

public class MySqlService
{
    private readonly string _connectionString;
    private readonly ILogger<MySqlService> _logger;

    public MySqlService(IConfiguration configuration, ILogger<MySqlService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _logger = logger;
    }

    private MySqlConnection GetConnection()
    {
        return new MySqlConnection(_connectionString);
    }

    // ==================== USER METHODS ====================
    
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        using var connection = GetConnection();
        var sql = "SELECT * FROM Users WHERE Email = @Email";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        using var connection = GetConnection();
        var sql = "SELECT * FROM Users WHERE Id = @UserId";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
    }

    public async Task<User> CreateUserAsync(string fullName, string email, string passwordHash, string? contactNumber = null)
    {
        using var connection = GetConnection();
        var sql = @"
            INSERT INTO Users (FullName, Email, PasswordHash, ContactNumber, CreatedAt, UpdatedAt)
            VALUES (@FullName, @Email, @PasswordHash, @ContactNumber, @CreatedAt, @UpdatedAt);
            SELECT * FROM Users WHERE Id = LAST_INSERT_ID();";
        
        var user = await connection.QueryFirstAsync<User>(sql, new
        {
            FullName = fullName,
            Email = email,
            PasswordHash = passwordHash,
            ContactNumber = contactNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        
        return user;
    }

    // ==================== WALK REQUEST METHODS ====================

    public async Task<List<WalkRequest>> GetAvailableRequestsAsync(int userId)
    {
        using var connection = GetConnection();
        var sql = @"
            SELECT * FROM WalkRequests 
            WHERE Status = 'pending' 
            AND RequesterId != @UserId 
            ORDER BY CreatedAt DESC";
        
        var requests = await connection.QueryAsync<WalkRequest>(sql, new { UserId = userId });
        return requests.ToList();
    }

    public async Task<List<WalkRequest>> GetActiveRequestsAsync(int userId)
    {
        using var connection = GetConnection();
        var sql = @"
            SELECT * FROM WalkRequests 
            WHERE RequesterId = @UserId 
            AND Status IN ('pending', 'accepted') 
            ORDER BY CreatedAt DESC";
        
        var requests = await connection.QueryAsync<WalkRequest>(sql, new { UserId = userId });
        return requests.ToList();
    }

    public async Task<List<WalkRequest>> GetAcceptedRequestsAsync(int userId)
    {
        using var connection = GetConnection();
        var sql = @"
            SELECT * FROM WalkRequests 
            WHERE CompanionId = @UserId 
            AND Status = 'accepted' 
            ORDER BY CreatedAt DESC";
        
        var requests = await connection.QueryAsync<WalkRequest>(sql, new { UserId = userId });
        return requests.ToList();
    }

    public async Task<WalkRequest?> AcceptRequestAsync(int requestId, int companionId, string companionName)
    {
        using var connection = GetConnection();
        var sql = @"
            UPDATE WalkRequests 
            SET Status = 'accepted', 
                CompanionId = @CompanionId, 
                CompanionName = @CompanionName,
                UpdatedAt = @UpdatedAt
            WHERE Id = @RequestId AND Status = 'pending';
            SELECT * FROM WalkRequests WHERE Id = @RequestId;";
        
        var result = await connection.QueryFirstOrDefaultAsync<WalkRequest>(sql, new
        {
            RequestId = requestId,
            CompanionId = companionId,
            CompanionName = companionName,
            UpdatedAt = DateTime.UtcNow
        });
        
        return result;
    }

    public async Task<WalkRequest> CreateRequestAsync(WalkRequest request)
    {
        using var connection = GetConnection();
        var sql = @"
            INSERT INTO WalkRequests 
            (RequesterId, RequesterName, FromLocation, ToLocation, WalkTime, WalkDate, 
             Status, AttireDescription, AdditionalNotes, ContactNumber, CreatedAt, UpdatedAt)
            VALUES 
            (@RequesterId, @RequesterName, @FromLocation, @ToLocation, @WalkTime, @WalkDate,
             @Status, @AttireDescription, @AdditionalNotes, @ContactNumber, @CreatedAt, @UpdatedAt);
            SELECT * FROM WalkRequests WHERE Id = LAST_INSERT_ID();";

        var newRequest = await connection.QueryFirstAsync<WalkRequest>(sql, new
        {
            RequesterId = request.RequesterId,
            RequesterName = request.RequesterName,
            FromLocation = request.FromLocation,
            ToLocation = request.ToLocation,
            WalkTime = request.WalkTime,
            WalkDate = request.WalkDate,
            Status = "pending",
            AttireDescription = request.AttireDescription,
            AdditionalNotes = request.AdditionalNotes,
            ContactNumber = request.ContactNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        return newRequest;
    }
}
