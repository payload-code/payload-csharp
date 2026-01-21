# Payload C# Library

A C# library for integrating [Payload](https://payload.com).

## Installation

### 1) Download

Download the [latest](https://github.com/payload-code/payload-csharp/archive/master.zip)
version from GitHub.

### 2) Include in Project

Include the **Payload** folder in your Visual Studio project.

### NuGet

```bash
dotnet add package payload-api
```

## Get Started

Once you've included the Payload C# library in your project,
include the `Payload` namespace to get started.

All Payload objects and methods are accessible using the `pl` static class.

### API Authentication

To authenticate with the Payload API, you'll need a live or test API key. API
keys are accessible from within the Payload dashboard.

```csharp
using Payload;

var pl = new Payload.Session("secret_key_3bW9JMZtPVDOfFNzwRdfE");
```

### API Versioning

The Payload API supports multiple versions. You can specify which version to use when creating a session:

```csharp
using Payload;

var pl = new Payload.Session(
    "secret_key_3bW9JMZtPVDOfFNzwRdfE",
    api_version: "v2"
);
```

### Creating an Object

Interfacing with the Payload API is done primarily through Payload Objects. Below is an example of
creating a customer using the `pl.Customer` object.

```csharp
// Create a Customer
var customer = await pl.Customer.CreateAsync(new
{
    email = "matt.perez@example.com",
    name = "Matt Perez"
});
```

```csharp
// Create a Payment
var payment = await pl.Payment.CreateAsync(new
{
    amount = 100.0,
    payment_method = new pl.Card(new
    {
        card_number = "4242 4242 4242 4242"
    })
});
```

### Accessing Object Attributes

Object attributes are accessible through both dot and bracket notation.

```csharp
// Dynamic
Console.WriteLine(customer.name);

// Compile-time checking
Console.WriteLine(customer["email"]); 
Console.WriteLine(customer.Data.email);
```

### Updating an Object

Updating an object is a simple call to the `UpdateAsync` object method.

```csharp
// Updating a customer's email
await customer.UpdateAsync(new { email = "matt.perez@newwork.com" });
```

### Selecting Objects

Objects can be selected using any of their attributes.

```csharp
// Select a customer by email
var customers = await pl.Customer
    .FilterBy(new { email = "matt.perez@example.com" })
    .OneAsync();
```

Use the `pl.Attr` attribute helper interface to write powerful
queries with a little extra syntax sugar.

```csharp
var payments = await pl.Payment
    .FilterBy(
        pl.Attr.amount.gt(100),
        pl.Attr.amount.lt(200),
        pl.Attr.description.contains("Test"),
        pl.Attr.created_at.gt(new DateTime(2019,2,1))
    )
    .AllAsync();
```

### Testing the Payload C# Library

Tests are contained within the PayloadTests/ directory. To run tests enter the command in terminal

```bash
API_KEY=your_test_secret_key dotnet test
```

## Documentation

To get further information on Payload's C# library and API capabilities,
visit the unabridged [Payload Documentation](https://docs.payload.com/?csharp).
