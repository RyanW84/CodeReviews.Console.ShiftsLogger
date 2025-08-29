using Microsoft.EntityFrameworkCore;
using ShiftsLoggerV2.RyanW84.Models;

namespace ShiftsLoggerV2.RyanW84.Data;

public class ShiftsLoggerDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Shift> Shifts { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Worker> Workers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Shift>()
            .HasOne(s => s.Location)
            .WithMany(l => l.Shifts)
            .HasForeignKey(s => s.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder
            .Entity<Shift>()
            .HasOne(s => s.Worker)
            .WithMany(w => w.Shifts)
            .HasForeignKey(s => s.WorkerId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Worker>().HasIndex(w => w.Email).IsUnique();
        modelBuilder.Entity<Worker>().HasIndex(w => w.PhoneNumber).IsUnique();

        // Performance indexes for frequently queried fields
        modelBuilder.Entity<Shift>().HasIndex(s => s.StartTime);
        modelBuilder.Entity<Shift>().HasIndex(s => s.EndTime);
        modelBuilder.Entity<Shift>().HasIndex(s => s.WorkerId);
        modelBuilder.Entity<Shift>().HasIndex(s => s.LocationId);
        modelBuilder.Entity<Shift>().HasIndex(s => new { s.WorkerId, s.StartTime });
        modelBuilder.Entity<Shift>().HasIndex(s => new { s.LocationId, s.StartTime });

        modelBuilder.Entity<Worker>().HasIndex(w => w.Name);

        modelBuilder.Entity<Location>().HasIndex(l => l.Name);
        modelBuilder.Entity<Location>().HasIndex(l => l.Town);
        modelBuilder.Entity<Location>().HasIndex(l => l.PostCode);
    }

    public void SeedData(ILogger<ShiftsLoggerDbContext>? logger)
    {
        if (!Workers.Any())
        {
            SeedWorkers(logger);
        }

        if (!Locations.Any())
        {
            SeedLocations(logger);
        }

        SeedRandomShifts(logger);
    }

    private void SeedWorkers(ILogger<ShiftsLoggerDbContext>? logger)
    {
        var workerData = new[]
        {
            ("John Smith", "john.smith@company.com", "+44 7911 123456"),
            ("Sarah Johnson", "sarah.johnson@company.com", "+44 7700 900123"),
            ("Mike Davis", "mike.davis@company.com", "+44 7802 345678"),
            ("Emily Wilson", "emily.wilson@company.com", "+44 7920 765432"),
            ("David Brown", "david.brown@company.com", "+44 7555 123456"),
            ("Lisa Anderson", "lisa.anderson@company.com", "+44 7666 789012"),
            ("James Taylor", "james.taylor@company.com", "+44 7777 234567"),
            ("Anna Thompson", "anna.thompson@company.com", "+44 7888 345678")
        };

        foreach (var (name, email, phone) in workerData)
        {
            if (!Workers.Any(w => w.Email != null && w.Email.ToLower() == email.ToLower()))
            {
                Workers.Add(new Worker
                {
                    Name = name,
                    Email = email,
                    PhoneNumber = phone
                });
            }
        }
        SaveChanges();
    }

    // Seed random data
    private void SeedLocations(ILogger<ShiftsLoggerDbContext>? logger)
    {
        var locationData = new[]
        {
            ("London Office", "1 Canary Wharf", "London", "Greater London", "E14 5AB", "UK"),
            ("Manchester Warehouse", "22 Trafford Park", "Manchester", "Greater Manchester", "M17 1AB", "UK"),
            ("Birmingham Plant", "15 Aston Road", "Birmingham", "West Midlands", "B6 4DA", "UK"),
            ("Leeds Service Centre", "8 Wellington Place", "Leeds", "West Yorkshire", "LS1 4AP", "UK"),
            ("Bristol Research Lab", "3 Temple Quay", "Bristol", "Bristol", "BS1 6DZ", "UK"),
            ("Glasgow Branch", "45 George Square", "Glasgow", "Scotland", "G2 1DY", "UK"),
            ("Cardiff Hub", "12 Cardiff Bay", "Cardiff", "Wales", "CF10 4PA", "UK")
        };

        foreach (var (name, address, town, county, postCode, country) in locationData)
        {
            if (!Locations.Any(l => l.Name.ToLower() == name.ToLower()))
            {
                Locations.Add(new Location
                {
                    Name = name,
                    Address = address,
                    Town = town,
                    County = county,
                    PostCode = postCode,
                    Country = country
                });
            }
        }
        SaveChanges();
    }

    private void SeedRandomShifts(ILogger<ShiftsLoggerDbContext>? logger)
    {
        var savedWorkers = Workers.ToList();
        var savedLocations = Locations.ToList();

        if (savedWorkers.Count == 0 || savedLocations.Count == 0)
        {
            logger?.LogWarning("Cannot seed shifts: No workers or locations available.");
            return;
        }

        var currentShiftCount = Shifts.Count();

        var shiftsToGenerate = 200;
        var randomShifts = GenerateRandomShifts(savedWorkers, savedLocations, shiftsToGenerate);

        try
        {
            var addedCount = 0;
            foreach (var shift in randomShifts)
            {
                if (shift.EndTime <= shift.StartTime)
                    continue;

                var exists = Shifts.Any(s =>
                    s.WorkerId == shift.WorkerId
                    && s.LocationId == shift.LocationId
                    && s.StartTime == shift.StartTime
                );

                if (!exists)
                {
                    Shifts.Add(shift);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                SaveChanges();
                logger?.LogInformation("Added {AddedCount} random shifts. Total shifts in database: {ShiftCount}",
                    addedCount, Shifts.Count());
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An error occurred while seeding random shifts.");
        }
    }

    private List<Shift> GenerateRandomShifts(List<Worker> workers, List<Location> locations, int count)
    {
        var shifts = new List<Shift>();
        var random = new Random();
        var baseDate = DateTimeOffset.Now.Date; // Start from today at midnight
        
        var shiftDurations = new[] { 4, 6, 8, 10, 12 }; // More varied shift lengths
        var startHours = new[] { 6, 7, 8, 9, 14, 15, 16, 18, 22 }; // Various start times

        for (int i = 0; i < count; i++)
        {
            var worker = workers[random.Next(workers.Count)];
            var location = locations[random.Next(locations.Count)];
            
            // Generate shifts within a 30-day window (past and future)
            var daysOffset = random.Next(-15, 16);
            var startHour = startHours[random.Next(startHours.Length)];
            var startTime = baseDate.AddDays(daysOffset).AddHours(startHour);
            
            var durationHours = shiftDurations[random.Next(shiftDurations.Length)];
            var endTime = startTime.AddHours(durationHours);

            shifts.Add(new Shift
            {
                WorkerId = worker.WorkerId,
                LocationId = location.LocationId,
                StartTime = startTime,
                EndTime = endTime
            });
        }

        return shifts;
    }

    public void ResetShiftsForTesting(ILogger<ShiftsLoggerDbContext>? logger)
    {
        try
        {
            var shiftCount = Shifts.Count();
            if (shiftCount > 0)
            {
                Shifts.RemoveRange(Shifts);
                SaveChanges();
                logger?.LogInformation("Cleared {ShiftCount} shifts from database for testing purposes.", shiftCount);
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to reset shifts for testing.");
        }
    }
}