using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;

namespace ShiftsLoggerV2.RyanW84.Tests.Utilities;

public static class TestDataHelper
{
    public static Worker CreateValidWorker(int id = 1, string name = "Test Worker")
    {
        return new Worker
        {
            WorkerId = id,
            Name = name,
            Email = $"test{id}@example.com",
            PhoneNumber = "123-456-7890",
            ShiftCount = 0
        };
    }

    public static List<Worker> CreateWorkerList(int count = 3)
    {
        var workers = new List<Worker>();
        for (int i = 1; i <= count; i++)
        {
            workers.Add(CreateValidWorker(i, $"Worker {i}"));
        }
        return workers;
    }

    public static WorkerApiRequestDto CreateValidWorkerDto(string name = "Test Worker")
    {
        return new WorkerApiRequestDto
        {
            Name = name,
            Email = "test@example.com",
            PhoneNumber = "123-456-7890"
        };
    }

    public static Shift CreateValidShift(int id = 1, int workerId = 1, int locationId = 1)
    {
        var now = DateTimeOffset.Now;
        return new Shift
        {
            ShiftId = id,
            WorkerId = workerId,
            LocationId = locationId,
            StartTime = now,
            EndTime = now.AddHours(8)
        };
    }

    public static Location CreateValidLocation(int id = 1, string name = "Test Location")
    {
        return new Location
        {
            LocationId = id,
            Name = name,
            Address = "123 Test Street",
            Town = "Test Town",
            County = "Test County",
            PostCode = "12345",
            Country = "Test Country"
        };
    }

    public static List<Location> CreateLocationList(int count = 3)
    {
        var locations = new List<Location>();
        for (int i = 1; i <= count; i++)
        {
            locations.Add(CreateValidLocation(i, $"Location {i}"));
        }
        return locations;
    }

    public static List<Shift> CreateShiftList(int count = 3, int workerId = 1, int locationId = 1)
    {
        var shifts = new List<Shift>();
        var baseDate = DateTimeOffset.Now.AddDays(-count);
        
        for (int i = 1; i <= count; i++)
        {
            var startTime = baseDate.AddDays(i);
            shifts.Add(new Shift
            {
                ShiftId = i,
                WorkerId = workerId,
                LocationId = locationId,
                StartTime = startTime,
                EndTime = startTime.AddHours(8)
            });
        }
        return shifts;
    }

    public static WorkerFilterOptions CreateWorkerFilterOptions()
    {
        return new WorkerFilterOptions();
    }

    public static ShiftApiRequestDto CreateValidShiftDto(int workerId = 1, int locationId = 1)
    {
        var now = DateTimeOffset.Now;
        return new ShiftApiRequestDto
        {
            WorkerId = workerId,
            LocationId = locationId,
            StartTime = now,
            EndTime = now.AddHours(8)
        };
    }

    public static LocationApiRequestDto CreateValidLocationDto(string name = "Test Location")
    {
        return new LocationApiRequestDto
        {
            Name = name,
            Address = "123 Test Street",
            Town = "Test Town",
            County = "Test County",
            PostCode = "12345",
            Country = "Test Country"
        };
    }
}
