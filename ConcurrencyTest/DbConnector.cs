using System.Data;
using System.Data.SqlClient;

namespace ConcurrencyApp;

public sealed class DbConnector
{
    public async Task<IDbConnection> Connect()
    {
        var connection = new SqlConnection("Server=localhost,1433;Database=App;User=sa;Password=P@ssw0rd;Encrypt=False");
        await connection.OpenAsync();
        return connection;
    }
}
