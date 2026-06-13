using Microsoft.Data.Sqlite;
namespace websiteServer;

public class Database
{
    public static void Init(string dbPath)
    {
        bool create = !File.Exists(dbPath);
        create = create && File.Exists(dbPath);
        Console.WriteLine($"Database {dbPath} created.");
        var connStr = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();

        using var conn = new SqliteConnection(connStr);
        conn.Open();

        using var cmd = conn.CreateCommand();

        // users
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT NOT NULL UNIQUE,
                email TEXT,
                password_hash TEXT NOT NULL,
                phone_number TEXT,
                birth_date TEXT,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP
            );
        ";
        cmd.ExecuteNonQuery();
        
        
        // Add online/offline tracking columns
        cmd.CommandText = @"
    ALTER TABLE users ADD COLUMN online INTEGER NOT NULL DEFAULT 0;
";
        try { cmd.ExecuteNonQuery(); } catch { }

        cmd.CommandText = @"
    ALTER TABLE users ADD COLUMN last_seen TEXT DEFAULT CURRENT_TIMESTAMP;
";
        try { cmd.ExecuteNonQuery(); } catch { }


        // sessions
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS sessions (
                token TEXT PRIMARY KEY,
                user_id INTEGER NOT NULL,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY(user_id) REFERENCES users(id)
            );
        ";
        cmd.ExecuteNonQuery();

        // messages
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS messages (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                chat_id TEXT NOT NULL,
                user_id INTEGER NOT NULL,
                content TEXT NOT NULL,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY(user_id) REFERENCES users(id)
            );
        ";
        cmd.ExecuteNonQuery();

        // files (metadata)
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS files (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                filename TEXT,
                mime TEXT,
                size INTEGER,
                path TEXT,
                uploaded_by INTEGER,
                uploaded_at DATETIME DEFAULT CURRENT_TIMESTAMP
            );
        ";
        cmd.ExecuteNonQuery();
        
        //CHATS
        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Chats (
                        Id TEXT PRIMARY KEY,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";
        cmd.ExecuteNonQuery();
        
        //SHARED ROOMS MEMBERS
        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS ChatMembers (
                        ChatId TEXT,
                        UserId INTEGER,
                        FOREIGN KEY(ChatId) REFERENCES Chats(Id),
                        FOREIGN KEY(UserId) REFERENCES Users(Id),
                        PRIMARY KEY(ChatId, UserId)
                    );";
        cmd.ExecuteNonQuery();
    }
    
    public static int SaveFile(string filename, string mime, long size, string path, int userId)
    {
        var connStr = new SqliteConnectionStringBuilder
        {
            DataSource = "server.db" 
        }.ToString();

        using var conn = new SqliteConnection(connStr);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        INSERT INTO files (filename, mime, size, path, uploaded_by)
        VALUES ($filename, $mime, $size, $path, $uploaded_by);
        SELECT last_insert_rowid();
    ";

        cmd.Parameters.AddWithValue("$filename", filename);
        cmd.Parameters.AddWithValue("$mime", mime);
        cmd.Parameters.AddWithValue("$size", size);
        cmd.Parameters.AddWithValue("$path", path);
        cmd.Parameters.AddWithValue("$uploaded_by", userId);

        long id = (long)cmd.ExecuteScalar();
        return (int)id;
    }
    
}