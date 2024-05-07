namespace Customers;

public sealed class CustomerService
{
    private readonly DbConnectionProvider _dbConnectionProvider;

    public CustomerService(DbConnectionProvider dbConnectionProvider)
    {
        _dbConnectionProvider = dbConnectionProvider;
        CreateCustomersTable();
    }

    public IEnumerable<Customer> GetCustomers()
    {
        IList<Customer> customers = new List<Customer>();

        using var connection = _dbConnectionProvider.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name FROM customers";
        command.Connection?.Open();

        using var dataReader = command.ExecuteReader();
        while (dataReader.Read())
        {
            var id = dataReader.GetInt64(0);
            var name = dataReader.GetString(1);
            customers.Add(new Customer(id, name));
        }

        return customers;
    }

    public void Create(Customer customer)
    {
        using var connection = _dbConnectionProvider.GetConnection();
        using var command = connection.CreateCommand();

        var id = command.CreateParameter();
        id.ParameterName = "@id";
        id.Value = customer.Id;

        var name = command.CreateParameter();
        name.ParameterName = "@name";
        name.Value = customer.Name;

        command.CommandText = "INSERT INTO customers (id, name) VALUES(@id, @name)";
        command.Parameters.Add(id);
        command.Parameters.Add(name);
        command.Connection?.Open();
        command.ExecuteNonQuery();
    }

    private void CreateCustomersTable()
    {
        using var connection = _dbConnectionProvider.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS customers (id BIGINT NOT NULL, name VARCHAR NOT NULL, PRIMARY KEY (id))";
        command.Connection?.Open();
        command.ExecuteNonQuery();
    }

    public Customer? GetCustomerById(int id)
    {
        using var connection = _dbConnectionProvider.GetConnection();
        using var command = connection.CreateCommand();

        var idParam = command.CreateParameter();
        idParam.ParameterName = "@id";
        idParam.Value = id;

        command.CommandText = "SELECT id, name FROM customers WHERE id = @id";
        command.Parameters.Add(idParam);
        command.Connection?.Open();

        using var dataReader = command.ExecuteReader();
        if (!dataReader.Read())
        {
            return null;
        }

        var customerId = dataReader.GetInt64(0);
        var customerName = dataReader.GetString(1);
        return new Customer(customerId, customerName);

    }

    public void Update(Customer customer)
    {
        using var connection = _dbConnectionProvider.GetConnection();
        using var command = connection.CreateCommand();

        // Set up the parameters for the command to avoid SQL injection.
        var idParam = command.CreateParameter();
        idParam.ParameterName = "@id";
        idParam.Value = customer.Id;

        var nameParam = command.CreateParameter();
        nameParam.ParameterName = "@name";
        nameParam.Value = customer.Name;

        // Write the SQL command text to update the customer.
        command.CommandText = "UPDATE customers SET name = @name WHERE id = @id";
        command.Parameters.Add(idParam);
        command.Parameters.Add(nameParam);

        command.Connection?.Open();

        // Execute the command.
        int affectedRows = command.ExecuteNonQuery();

        // Optionally, you can check how many rows were affected.
        if (affectedRows == 0)
        {
            throw new Exception("No customer was updated. Customer not found.");
        }
    }

}