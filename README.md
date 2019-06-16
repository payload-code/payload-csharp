# Payload C# Library

A C# library for integrating [Payload](https://payload.co).

## Installation

### 1) Download

Download the [latest](https://github.com/payload-code/payload-csharp/archive/master.zip)
version from GitHub.

### 2) Include in Project

Include the **Payload** folder in your Visual Studio project.

### NuGet

nuget install payload-api

## Get Started

Once you've included the Payload C# library in your project,
include the `Payload` namespace to get started.

All Payload objects and methods are accessible using the `pl` static class.

### API Authentication

To authenticate with the Payload API, you'll need a live or test API key. API
keys are accessible from within the Payload dashboard.

```csharp
using Payload;
pl.api_key = "secret_key_3bW9JMZtPVDOfFNzwRdfE";
```

### Creating an Object

Interfacing with the Payload API is done primarily through Payload Objects. Below is an example of
creating a customer using the `pl.Customer` object.

```csharp
// Create a Customer
var customer = pl.Customer.create(new {
    email="matt.perez@example.com",
    full_name="Matt Perez"
});
```

```csharp
// Create a Payment
var payment = pl.Payment.create(new {
    amount=100.0,
    payment_method=new pl.Card(new{
        card_number="4242 4242 4242 4242"
    })
});
```

### Accessing Object Attributes

Object attributes are accessible through both dot and bracket notation.

```csharp
Console.WriteLine(customer.name);
Console.WriteLine(customer["email"]);
```

### Updating an Object

Updating an object is a simple call to the `update` object method.

```csharp
// Updating a customer's email
customer.update(new { email="matt.perez@newwork.com" });
```

### Selecting Objects

Objects can be selected using any of their attributes.

```csharp
// Select a customer by email
var customers = pl.Customer.filter_by(new {
    email="matt.perez@example.com"
});
```

Use the `pl.attr` attribute helper
interface to write powerful queries with a little extra syntax sugar.

```csharp
var payments = pl.Payments.filter_by(
    pl.attr.amount.gt(100),
    pl.attr.amount.lt(200),
    pl.attr.description.contains("Test"),
    pl.attr.created_at.gt(new DateTime(2019,2,1))
).all();
```

## Documentation

To get further information on Payload's C# library and API capabilities,
visit the unabridged [Payload Documentation](https://docs.payload.co/?csharp).
