#!/bin/bash

# ShiftsLogger User Secrets Setup Script
# This script helps set up user secrets for database configuration

echo "ShiftsLogger - User Secrets Setup"
echo "=================================="
echo ""

# Check if we're in the right directory
if [ ! -f "ShiftsLoggerV2.RyanW84/ShiftsLoggerV2.RyanW84.csproj" ]; then
    echo "Error: Please run this script from the root of the ShiftsLogger project"
    exit 1
fi

echo "This script will help you set up user secrets for database configuration."
echo "User secrets are used to securely store sensitive information like database passwords."
echo ""

# Navigate to the API project directory
cd ShiftsLoggerV2.RyanW84

echo "Current directory: $(pwd)"
echo ""

# Check if user secrets are already initialized
if [ -f "obj/usersecrets.json" ] || [ -d ".usersecrets" ]; then
    echo "User secrets are already initialized for this project."
else
    echo "Initializing user secrets for the project..."
    dotnet user-secrets init
    echo "User secrets initialized successfully."
fi

echo ""
echo "To set up your Linux SQL Server connection string, run one of the following commands:"
echo ""
echo "Option 1 - Using environment variables in the connection string:"
echo "dotnet user-secrets set \"ConnectionStrings:LinuxSqlServer\" \"Server=127.0.0.1,1433;Database=ShiftsLoggerDb;User Id=sa;Password=YOUR_PASSWORD_HERE;TrustServerCertificate=yes;Encrypt=false;MultipleActiveResultSets=yes\""
echo ""
echo "Option 2 - Using a different server/username:"
echo "dotnet user-secrets set \"ConnectionStrings:LinuxSqlServer\" \"Server=YOUR_SERVER;Database=ShiftsLoggerDb;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=yes;Encrypt=false;MultipleActiveResultSets=yes\""
echo ""
echo "Replace YOUR_PASSWORD_HERE, YOUR_SERVER, and YOUR_USERNAME with your actual values."
echo ""
echo "To view your current user secrets:"
echo "dotnet user-secrets list"
echo ""
echo "To remove a secret:"
echo "dotnet user-secrets remove \"ConnectionStrings:LinuxSqlServer\""
echo ""
echo "For more information, see the Database Configuration section in README.md"
