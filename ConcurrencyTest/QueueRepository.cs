using Dapper;

namespace ConcurrencyApp;

public sealed class QueueRepository(DbConnector connector)
{
    private const int InvisibilityTimeout = 30;
    public async Task Insert(int numOfRecords)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@NumOfMessages", numOfRecords);

        using var connection = await connector.Connect();
        await connection.ExecuteAsync("""
                ;WITH msgs AS (
                	SELECT
                		CONCAt('MSG::', NEWID()) AS Message,
                		1 As MsgNum
                		UNION ALL
                	SELECT
                		CONCAt('MSG::', NEWID()),
                		m.MsgNum + 1
                	FROM msgs m
                	WHERE m.MsgNum < @NumOfMessages
                )
                INSERT INTO dbo.QueuedMessage (Message)
                SELECT Message
                FROM msgs
                OPTION(MAXRECURSION 10000);
                """, parameters);
    }

    public async Task Delete(int id)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);

        using var connection = await connector.Connect();
        await connection.ExecuteAsync("""
            DELETE dbo.QueuedMessage
            WHERE Id = @Id;
            """, parameters);
    }

    public async Task<IEnumerable<QueuedMessage>> GetNextBatch(int numOfRecords)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@RecCount", numOfRecords);
        parameters.Add("@Timeout", InvisibilityTimeout);

        using var connection = await connector.Connect();
        return await connection.QueryAsync<QueuedMessage>(GetNextBatchSql, parameters);
    }

    private const string GetNextBatchSql = """
        DECLARE @InvisibleUntil DATETIME = DATEADD(SECOND, @Timeout, GETDATE())
        UPDATE TOP (@RecCount) dbo.QueuedMessage
        SET InvisibleUntil = @InvisibleUntil
        OUTPUT inserted.*
        WHERE InvisibleUntil <= GETDATE();
        """;

    private const string CreateTable = """
        DROP TABLE dbo.QueuedMessage;

        CREATE TABLE dbo.QueuedMessage (
        	Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
        	Message NVARCHAR(MAX) NULL,
        	CreatedOn DATETIME NOT NULL DEFAULT (GETDATE()),
        	InvisibleUntil DATETIME NOT NULL DEFAULT (GETDATE()),
        	INDEX IX_QueuedMessage_Timeout NONCLUSTERED (InvisibleUntil)
        );
        """;
}
