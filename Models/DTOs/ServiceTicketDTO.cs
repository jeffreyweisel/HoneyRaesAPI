using HoneyRaesAPI.Models.DTOs;

public class ServiceTicketDTO
{
    public int Id { get; set; }
    public int? CustomerId { get; set;}
    public int? EmployeeId { get; set;}
    public string Description { get; set; }
    public bool Emergency { get; set;}
    public DateTime DateCompleted { get; set; }
    public EmployeeDTO Employee { get; set; }
    public CustomerDTO Customer { get; set;}
}