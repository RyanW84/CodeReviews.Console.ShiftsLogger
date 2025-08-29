using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ConsoleFrontEnd.Models.Dtos;

namespace ConsoleFrontEnd.Services.Validation;

public static class WorkerValidation
{
    public static List<string> Validate(WorkerApiRequestDto dto)
    {
        List<string> errors = [];
        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Worker name is required.");
        if (dto.Name.Length < 1)
            errors.Add("Worker name must be at least 1 character.");
        if (dto.Name.Length > 100)
            errors.Add("Worker name must be less than 100 characters.");

        // Email validation - more forgiving
        if (string.IsNullOrWhiteSpace(dto.Email))
            errors.Add("Email is required.");
        else
        {
            // Basic email validation - just check for @ and reasonable length
            if (!dto.Email.Contains("@") || dto.Email.Length < 5)
                errors.Add("Email must contain @ and be at least 5 characters.");
            // Check for basic format: something@something.something
            var parts = dto.Email.Split('@');
            if (parts.Length != 2 || parts[0].Length < 1 || parts[1].Length < 3 || !parts[1].Contains("."))
                errors.Add("Email must be in format: user@domain.extension");
        }

        // Phone validation - more forgiving
        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            errors.Add("Phone number is required.");
        else
        {
            // Remove spaces, hyphens, and other common formatting
            var phone = dto.PhoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace(".", "");
            if (phone.Length < 10)
                errors.Add("Phone number must be at least 10 digits.");
            if (phone.Length > 15)
                errors.Add("Phone number must be less than 15 digits.");
            // Allow various formats - just check if it contains mostly digits
            var digitCount = phone.Count(char.IsDigit);
            if (digitCount < 10)
                errors.Add("Phone number must contain at least 10 digits.");
        }

        // Add more business rules as needed
        return errors;
    }
}
