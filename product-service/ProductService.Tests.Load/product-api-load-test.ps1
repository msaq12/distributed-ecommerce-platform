param(
    [int]$Requests = 100,
    [int]$ConcurrentUsers = 10,
    [string]$ApiKey = ""
)

# Configuration
$baseUrl = "https://apim-furniture-dev.azure-api.net/products"
$headers = @{
    "Ocp-Apim-Subscription-Key" = $ApiKey
}

# Test endpoints
$endpoints = @(
    "/api/Products/prod-bed-001?categoryId=beds",
    "/api/Products"
)

Write-Host "Starting load test: $Requests requests with $ConcurrentUsers concurrent users"
Write-Host "Target: $baseUrl"
Write-Host ""

# Run load test
$results = 1..$Requests | ForEach-Object -Parallel {
    $url = $using:baseUrl
    $headers = $using:headers
    $endpoints = $using:endpoints

    # Random endpoint selection
    $endpoint = $endpoints | Get-Random
    $fullUrl = "$url$endpoint"

    $startTime = Get-Date

    try {
        $response = Invoke-WebRequest -Uri $fullUrl -Headers $headers -UseBasicParsing
        $duration = ((Get-Date) - $startTime).TotalMilliseconds
        $cacheHeader = if ($response.Headers['X-Cache']) { $response.Headers['X-Cache'] } else { "NONE" }

        [PSCustomObject]@{
            Endpoint = $endpoint
            StatusCode = $response.StatusCode
            Duration = [math]::Round($duration, 0)
            CacheHeader = $cacheHeader
        }
    }
    catch {
        $duration = ((Get-Date) - $startTime).TotalMilliseconds
        [PSCustomObject]@{
            Endpoint = $endpoint
            StatusCode = 500
            Duration = [math]::Round($duration, 0)
            CacheHeader = "ERROR"
        }
    }
} -ThrottleLimit $ConcurrentUsers

# Display results
Write-Host ""
$results | Format-Table -AutoSize

# Calculate statistics
$successful = ($results | Where-Object StatusCode -eq 200).Count
$failed = ($results | Where-Object StatusCode -ne 200).Count
$avgDuration = [math]::Round(($results | Measure-Object Duration -Average).Average, 2)
$minDuration = ($results | Measure-Object Duration -Minimum).Minimum
$maxDuration = ($results | Measure-Object Duration -Maximum).Maximum

Write-Host ""
Write-Host "=== LOAD TEST RESULTS ==="
Write-Host "Total Requests: $Requests"
Write-Host "Successful: $successful"
Write-Host "Failed: $failed"
Write-Host ""
Write-Host "Response Times:"
Write-Host "  Average: ${avgDuration}ms"
Write-Host "  Min: ${minDuration}ms"
Write-Host "  Max: ${maxDuration}ms"

if ($results.Count -gt 0) {
    $sortedResults = $results | Sort-Object Duration
    $p50Index = [math]::Floor($results.Count * 0.5)
    $p95Index = [math]::Floor($results.Count * 0.95)
    Write-Host "  P50: $($sortedResults[$p50Index].Duration)ms"
    Write-Host "  P95: $($sortedResults[$p95Index].Duration)ms"
}

# Cache statistics
$cacheHits = ($results | Where-Object CacheHeader -eq 'HIT').Count
$cacheMisses = ($results | Where-Object { $_.CacheHeader -eq 'MISS' -or $_.CacheHeader -eq 'NONE' }).Count

Write-Host ""
Write-Host "Cache Statistics:"
Write-Host "  Cache Hits: $cacheHits"
Write-Host "  Cache Misses: $cacheMisses"

if (($cacheHits + $cacheMisses) -gt 0) {
    $hitRate = [math]::Round((($cacheHits / ($cacheHits + $cacheMisses)) * 100), 2)
    Write-Host "  Hit Rate: ${hitRate}%"
}
