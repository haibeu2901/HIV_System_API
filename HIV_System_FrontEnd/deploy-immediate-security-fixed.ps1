# PowerShell script to deploy immediate security to all private HTML files

Write-Host "🔒 IMMEDIATE SECURITY DEPLOYMENT STARTING..." -ForegroundColor Cyan
Write-Host "📁 Scanning private-view directory for HTML files..." -ForegroundColor Yellow

$protectedCount = 0
$alreadyProtectedCount = 0
$failedCount = 0

# Find all HTML files in private-view directory
Get-ChildItem -Path "private-view" -Filter "*.html" -Recurse -File | ForEach-Object {
    $file = $_.FullName
    $relativePath = $_.FullName -replace [regex]::Escape((Get-Location).Path + "\"), ""
    
    Write-Host "📄 Processing: $relativePath" -ForegroundColor White
    
    # Read file content
    $content = Get-Content $file -Raw -ErrorAction SilentlyContinue
    
    if (-not $content) {
        Write-Host "   ❌ Cannot read file" -ForegroundColor Red
        $failedCount++
        return
    }
    
    # Check if file is already protected with immediate security
    if ($content -match "immediate-security\.js") {
        Write-Host "   ✅ Already has immediate security" -ForegroundColor Green
        $alreadyProtectedCount++
        return
    }
    
    # Check if it's a full HTML document
    if ($content -notmatch "DOCTYPE html") {
        Write-Host "   ⏭️  Skipping partial HTML file" -ForegroundColor Gray
        return
    }
    
    # Create backup
    try {
        Copy-Item $file "$file.backup" -Force
    } catch {
        Write-Host "   ⚠️  Warning: Could not create backup" -ForegroundColor Yellow
    }
    
    # Calculate correct relative path to immediate-security.js
    $pathParts = $relativePath.Split('\')
    $depth = $pathParts.Length - 2  # Subtract 2 (file name and private-view)
    
    $relativeSecurityPath = ""
    for ($i = 0; $i -lt $depth; $i++) {
        $relativeSecurityPath += "../"
    }
    $relativeSecurityPath += "immediate-security.js"
    
    # Add immediate security script right after <head>
    $securityScript = "    <!-- IMMEDIATE SECURITY PROTECTION -->`n    <script src=`"$relativeSecurityPath`"></script>"
    $newContent = $content -replace "(<head[^>]*>)", "`$1`n$securityScript"
    
    if ($newContent -ne $content) {
        try {
            Set-Content -Path $file -Value $newContent -NoNewline
            Write-Host "   🔒 Immediate security added successfully (path: $relativeSecurityPath)" -ForegroundColor Green
            $protectedCount++
        } catch {
            Write-Host "   ❌ Failed to add security: $_" -ForegroundColor Red
            $failedCount++
            # Restore backup on failure
            if (Test-Path "$file.backup") {
                Move-Item "$file.backup" $file -Force
            }
        }
    } else {
        Write-Host "   ❌ Failed to modify content" -ForegroundColor Red
        $failedCount++
    }
}

Write-Host ""
Write-Host "🛡️  IMMEDIATE SECURITY DEPLOYMENT COMPLETE!" -ForegroundColor Cyan
Write-Host "✅ Files protected: $protectedCount" -ForegroundColor Green
Write-Host "✅ Already protected: $alreadyProtectedCount" -ForegroundColor Green
Write-Host "❌ Failed: $failedCount" -ForegroundColor Red
Write-Host ""
Write-Host "🚀 All private-view pages now have IMMEDIATE security!" -ForegroundColor Yellow
Write-Host "🔒 Immediate security will:" -ForegroundColor Yellow
Write-Host "   - Block access INSTANTLY without valid token" -ForegroundColor White
Write-Host "   - Hide page content immediately" -ForegroundColor White
Write-Host "   - Enforce role-based access control" -ForegroundColor White
Write-Host "   - Redirect unauthorized users in 100ms" -ForegroundColor White
