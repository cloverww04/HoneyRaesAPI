using HoneyRaesAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


List<Customer> customers = new List<Customer> {
    new Customer()
    {
        Id = 1,
        Name = "Jeff",
        Address = "1234 Somewhere Ave. SomePlace, USA"
    },
    new Customer()
    {
        Id=2,
        Name = "Susan",
        Address = "3214 Somewhere Ave. Someplace, KY"
    },
    new Customer()
    {
        Id = 3,
        Name = "Bob",
        Address = "7584 Somewhere Ave. Otherplace, TN"
    }
};
List<Employee> employees = new List<Employee> {
            new Employee 
            { 
                Id = 1, 
                Name = "John Doe",
                Specialty = "Software Development"
            },
            new Employee 
            { 
                Id = 2,
                Name = "Jane Smith",
                Specialty = "Data Science"
            },
            new Employee 
            { 
                Id = 3,
                Name = "Michael Johnson",
                Specialty = "UX/UI Design"
            }
};


List<ServiceTicket> serviceTickets = new List<ServiceTicket> {
    new ServiceTicket
                {
                    Id = 1,
                    CustomerId = 1,
                    EmployeeId = 1,
                    Description = "Ticket 1 Description",
                    Emergency = false,
                    DateCompleted = DateTime.Now.AddDays(2)
                },
                new ServiceTicket
                {
                    Id = 2,
                    CustomerId = 2,
                    Description = "Ticket 2 Description",
                    Emergency = true,
                    // Leave EmployeeId unassigned (null).
                    // Leave DateCompleted unassigned (null).
                },
                new ServiceTicket
                {
                    Id = 3,
                    CustomerId = 1,
                    Description = "Ticket 3 Description",
                    Emergency = false,
                    DateCompleted = null
                },
                new ServiceTicket
                {
                    Id = 4,
                    CustomerId = 3,
                    EmployeeId = 2,
                    Description = "Ticket 4 Description",
                    Emergency = true,
                    // Leave DateCompleted unassigned (null).
                },
                new ServiceTicket
                {
                    Id = 5,
                    CustomerId = 2,
                    Description = "Ticket 5 Description",
                    Emergency = false,
                    EmployeeId = null,
                    DateCompleted = null
                }
};

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/customer", () =>
{
    return customers;
});

app.MapGet("/customer/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(cu => cu.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(customer);
});

app.MapGet("/employee", () =>
{
    return employees;
});

app.MapGet("/employee/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(employee);
});

app.MapGet("/servicetickets", () =>
{
    return serviceTickets;
});

app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    serviceTicket.Customer = customers.FirstOrDefault(cu => cu.Id == id);
    return Results.Ok(serviceTicket);
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

