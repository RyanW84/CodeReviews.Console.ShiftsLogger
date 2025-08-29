#!/bin/bash

# Test script for ShiftsLogger project
echo "Running ShiftsLogger Unit Tests"
echo "==============================="

# Run Model Tests
echo "Running Model Tests..."
dotnet test --filter "FullyQualifiedName~Models" --verbosity quiet

# Run Service Tests
echo "Running Service Tests..."
dotnet test --filter "FullyQualifiedName~Services" --verbosity quiet

# Run Controller Tests
echo "Running Controller Tests..."
dotnet test --filter "FullyQualifiedName~Controllers" --verbosity quiet

# Run Integration Tests
echo "Running Integration Tests..."
dotnet test --filter "FullyQualifiedName~Integration" --verbosity quiet

echo "==============================="
echo "All tests completed!"
echo "Note: Integration tests are now enabled with in-memory database configuration"
