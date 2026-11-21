$ErrorActionPreference = 'Stop'
$projectDirectory = Join-Path $PSScriptRoot 'src\VT3CyberZone.Server'

$existingListener = Get-NetTCPConnection -State Listen -LocalPort 5121 -ErrorAction SilentlyContinue |
    Select-Object -First 1
if ($existingListener) {
    throw "Cong 5121 dang duoc su dung boi tien trinh PID $($existingListener.OwningProcess). Hay dong server cu bang Ctrl+C roi chay lai file nay."
}

$mysqlPassword = Read-Host 'Nhap mat khau MySQL root (mat khau se khong hien tren man hinh)' -AsSecureString
if ($mysqlPassword.Length -eq 0) {
    throw 'Mat khau MySQL khong duoc de trong.'
}

$passwordPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($mysqlPassword)
try {
    $plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($passwordPointer)
    $escapedPassword = $plainPassword.Replace('"', '""')
} finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passwordPointer)
    $plainPassword = $null
    $mysqlPassword.Dispose()
}

$env:ASPNETCORE_URLS = 'http://0.0.0.0:5121'
$env:ConnectionStrings__DefaultConnection = "Server=127.0.0.1;Port=3306;Database=vt3_cyberzone_db;User=root;Password=`"$escapedPassword`""
$escapedPassword = $null

$jwtKeyBytes = New-Object byte[] 32
$randomNumberGenerator = [Security.Cryptography.RandomNumberGenerator]::Create()
try {
    $randomNumberGenerator.GetBytes($jwtKeyBytes)
} finally {
    $randomNumberGenerator.Dispose()
}
$env:Jwt__Key = [Convert]::ToBase64String($jwtKeyBytes)
$env:Jwt__Issuer = 'VT3CyberZone'
$env:Jwt__Audience = 'VT3CyberZone.Clients'

try {
    Set-Location $projectDirectory

    $dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($dotnetCommand) {
        & $dotnetCommand.Source run --launch-profile http -p:UseAppHost=false
    } else {
        $localDotnet = Join-Path $PSScriptRoot '..\.dotnet\dotnet.exe'
        if (-not (Test-Path $localDotnet)) {
            throw 'Khong tim thay .NET SDK. Hay cai .NET 10 SDK roi chay lai.'
        }

        & $localDotnet run --project (Join-Path $projectDirectory 'VT3CyberZone.Server.csproj') --launch-profile http -p:UseAppHost=false
    }

    if ($LASTEXITCODE -ne 0) {
        throw "Server dung voi ma loi $LASTEXITCODE."
    }
} finally {
    Remove-Item Env:ConnectionStrings__DefaultConnection -ErrorAction SilentlyContinue
    Remove-Item Env:Jwt__Key -ErrorAction SilentlyContinue
    Remove-Item Env:Jwt__Issuer -ErrorAction SilentlyContinue
    Remove-Item Env:Jwt__Audience -ErrorAction SilentlyContinue
    Remove-Item Env:ASPNETCORE_URLS -ErrorAction SilentlyContinue
}
