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
                    DateCompleted = DateTime.Now.AddDays(-2)
                },
                new ServiceTicket
                {
                    Id = 2,
                    CustomerId = 2,
                    Description = "Ticket 2 Description",
                    Emergency = true,
                    // Leave EmployeeId unassigned (null).
                    DateCompleted = DateTime.Now.AddYears(-2)
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
                    EmployeeId = 3,
                    DateCompleted = DateTime.Now.AddYears(-2)
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
    employee.ServiceTickets = serviceTickets.Select(x => new ServiceTicket { CustomerId = x.CustomerId, EmployeeId = x.EmployeeId, DateCompleted = x.DateCompleted, Description = x.Description, Emergency = x.Emergency, Id = x.Id })
   .Where(st => st.EmployeeId == id).ToList();
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

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

app.MapDelete("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicketToDelete = serviceTickets.FirstOrDefault(st => st.Id == id);
    if(serviceTicketToDelete == null)
    {
        return Results.NotFound();
    }

    serviceTickets.Remove(serviceTicketToDelete);

    return Results.NoContent();
});

app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) => 
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    if ( ticketToUpdate == null )
    {
        return Results.NotFound();
    }

    if (id != serviceTicket.Id) 
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});

app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Now;
});

app.MapGet("/emergencies", () => 
{
    List<ServiceTicket> emergencyTickets = serviceTickets.Where(st => st.Emergency && !st.DateCompleted.HasValue).ToList();

    return Results.Ok(emergencyTickets);
});

app.MapGet("/unassigned", () => 
{
    List<ServiceTicket> unassignedTickets = serviceTickets.Where(st => st.EmployeeId == null).ToList();
    return Results.Ok(unassignedTickets);
});

app.MapGet("/oldcustomers", () =>
{
    DateTime oneYearAgo = DateTime.Now.AddYears(-1);

    List<Customer> oldCustomers = customers.Where(cust =>
    {
        List<ServiceTicket> customerTickets = serviceTickets
            .Where(st => st.CustomerId == cust.Id && st.DateCompleted.HasValue && st.DateCompleted.Value >= oneYearAgo)
            .ToList();

        return customerTickets.Count == 0;
    }).ToList();

    return Results.Ok(oldCustomers);
});

app.MapGet("/unassignedemployees", () =>
{
    List<Employee> unassignedEmployees = employees.Where(emp =>
    {
        return !serviceTickets.Any(st => st.EmployeeId == emp.Id && !st.DateCompleted.HasValue);
    }).ToList();

    return Results.Ok(unassignedEmployees);
});

app.MapGet("/employee/{id}/customers", (int employeeId) =>
{
    var employee = employees.FirstOrDefault(e => e.Id == employeeId);
    if (employee == null)
    {
        return Results.NotFound();
    }

    List<Customer> employeeCustomers = customers.Where(cust => serviceTickets.Any(st => st.EmployeeId == employeeId && st.CustomerId == cust.Id)).ToList();

    return Results.Ok(employeeCustomers);
});

app.MapGet("/employeeofthemonth", () =>
{
    DateTime lastMonth = DateTime.Now.AddMonths(-1);

    var employeeOfTheMonth = employees.OrderByDescending((Employee e) =>
    {
        return serviceTickets.Count((ServiceTicket st) =>
            st.EmployeeId == e.Id && st.DateCompleted >= lastMonth);
    }).FirstOrDefault();

    return Results.Ok(employeeOfTheMonth);
});

app.MapGet("/completedtickets", () =>
{
    List<ServiceTicket> completedTickets = serviceTickets.Where(st => st.DateCompleted.HasValue).OrderBy(st => st.DateCompleted).ToList();

    return Results.Ok(completedTickets);
});

app.MapGet("/incompletetickets", () => 
{
    List<ServiceTicket> incompleteTicket = serviceTickets.Where(st => !st.DateCompleted.HasValue).OrderByDescending(st => st.Emergency).ThenBy(st => st.EmployeeId == null).ToList();

    return Results.Ok(incompleteTicket);
});












app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

