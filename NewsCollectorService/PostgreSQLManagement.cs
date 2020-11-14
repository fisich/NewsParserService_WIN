using NewsParsingUtils;
using Npgsql;

public enum PostgreSQLState
{
    Unknown = 0,
    Running = 1,
    ConnectionError = 2,
    DatabaseInitError = 3,
    ExecutionError = 4,
    ExecutionCompleted = 5
}

class PostgreSQLManagement
{
    public static string connectString = "Host=localhost;Username=postgres;Password=$PASSWORD";
    public static string databaseName = "news_parser";
    private string lastQuery = string.Empty;
    private PostgreSQLState state;
    public PostgreSQLManagement()
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(connectString))
        {
            try
            {
                connection.Open();
                state = PostgreSQLState.Running;
            }
            catch
            {
                state = PostgreSQLState.ConnectionError;
                return;
            }
            using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT datname FROM pg_catalog.pg_database WHERE (datname) = ('" + databaseName + "') ", connection))
            {
                var answer = cmd.ExecuteScalar();
                if (answer == null)
                {
                    InitializeDatabase();
                }
            }
        }
    }

    public PostgreSQLState GetState()
    {
        return state;
    }

    private void InitializeDatabase()
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(connectString))
        {
            try
            {
                connection.Open();
                state = PostgreSQLState.Running;
            }
            catch
            {
                state = PostgreSQLState.ConnectionError;
                return;
            }
            using (NpgsqlCommand cmd = new NpgsqlCommand("CREATE DATABASE " + databaseName, connection))
            {
                try
                {
                    var answer = cmd.ExecuteNonQuery();
                }
                catch
                {
                    state = PostgreSQLState.DatabaseInitError;
                    return;
                }
            }
        }
        InitializeTables();
    }

    private void InitializeTables()
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(connectString + ";Database=" + databaseName))
        {
            try
            {
                connection.Open();
                state = PostgreSQLState.Running;
            }
            catch
            {
                state = PostgreSQLState.ConnectionError;
                return;
            }
            using (NpgsqlCommand cmd = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS \"news\" (\"id\" serial NOT NULL, \"title\" varchar(255) NOT NULL," +
                "\"annotation\" varchar(1023) NOT NULL, \"url\" varchar(255) NOT NULL UNIQUE, \"id_source\" bigint NOT NULL," +
                "\"publication_date\" timestamp with time zone NOT NULL, \"upload_date\" timestamp with time zone NOT NULL," +
                "CONSTRAINT \"news_pk\" PRIMARY KEY (\"id\")) WITH(OIDS = FALSE);" +
                "CREATE TABLE IF NOT EXISTS \"news_source\" (\"id\" serial NOT NULL, \"name\" varchar(63) NOT NULL, \"category\" varchar(63) NOT NULL," +
                "\"url\" varchar(255) NOT NULL UNIQUE, CONSTRAINT \"news_source_pk\" PRIMARY KEY (\"id\")) WITH(OIDS = FALSE);" +
                "ALTER TABLE \"news\" DROP CONSTRAINT IF EXISTS \"news_fk0\";" +
                "ALTER TABLE \"news\" ADD CONSTRAINT \"news_fk0\" FOREIGN KEY (\"id_source\") REFERENCES \"news_source\"(\"id\");"
                , connection))
            {
                try
                {
                    var answer = cmd.ExecuteNonQuery();
                }
                catch
                {
                    state = PostgreSQLState.DatabaseInitError;
                    return;
                }
            }
        }
    }

    public string GetLastQuery()
    {
        return lastQuery;
    }

    public PostgreSQLState InsertNewsItems(IBaseNewsParser parser)
    {
        string sourceId = InsertNewsSource(parser);
        if(string.IsNullOrEmpty(sourceId))
        {
            return state;
        }
        using (NpgsqlConnection connection = new NpgsqlConnection(connectString + ";Database=" + databaseName))
        {
            try
            {
                connection.Open();
                state = PostgreSQLState.Running;
            }
            catch
            {
                return state = PostgreSQLState.ConnectionError;
            }
            lastQuery = "INSERT INTO news (title, annotation, url, id_source, publication_date, upload_date) VALUES\n";
            foreach (var item in parser.GetNewsItems())
            {
                if (string.IsNullOrEmpty(item.date))
                    continue;
                string annotation = item.annotation;
                if(annotation.Length > 1023)
                {
                    annotation = annotation.Remove(1019);
                    annotation += "...";
                }
                lastQuery += "('" + item.title.Replace("'", "''") + "','" + annotation.Replace("'", "''") + "','" + item.newsUrl.Replace("'", "''") + "','" + sourceId.Replace("'", "''") + "','" + item.date + "', (SELECT * FROM transaction_timestamp())),\n";
            }
            lastQuery = lastQuery.Remove(lastQuery.Length - 2);
            lastQuery += "\nON CONFLICT DO NOTHING;";
            using (NpgsqlCommand cmd = new NpgsqlCommand(lastQuery, connection))
            {
                try
                {
                    var answer = cmd.ExecuteNonQuery();
                    return state = PostgreSQLState.ExecutionCompleted;
                }
                catch
                {
                    return state = PostgreSQLState.ExecutionError;
                }
            }
        }
    }

    private string InsertNewsSource(IBaseNewsParser parser)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(connectString + ";Database=" + databaseName))
        {
            try
            {
                connection.Open();
                state = PostgreSQLState.Running;
            }
            catch
            {
                state = PostgreSQLState.ConnectionError;
                return string.Empty;
            }
            using (NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO news_source (name, category, url) VALUES ('" +
                parser.GetName() + "','" + parser.GetCategory() + "','" + parser.GetUrl() + "') ON CONFLICT DO NOTHING;"
                , connection))
            {
                cmd.ExecuteNonQuery();
            }
            using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT id FROM news_source WHERE name = '" + parser.GetName() + "'", connection))
            {
                return cmd.ExecuteScalar().ToString();
            }
        }
    }
}
