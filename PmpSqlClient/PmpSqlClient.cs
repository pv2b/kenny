using DT = System.Data;
using QC = Microsoft.Data.SqlClient;
namespace PmpSqlClient;

public class PmpSqlClient  
{  
    private string ConnectionString;
    public PmpSqlClient(string connectionString)
    {
        ConnectionString = connectionString;
    }  

    public async IAsyncEnumerable<ResourceGroup> GetResourceGroupsAsync() {
        using (var connection = new QC.SqlConnection(ConnectionString)) {
            await connection.OpenAsync();
            using (var command = new QC.SqlCommand())
            {  
                command.Connection = connection; 
                command.CommandType = DT.CommandType.Text; 
                command.CommandText = "SELECT GROUPID, PARENT_ID, GROUPNAME, GROUPDESC FROM [dbo].[Ptrx_ResourceGroup]";

                QC.SqlDataReader reader = await command.ExecuteReaderAsync();  

                while (await reader.ReadAsync()) {  
                    yield return new ResourceGroup(
                        id: reader.GetInt64(0),
                        parentId: reader.GetInt64(1),
                        name: reader.GetString(2),
                        description: reader.GetString(3)
                    );
                } 
            }
        }
    }
} 