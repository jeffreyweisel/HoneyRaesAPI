using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using HoneyRaesAPI.Models;
using HoneyRaesAPI.Models.DTOs;
using Microsoft.VisualBasic;

// List of customers
List<Customer> customers = new List<Customer>
{
    new Customer { Id = 1, Name = "Dwight Schrute", Address = "123 Main Street" },
    new Customer { Id = 2, Name = "Jim Halpert", Address = "321 Other Street" },
    new Customer { Id = 3, Name = "Kelly Kapoor", Address = "456 Grape Street" }
};

// List of employees
List<Employee> employees = new List<Employee>
{
    new Employee {Id = 1, Name = "Stanley Hudson", Specialty = "Cell Phone Repair"},
    new Employee {Id = 2, Name = "Michael Scott", Specialty = "Customer Service"}
};

// List of service tickets
List<ServiceTicket> serviceTickets = new List<ServiceTicket>
{
    new ServiceTicket {Id = 1, CustomerId = 1, EmployeeId = 2, Description = "Phone dosent turn on", Emergency = false, DateCompleted = new DateTime(2023, 11 , 23)},
    new ServiceTicket {Id = 2, CustomerId = 1, EmployeeId = 1, Description = "Phone screen is cracked", Emergency = false},
    new ServiceTicket {Id = 3, CustomerId = 2, Description = "Laptop won't open", Emergency = true, DateCompleted = new DateTime(2023, 11 , 20)},
    new ServiceTicket {Id = 4, CustomerId = 2, EmployeeId = 2, Description = "Monitor is stuck on a black screen", Emergency = false},
    new ServiceTicket {Id = 5, CustomerId = 3, Description = "Xbox has red ring of death", Emergency = true, DateCompleted = new DateTime(2023, 11 , 15)},

};

var builder = WebApplication.CreateBuilder(args);

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Define app as a variable
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// List of employees
app.MapGet("/employees", () =>
{
    return Results.Ok(employees);
});

// List of customers 
app.MapGet("/customers", () => 
{
    return Results.Ok(customers);
});

// List of service tickets
app.MapGet("/servicetickets", () =>
{
    return Results.Ok(serviceTickets);
});

// Get employees based off their Id
app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    
    if (employee == null)
    {
        return Results.NotFound();
    }
    // Find the service tickets that are assigned to a given employee
    List<ServiceTicket> tickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();

    return Results.Ok(new EmployeeDTO
    {
        Id = employee.Id,
        Name = employee.Name,
        Specialty = employee.Specialty,
        ServiceTickets = tickets.Select(t => new ServiceTicketDTO
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            EmployeeId = t.EmployeeId,
            Description = t.Description,
            Emergency = t.Emergency,
            DateCompleted = t.DateCompleted
        }).ToList()
    });
});

// Get the customers based off of their Id
app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }

    // Find the service tickets each customer created based off their Id
    List<ServiceTicket> tickets = serviceTickets.Where(st => st.CustomerId == id).ToList();

    return Results.Ok(new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
        ServiceTickets = tickets.Select(t => new ServiceTicketDTO
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            EmployeeId = t.EmployeeId,
            Description = t.Description,
            Emergency = t.Emergency,
            DateCompleted = t.DateCompleted
        }).ToList()
    });
});

// Get service tickets based off their Id
app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (serviceTicket == null)
    {
        return Results.NotFound();
    }

    // Find the customer and employee attatched to each service ticket based off FK
    Employee employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    Customer customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);

    return Results.Ok(new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        Customer = customer == null ? null : new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        EmployeeId = serviceTicket.EmployeeId,
        Employee = employee == null ? null : new EmployeeDTO
        {
            Id = employee.Id,
            Name = employee.Name,
            Specialty = employee.Specialty
        },
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency,
        DateCompleted = serviceTicket.DateCompleted
    });
});

// Posting a new service ticket to the list
app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (SQL will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    // serviceTickets.Add(serviceTicket);

    // Get the customer data to check that the customerid for the service ticket is valid
    Customer customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);

    // if the client did not provide a valid customer id, this is a bad request
    if (customer == null)
    {
        return Results.BadRequest();
    }

    serviceTickets.Add(serviceTicket);

    // Created returns a 201 status code with a link in the headers to where the new resource can be accessed
    return Results.Created($"/servicetickets/{serviceTicket.Id}", new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        Customer = new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency
    });

});

// Delete a service ticket based off its Id
app.MapDelete("/servicetickets/{id}", (int id) =>
{
    // Find the service ticket by ID
    ServiceTicket serviceTicketToDelete = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (serviceTicketToDelete == null)
    {
        return Results.NotFound();
    }

    // Remove the service ticket from list
    serviceTickets.Remove(serviceTicketToDelete);

    return Results.NoContent();
});

// Assign an employee to a ticket via PUT request
app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }

    ticketToUpdate.CustomerId = serviceTicket.CustomerId;
    ticketToUpdate.EmployeeId = serviceTicket.EmployeeId;
    ticketToUpdate.Description = serviceTicket.Description;
    ticketToUpdate.Emergency = serviceTicket.Emergency;
    ticketToUpdate.DateCompleted = serviceTicket.DateCompleted;

    return Results.NoContent();
});

// Mark service ticket as completed by posting a DateCompleted to the ticket based off of Id 
app.MapPost("/servicetickets/{id}/complete", (int id) =>
{

    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (ticketToComplete == null)
    {
        return Results.NotFound();
    }

    ticketToComplete.DateCompleted = DateTime.Today;

    return Results.NoContent();

});

// Runs app needs to stay at bottom of page
app.Run();


