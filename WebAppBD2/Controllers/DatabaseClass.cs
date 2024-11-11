using Npgsql;

class Database_Queries
{
	public NpgsqlConnection? Connection_BD { get; set; }

	public void ConnectToBD()
	{
		string connString = "Host=students.ami.nstu.ru;Username=pmi-b1309;Password=MalCeer0;Database=students;Search Path=pmib1309";
		try
		{
			this.Connection_BD = new NpgsqlConnection(connString);
			this.Connection_BD.Open();
			Console.WriteLine("Соединение открыто\n");
		}
		catch (Exception ex)
		{
			this.CloseConnectToBD();
			throw new Exception(ex.Message);
		}
	}
	public void CloseConnectToBD() => this.Connection_BD?.Close();

	public List<string>? GetListTownPost()
	{
		if (this.Connection_BD == null) return null;
		List<string> towns = [];
		try
		{
			var transaction = this.Connection_BD.BeginTransaction();
			var command = this.Connection_BD.CreateCommand();
			command.Transaction = transaction;
			command.CommandText = "SELECT distinct town FROM p";
			try
			{
				var reader = command.ExecuteReader();
				if (reader.HasRows)
					while (reader.Read())
						towns.Add(reader.GetString(0));
				else
				{
					reader.Close();
					return null;
				}
				reader.Close();
				transaction.Commit();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				transaction.Rollback();
			}
			return towns;
		}
		catch (Exception ex)
		{
			this.CloseConnectToBD();
			throw new Exception(ex.Message);
		}
	}
	public object? ExecuteScalarCommand(string com)
	{
		if (this.Connection_BD == null) return null;
		object? result = null;
		try
		{
			var transaction = this.Connection_BD.BeginTransaction();
			var command = this.Connection_BD.CreateCommand();
			command.Transaction = transaction;
			command.CommandText = com;
			try
			{
				result = command.ExecuteScalar();
				transaction.Commit();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				transaction.Rollback();
			}
			return result;
		}
		catch (Exception ex)
		{
			this.CloseConnectToBD();
			throw new Exception(ex.Message);
		}

	}
	public int ExecuteNonQueryCommand(string com) {
		if (this.Connection_BD == null) return 0;
		int num = 0;
        try
        {
            var transaction = this.Connection_BD.BeginTransaction();
            var command = this.Connection_BD.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = com;
            try
            {
                num = command.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                transaction.Rollback();
            }
            return num;
        }
        catch (Exception ex)
        {
            this.CloseConnectToBD();
            throw new Exception(ex.Message);
        }

    }

    ~Database_Queries()
	{
		this.CloseConnectToBD();
	}

}
