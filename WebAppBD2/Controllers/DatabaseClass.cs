using Npgsql;
using WebAppBD2.Controllers;

class Database_Queries : IDisposable
{
	public NpgsqlConnection? Connection_BD { get; set; }
    private readonly ILogger<HomeController> _logger;

	public Database_Queries(ILogger<HomeController> logger) {
		_logger = logger;
	}

    public void ConnectToBD()
	{
		string connString = "Host=students.ami.nstu.ru;Username=pmi-b1309;Password=MalCeer0;Database=students;Search Path=pmib1309";
		try
		{
			this.Connection_BD = new NpgsqlConnection(connString);
			this.Connection_BD.Open();
			_logger.LogInformation("Connection open...");
		}
		catch (Exception ex)
		{
			this.CloseConnectToBD();
			_logger.LogCritical("The connection is closing...");
			_logger.LogCritical(ex.Message);
			throw new Exception(ex.Message);
		}
	}
	public void CloseConnectToBD() => this.Connection_BD?.Close();

	public List<string>? GetListTownPost()
	{
		if (this.Connection_BD == null)
		{
			_logger.LogError("No connection to the server.");
			return null;
		}
		List<string> towns = [];
		try
		{
			var transaction = this.Connection_BD.BeginTransaction();
			_logger.LogInformation("Starting a transaction...");
			var command = this.Connection_BD.CreateCommand();
			command.Transaction = transaction;
			command.CommandText = "SELECT distinct town FROM p";
            _logger.LogInformation("Create command: {}", command.CommandText);
            try
			{
				var reader = command.ExecuteReader();
				_logger.LogInformation("Sending command to server. Receiving result...");
				if (reader.HasRows) {
					_logger.LogInformation("Information received. Processing data...");
					while (reader.Read())
						towns.Add(reader.GetString(0));
                }
                else
				{
					_logger.LogInformation("Empty result received. Transaction completed.");
					reader.Close();
					return null;
				}
				_logger.LogInformation("Data processed. Completing transaction...");
				reader.Close();
				transaction.Commit();
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.Message);
				_logger.LogInformation("Rollback transaction...");
				transaction.Rollback();
			}
			_logger.LogInformation("Transaction completed. Data returned.");
			return towns;
		}
		catch (Exception ex)
		{
            _logger.LogCritical("The connection is closing...");
            _logger.LogCritical(ex.Message);
			this.CloseConnectToBD();
			throw new Exception(ex.Message);
		}
	}
	public object? ExecuteScalarCommand(string com)
	{
		if (this.Connection_BD == null)
		{
			_logger.LogError("No connection to the server.");
			return null;
		}
		object? result = null;
		try
		{
			var transaction = this.Connection_BD.BeginTransaction();
            _logger.LogInformation("Starting a transaction...");
            var command = this.Connection_BD.CreateCommand();
			command.Transaction = transaction;
			command.CommandText = com;
            _logger.LogInformation("Create command: {}", command.CommandText);
            try
			{
				result = command.ExecuteScalar();
                _logger.LogInformation("Sending command to server. Information received. Completing transaction...");
                transaction.Commit();
			}
			catch (Exception ex)
			{
                _logger.LogWarning(ex.Message);
                _logger.LogInformation("Rollback transaction...");
                transaction.Rollback();
			}
            _logger.LogInformation("Transaction completed. Data returned.");
            return result;
		}
		catch (Exception ex)
		{
            _logger.LogCritical("The connection is closing...");
            _logger.LogCritical(ex.Message);
            this.CloseConnectToBD();
			throw new Exception(ex.Message);
		}

	}
	public int ExecuteNonQueryCommand(string com) {
		if (this.Connection_BD == null) {
            _logger.LogError("No connection to the server.");
            return 0;
		}
		int num = 0;
        try
        {
            var transaction = this.Connection_BD.BeginTransaction();
            _logger.LogInformation("Starting a transaction...");
            var command = this.Connection_BD.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = com;
            _logger.LogInformation("Create command: {}", command.CommandText);
            try
            {
                num = command.ExecuteNonQuery();
                _logger.LogInformation("Sending command to server. Completing transaction...");
                transaction.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                _logger.LogInformation("Rollback transaction...");
                transaction.Rollback();
            }
            _logger.LogInformation("Transaction completed. Data returned.");
            return num;
        }
        catch (Exception ex)
        {
            _logger.LogCritical("The connection is closing...");
            _logger.LogCritical(ex.Message);
            this.CloseConnectToBD();
            throw new Exception(ex.Message);
        }

    }
	public Dictionary<string, List<string?>>? ExecuteReaderCommand(string com) {
		if (this.Connection_BD == null)
		{
            _logger.LogError("No connection to the server.");
            return null;
		}
		Dictionary<string, List<string?>> result = [];
		try
        {
            var transaction = this.Connection_BD.BeginTransaction();
            _logger.LogInformation("Starting a transaction...");
            var command = this.Connection_BD.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = com;
            _logger.LogInformation("Create command: {}", command.CommandText);
            try
            {
				var reader = command.ExecuteReader();
                _logger.LogInformation("Sending command to server. Receiving result...");
                if (reader.HasRows)
                {
                    _logger.LogInformation("Information received. Processing data...");
                    for (int i = 0; i < reader.FieldCount; i++)
						result.Add(reader.GetName(i), []);
					while (reader.Read())
						for (int i = 0; i < reader.FieldCount; i++)
						{
							string key = reader.GetName(i);
							string? value = reader.GetValue(i).ToString();
							result[key].Add(value);
						}
				}
                else
                {
                    _logger.LogInformation("Empty result received. Transaction completed.");
                    reader.Close();
                    transaction.Commit();
                    return null;
                }
                _logger.LogInformation("Data processed. Completing transaction...");
                reader.Close();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                _logger.LogInformation("Rollback transaction...");
                transaction.Rollback();
            }
            _logger.LogInformation("Transaction completed. Data returned.");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogCritical("The connection is closing...");
            _logger.LogCritical(ex.Message);
            this.CloseConnectToBD();
            throw new Exception(ex.Message);
        }
    }

    public void Dispose()
    {
        _logger.LogCritical("The connection is closing...");
        this.CloseConnectToBD();
    }

    ~Database_Queries()
	{
		Dispose();
	}

}
