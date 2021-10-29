#wip sql utility class to hide away some of the boilerplate around
#https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient?view=dotnet-plat-ext-5.0
#add support for multiple authentication modes and potentially non-MS driver support, is mssql only right now. 
#would like to try and reflection.assembly load the dll for https://www.npgsql.org and see if it will work in a pwsh setting...

using namespace System.Data
using namespace System.Collections.Generic

class SQL {
    [String] $Server 
    [String] $Database 
    [String] $query
    
    SQL ( ) {}

    SQL (
        $_server,
        $_db,
        $_query
    ) {
        #maybe put null handling in ctors ?
        $this.Server = $_server
        $this.Database = $_db
        $this.query = $_query
    }

    SQL (
        $_server,
        $_db
    ) {
        $this.Server = $_server
        $this.Database = $_db
    }
    
    [void]
    NotNull
    {
          if ( (
                $null -ne $this.db  -and 
                $null -ne $this.server -and
                $null -ne $this.query)){
                }
                else {
                  throw [Exception]::new("Cannot execute query with null parameters, ~nset server and/or db and pass in a query before running") 
                }
    }
    
    #call this.query, based on constructor based values
    [System.Collections.Generic.List[DataRow]] 
    ExecuteQuery() 
    {        
        $this.NotNull()
        [SqlClient.SqlConnection]$Connection = [SqlClient.SqlConnection]::new()
        [SqlClient.SqlCommand]$Command = [SqlClient.SqlCommand]::new()
        [SqlClient.SqlDataAdapter]$Adapter = [SqlClient.SqlDataAdapter]::new()
        [Dataset]$Dataset = [DataSet]::new()
        $Connection.ConnectionString = "Server = $($this.Server); Database = $($this.Database); Integrated Security = True;"
        $Command.CommandText = $this.query
        $Command.Connection = $Connection
        $Adapter.SelectCommand = $Command
        $Adapter.Fill($DataSet) | Out-Null
        $Connection.Dispose();
        return ([System.Collections.Generic.List[DataRow]]$Dataset.Tables.Rows)
    }
    
    #static version useful-ish
    static
    [System.Collections.Generic.List[DataRow]] 
    ExecuteQuery([String]$q) 
    {
        $this.NotNull()
        [SqlClient.SqlConnection]$Connection = [SqlClient.SqlConnection]::new()
        [SqlClient.SqlCommand]$Command = [SqlClient.SqlCommand]::new()
        [SqlClient.SqlDataAdapter]$Adapter = [SqlClient.SqlDataAdapter]::new()
        [Dataset]$Dataset = [DataSet]::new()
        $Connection.ConnectionString = "Server = $($this.Server); Database = $($this.Database); Integrated Security = True;"
        $Command.CommandText = $q
        $Command.Connection = $Connection
        $Adapter.SelectCommand = $Command
        $Adapter.Fill($DataSet) | Out-Null
        $Connection.Dispose();
        return ([System.Collections.Generic.List[DataRow]]$Dataset.Tables.Rows)
    }
    
    #many queries, same connection
    [System.Collections.Generic.List[DataRow]] 
    ExecuteMany([String[]]$qs) 
    {
        $this.NotNull()
        [SqlClient.SqlConnection]$Connection = [SqlClient.SqlConnection]::new()
        [SqlClient.SqlCommand]$Command = [SqlClient.SqlCommand]::new()
        [SqlClient.SqlDataAdapter]$Adapter = [SqlClient.SqlDataAdapter]::new()
        [Dataset]$Dataset = [DataSet]::new()
        $Connection.ConnectionString = "Server = $($this.Server); Database = $($this.Database); Integrated Security = True;"
        $qs.foreach({
            $q = $_
            $Command.CommandText = $q
            $Command.Connection = $Connection
            $Adapter.SelectCommand = $Command
            try {
                $Adapter.Fill($DataSet) | out-null
            }
            catch {
                [Console]::WriteLine($q)
                [Console]::WriteLine($_)

            } 
        })
        $Connection.Dispose();
        return ([System.Collections.Generic.List[DataRow]]$Dataset.Tables.Rows)
    }
}
