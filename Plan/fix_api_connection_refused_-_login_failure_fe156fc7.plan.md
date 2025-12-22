---
name: Fix API Connection Refused - Login Failure
overview: Diagnose and resolve the ERR_CONNECTION_REFUSED error preventing the frontend from connecting to the backend API. The API server appears to not be running or accessible on http://localhost:5073.
todos: []
---

# Fix API Connection Refused - Login Failure

## Problem

The frontend is unable to connect to the backend API at `http://localhost:5073/api/v1/Auth/login`, resulting in `ERR_CONNECTION_REFUSED`. This indicates the backend API server is either not running, crashed, or is not listening on the expected port.

## Root Cause Analysis

The API was previously started with `dotnet run` in the background, but it may have:

- Failed to start due to compilation errors
- Crashed during startup
- Encountered a port conflict
- Failed due to missing database/configuration

## Solution Steps

### 1. Check API Process Status

- Verify if any .NET processes are running
- Check if port 5073 is in use by another process
- Identify any crashed API processes

### 2. Check API Startup Logs

- Review any error output from the previous API run
- Look for compilation errors, missing dependencies, or configuration issues
- Check for database connection failures

### 3. Restart API with Visible Output

- Stop any existing API processes
- Start the API in a way that captures startup output
- Monitor for startup errors or exceptions

### 4. Verify API Health

- Confirm the API is listening on `http://localhost:5073`
- Test the health endpoint (`/health`)
- Verify Swagger UI is accessible at `/swagger`

### 5. Test Login Endpoint

- Verify the login endpoint is accessible
- Check if authentication is properly configured
- Ensure database seeding completed successfully

## Implementation Details

### Check Running Processes

```powershell
# Check for .NET processes
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
# Check port usage
netstat -ano | findstr :5073
```

### Restart API

```powershell
cd OrderManagement.Api
dotnet run
```

### Verify Configuration

- Check `appsettings.json` and `appsettings.Development.json`
- Verify database connection string
- Ensure CORS is configured for `http://localhost:4200`

## Expected Outcome

- API server running and accessible on `http://localhost:5073`
- Health endpoint returns 200 OK
- Login endpoint accepts requests
- Frontend can successfully authenticate users