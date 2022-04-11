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
                command.CommandText = @"SELECT A.GROUPID, A.PARENT_ID, A.GROUPNAME, A.GROUPDESC, B.ACCESSTYPE, C.ADSPATH
                    FROM [dbo].[Ptrx_ResourceGroup] AS A
                    INNER JOIN [PassTrix].[dbo].[Ptrx_ResGrpUserGroup] AS B ON A.[GROUPID] = B.[GROUPID]
                    INNER JOIN [PassTrix].[dbo].[Ptrx_ActiveDirectoryGroupSyncDetails] AS C ON B.[USERGROUPID] = C.[GROUP_ID]
                    ORDER BY A.GROUPID;
                    ";

                QC.SqlDataReader reader = await command.ExecuteReaderAsync();  

                ResourceGroup? current = null;

                while (await reader.ReadAsync()) {  
                    long groupId = reader.GetInt64(0);
                    if (groupId != current?.Id) {
                        if (current != null) yield return current;
                        current = new ResourceGroup(
                            id: groupId,
                            parentId: reader.GetInt64(1),
                            name: reader.GetString(2),
                            description: reader.GetString(3)
                        );
                    }
                    switch (reader.GetString(4) /* accesstype */) {
                        case "readonly":
                        case "readwrite":
                        case "complete":
                            current.AllowGroups.Add(reader.GetString(5) /* adstype */);
                            break;

                        default:
                            break;
                    }
                } 
                if (current != null) yield return current;
            }
        }
    }
} 