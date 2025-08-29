using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ConsoleFrontEnd.Models.Dtos;

namespace ConsoleFrontEnd.Services.Validation;

public static class LocationValidation
{
    public static List<string> Validate(LocationApiRequestDto dto)
    {
        List<string> errors = [];
        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Location name is required.");
        if (dto.Name.Length < 1)
            errors.Add("Location name must be at least 1 character.");
        if (dto.Name.Length > 100)
            errors.Add("Location name must be less than 100 characters.");
        if (string.IsNullOrWhiteSpace(dto.Address))
            errors.Add("Address is required.");
        if (dto.Address.Length < 3)
            errors.Add("Address must be at least 3 characters.");
        if (string.IsNullOrWhiteSpace(dto.Town))
            errors.Add("Town is required.");
        if (string.IsNullOrWhiteSpace(dto.County))
            errors.Add("County is required.");
        if (string.IsNullOrWhiteSpace(dto.PostCode))
            errors.Add("PostCode is required.");
        if (dto.PostCode.Length < 3)
            errors.Add("PostCode must be at least 3 characters.");
        if (string.IsNullOrWhiteSpace(dto.Country))
            errors.Add("Country is required.");
        if (dto.Country.Length < 2)
            errors.Add("Country must be at least 2 characters.");
        // Add more business rules as needed
        return errors;
    }
}
