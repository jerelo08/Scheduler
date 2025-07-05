# TAMHR.Hangfire Integration Test Runner
# This script helps run different types of tests

Write-Host "🧪 TAMHR.Hangfire Test Runner" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

# Change to the project directory
$projectPath = "d:\Kantor\Project\Hangfire\TAMHR-Hangfire - git"
Set-Location $projectPath

Write-Host "`n📋 Available Test Options:" -ForegroundColor Yellow
Write-Host "1. Run Unit Tests Only" -ForegroundColor White
Write-Host "2. Run Database Integration Tests" -ForegroundColor White
Write-Host "3. Run Hangfire Integration Tests" -ForegroundColor White
Write-Host "4. Run End-to-End Integration Tests" -ForegroundColor White
Write-Host "5. Run ALL Tests" -ForegroundColor White
Write-Host "6. Build and Check Hangfire Jobs" -ForegroundColor White

$choice = Read-Host "`nEnter your choice (1-6)"

switch ($choice) {
    "1" {
        Write-Host "`n🧪 Running Unit Tests..." -ForegroundColor Cyan
        dotnet test --filter "FullyQualifiedName!~Integration"
    }
    "2" {
        Write-Host "`n🔌 Running Database Integration Tests..." -ForegroundColor Cyan
        Write-Host "⚠️  Ensure database connections are available!" -ForegroundColor Yellow
        dotnet test --filter "FullyQualifiedName~DatabaseIntegrationTests"
    }
    "3" {
        Write-Host "`n⚙️  Running Hangfire Integration Tests..." -ForegroundColor Cyan
        dotnet test --filter "FullyQualifiedName~HangfireIntegrationTests"
    }
    "4" {
        Write-Host "`n🎯 Running End-to-End Integration Tests..." -ForegroundColor Cyan
        Write-Host "⚠️  This will attempt real API calls (may fail due to network/endpoints)!" -ForegroundColor Yellow
        dotnet test --filter "FullyQualifiedName~EndToEndIntegrationTests"
    }
    "5" {
        Write-Host "`n🚀 Running ALL Tests..." -ForegroundColor Cyan
        Write-Host "⚠️  Ensure database connections are available!" -ForegroundColor Yellow
        dotnet test --verbosity normal
    }
    "6" {
        Write-Host "`n🔧 Building project and checking Hangfire jobs..." -ForegroundColor Cyan
        
        # Build the main project
        Write-Host "Building TAMHR.Hangfire project..." -ForegroundColor White
        dotnet build TAMHR.Hangfire/TAMHR.Hangfire.csproj
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Build successful!" -ForegroundColor Green
            
            Write-Host "`n📊 Hangfire Job Registration Check:" -ForegroundColor Yellow
            Write-Host "1. Start the application: dotnet run --project TAMHR.Hangfire" -ForegroundColor White
            Write-Host "2. Open browser to: http://localhost:5000/hangfire" -ForegroundColor White
            Write-Host "3. Look for 'DataSyncJob' in the Recurring Jobs tab" -ForegroundColor White
            Write-Host "4. Check that job is scheduled with cron: '0 3 * * *'" -ForegroundColor White
            
            $runApp = Read-Host "`nDo you want to start the application now? (y/n)"
            if ($runApp -eq "y" -or $runApp -eq "Y") {
                Write-Host "🚀 Starting TAMHR.Hangfire application..." -ForegroundColor Green
                Write-Host "💡 Open http://localhost:5000/hangfire in your browser" -ForegroundColor Cyan
                dotnet run --project TAMHR.Hangfire/TAMHR.Hangfire.csproj
            }
        }
        else {
            Write-Host "❌ Build failed!" -ForegroundColor Red
        }
    }
    default {
        Write-Host "❌ Invalid choice. Please run the script again." -ForegroundColor Red
    }
}

Write-Host "`n✨ Test execution completed!" -ForegroundColor Green
