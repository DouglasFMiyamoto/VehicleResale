using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using CustomerService.Core.Domain.Entities;
using CustomerService.Core.Domain.ValueObjects;
using CustomerService.Core.Ports.Out;
using Microsoft.Extensions.Configuration;

namespace CustomerService.Adapters.Persistence.DynamoDb;

public class CustomerRepositoryDynamoDb : ICustomerRepository
{
    private readonly ITable _t;
    private readonly ICustomerDataProtector _dataProtector;

    static string OnlyDigits(string s) => new string((s ?? "").Where(char.IsDigit).ToArray());

    public CustomerRepositoryDynamoDb(IAmazonDynamoDB ddb, IConfiguration cfg, ICustomerDataProtector dataProtector)
    {
        _dataProtector = dataProtector;
        string tableName = cfg["CUSTOMERS_TABLE"] ?? "Customers";

        _t = new TableBuilder(ddb, tableName)
            .AddHashKey("PK", DynamoDBEntryType.String)
            .Build();
    }

    static string Pk(string id) => $"CUST#{id}";

    public async Task CreateAsync(Customer customer, CancellationToken ct)
    {
        var d = new Document
        {
            ["PK"] = Pk(customer.CustomerId),
            ["CustomerId"] = customer.CustomerId,
            ["FullName"] = customer.FullName,
            ["DocumentCpf"] = _dataProtector.ProtectSensitive(customer.Document.Value),
            ["Email"] = _dataProtector.ProtectSensitive(customer.Email.Value),
            ["Phone"] = _dataProtector.ProtectSensitive(customer.Phone),
            ["AddressLine1"] = _dataProtector.ProtectSensitive(customer.AddressLine1),
            ["City"] = _dataProtector.ProtectSensitive(customer.City),
            ["State"] = _dataProtector.ProtectSensitive(customer.State),
            ["PostalCode"] = _dataProtector.ProtectSensitive(customer.PostalCode),
            ["CreatedAtUtc"] = customer.CreatedAtUtc.ToString("O")
        };

        await _t.PutItemAsync(d, new PutItemOperationConfig
        {
            ConditionalExpression = new Expression { ExpressionStatement = "attribute_not_exists(PK)" }
        }, ct);
    }

    public async Task<Customer?> GetByIdAsync(string customerId, CancellationToken ct)
    {
        var doc = await _t.GetItemAsync(Pk(customerId), ct);
        if (doc is null) return null;

        var fullName = doc.TryGetValue("FullName", out var n) ? n.AsString() : "";

        var cpfRaw = doc.TryGetValue("DocumentCpf", out var c) ? ReadProtectedString(doc, "DocumentCpf") : "";
        var cpfDigits = OnlyDigits(cpfRaw);

        var email = doc.TryGetValue("Email", out var e) ? ReadProtectedString(doc, "Email") : "";
        var phone = doc.TryGetValue("Phone", out var p) ? ReadProtectedString(doc, "Phone") : "";
        var addr = doc.TryGetValue("AddressLine1", out var a) ? ReadProtectedString(doc, "AddressLine1") : "";
        var city = doc.TryGetValue("City", out var ci) ? ReadProtectedString(doc, "City") : "";
        var state = doc.TryGetValue("State", out var st) ? ReadProtectedString(doc, "State") : "";
        var zip = doc.TryGetValue("PostalCode", out var z) ? ReadProtectedString(doc, "PostalCode") : "";

       var createdAt = DateTime.UtcNow;
        if (doc.TryGetValue("CreatedAtUtc", out var cat))
            DateTime.TryParse(cat.AsString(), out createdAt);

        if (cpfDigits.Length != 11)
             return null;

        return CustomerFactory.Rehydrate(
                 customerId,
                 fullName,
                 cpfDigits,
                 email,
                 phone,
                 addr,
                 city,
                 state,
                 zip,
                 createdAt
         );
    }

    public async Task<bool> ExistsAsync(string customerId, CancellationToken ct)
    {
        var doc = await _t.GetItemAsync(Pk(customerId), ct);
        return doc is not null;
    }

    private string ReadProtectedString(Document d, string key)
    {
        if (!d.TryGetValue(key, out var v) || v is null)
            return "";

        var s = v.AsString();
        return _dataProtector.UnprotectSensitive(s);
    }

}
