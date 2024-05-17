using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

public class Location
{
  public int LocationId { get; set; }
  public string Name { get; set; }
}

public class Event : ICloneable
{
  public int EventId { get; set; }
  public DateTime Date { get; set; }
  public string Description { get; set; }
  public Location Location { get; set; } // Properti referensi

  // Shallow copy
  public object Clone()
  {
    return this.MemberwiseClone();
  }
}

public class AppDbContext : DbContext
{
  public DbSet<Event> Events { get; set; }
  public DbSet<Location> Locations { get; set; } // Tambahkan DbSet untuk lokasi

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    optionsBuilder.UseMySql("server=localhost; database=calendar; user=devin; password=devin", new MySqlServerVersion(new Version(8, 0, 30)));
  }
}

public class Program
{
  static void Main(string[] args)
  {
    using (var context = new AppDbContext())
    {
      context.Database.Migrate();
    }

    bool isRunning = true;
    while (isRunning)
    {
      Console.Clear();
      Console.WriteLine("Calendar App");
      Console.WriteLine("1. View Calendar");
      Console.WriteLine("2. Add Event");
      Console.WriteLine("3. Edit Event");
      Console.WriteLine("4. Delete Event");
      Console.WriteLine("5. Duplicate Event");
      Console.WriteLine("6. Exit");
      Console.Write("Choose an option: ");
      string option = Console.ReadLine();

      switch (option)
      {
        case "1":
          ViewCalendar();
          break;
        case "2":
          AddEvent();
          break;
        case "3":
          EditEvent();
          break;
        case "4":
          DeleteEvent();
          break;
        case "5":
          DuplicateEvent();
          break;
        case "6":
          isRunning = false;
          break;
        default:
          Console.WriteLine("Invalid option, try again.");
          break;
      }
    }
  }

  static void ViewCalendar()
  {
    using (var context = new AppDbContext())
    {
      var events = context.Events.Include(e => e.Location).OrderBy(e => e.Date); // Include Location
      Console.WriteLine("\nEvents in Calendar:");
      foreach (var ev in events)
      {
        Console.WriteLine($"{ev.EventId}: {ev.Date.ToShortDateString()} - {ev.Description} at {ev.Location?.Name}");
      }
    }
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
  }

  static void AddEvent()
  {
    Console.Write("Enter the date (dd/MM/yyyy): ");
    if (DateTime.TryParse(Console.ReadLine(), out var date))
    {
      Console.Write("Enter the event description: ");
      var description = Console.ReadLine();
      Console.Write("Enter the location name: ");
      var locationName = Console.ReadLine();
      var location = new Location { Name = locationName };
      var ev = new Event { Date = date, Description = description, Location = location };
      using (var context = new AppDbContext())
      {
        context.Locations.Add(location);
        context.Events.Add(ev);
        context.SaveChanges();
      }
      Console.WriteLine("Event added successfully!");
    }
    else
    {
      Console.WriteLine("Invalid date format.");
    }
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
  }

  static void EditEvent()
  {
    Console.Write("Enter Event ID to edit: ");
    if (int.TryParse(Console.ReadLine(), out var eventId))
    {
      using (var context = new AppDbContext())
      {
        var ev = context.Events.Include(e => e.Location).FirstOrDefault(e => e.EventId == eventId);
        if (ev != null)
        {
          Console.Write("Enter new date (dd/MM/yyyy): ");
          if (DateTime.TryParse(Console.ReadLine(), out var newDate))
          {
            ev.Date = newDate;
          }
          Console.Write("Enter new description: ");
          ev.Description = Console.ReadLine();
          Console.Write("Enter new location name: ");
          ev.Location.Name = Console.ReadLine();
          context.SaveChanges();
          Console.WriteLine("Event updated successfully!");
        }
        else
        {
          Console.WriteLine("Event not found.");
        }
      }
    }
    else
    {
      Console.WriteLine("Invalid Event ID.");
    }
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
  }

  static void DeleteEvent()
  {
    Console.Write("Enter Event ID to delete: ");
    if (int.TryParse(Console.ReadLine(), out var eventId))
    {
      using (var context = new AppDbContext())
      {
        var ev = context.Events.FirstOrDefault(e => e.EventId == eventId);
        if (ev != null)
        {
          context.Events.Remove(ev);
          context.SaveChanges();
          Console.WriteLine("Event deleted successfully!");
        }
        else
        {
          Console.WriteLine("Event not found.");
        }
      }
    }
    else
    {
      Console.WriteLine("Invalid Event ID.");
    }
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
  }

  static void DuplicateEvent()
  {
    Console.Write("Enter Event ID to duplicate: ");
    if (int.TryParse(Console.ReadLine(), out var eventId))
    {
      using (var context = new AppDbContext())
      {
        var ev = context.Events.Include(e => e.Location).FirstOrDefault(e => e.EventId == eventId);
        if (ev != null)
        {
          var clonedEvent = (Event)ev.Clone();
          clonedEvent.EventId = 0; // Ensure the new event gets a new ID
          context.Events.Add(clonedEvent);
          context.SaveChanges();
          Console.WriteLine("Event duplicated successfully!");
        }
        else
        {
          Console.WriteLine("Event not found.");
        }
      }
    }
    else
    {
      Console.WriteLine("Invalid Event ID.");
    }
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
  }
}
