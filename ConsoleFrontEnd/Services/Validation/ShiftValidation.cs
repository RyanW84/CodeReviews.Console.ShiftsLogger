using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ConsoleFrontEnd.Models.Dtos;

namespace ConsoleFrontEnd.Services.Validation;

public static class ShiftValidation
{
    public static List<string> Validate(ShiftApiRequestDto dto)
    {
        List<string> errors = [];
        if (dto.WorkerId <= 0)
            errors.Add("WorkerId must be greater than zero.");
        if (dto.LocationId <= 0)
            errors.Add("LocationId must be greater than zero.");
        if (dto.StartTime >= dto.EndTime)
            errors.Add("Start time must be before end time.");
        if (dto.StartTime < DateTimeOffset.Now.AddYears(-5) || dto.StartTime > DateTimeOffset.Now.AddYears(5))
            errors.Add("Start time is out of allowed range (5 years past/future).");
        if (dto.EndTime < DateTimeOffset.Now.AddYears(-5) || dto.EndTime > DateTimeOffset.Now.AddYears(5))
            errors.Add("End time is out of allowed range (5 years past/future).");

        // Additional business rules:
        if ((dto.EndTime - dto.StartTime).TotalMinutes < 5)
            errors.Add("Shift duration must be at least 5 minutes.");
        // Allow shifts up to 24 hours (can span midnight)
        if ((dto.EndTime - dto.StartTime).TotalHours > 24)
            errors.Add("Shift duration cannot exceed 24 hours.");
        // Allow shifts to span multiple calendar days (e.g., overnight shifts)
        // More forgiving tolerance for past-start (30 minutes instead of 5)
        if (dto.StartTime < DateTimeOffset.Now.AddMinutes(-30))
            errors.Add("Shift cannot start more than 30 minutes in the past.");

        // Add more rules as needed for your business logic
        return errors;
    }
}
