using System;
using System.Collections.Generic;

namespace SGSForms.Api.Entities;

public class Station
{
	public int Id { get; set; }

	public string Code { get; set; } = "";

	public string NameAr { get; set; } = "";

	public string NameEn { get; set; } = "";

	public bool IsActive { get; set; } = true;

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public ICollection<User> Users { get; set; } = new List<User>();

	public ICollection<ArrivalReport> ArrivalReports { get; set; } = new List<ArrivalReport>();

	public ICollection<DepartureReport> DepartureReports { get; set; } = new List<DepartureReport>();
}
