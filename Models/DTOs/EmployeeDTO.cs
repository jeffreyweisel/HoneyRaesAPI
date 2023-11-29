namespace HoneyRaesAPI.Models.DTOs;

public class EmployeeDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Specialty { get; set; }
    public List<ServiceTicketDTO> ServiceTickets { get; set; }
}