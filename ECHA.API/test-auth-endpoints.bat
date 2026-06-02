@REM ECHA.API Authentication Endpoints Testing Script (Windows)
@REM Requires: curl (Windows 10+ or Git Bash)

@ECHO OFF
SETLOCAL ENABLEDELAYEDEXPANSION

SET API_URL=https://localhost:5001
REM SET API_URL=http://localhost:5000

ECHO.
ECHO ==========================================
ECHO ECHA.API Authentication Testing
ECHO ==========================================

REM Test 1: Register a new user
ECHO.
ECHO [TEST 1] POST /api/auth/register
ECHO Registering new user...

curl -s -X POST "%API_URL%/api/auth/register" ^
  -H "Content-Type: application/json" ^
  -d "{\"nome\": \"João Silva\", \"email\": \"joao.silva@example.com\", \"password\": \"SecurePassword123!\", \"telemovel\": \"+244 912 345 678\"}" ^
  > register_response.json

ECHO Response:
TYPE register_response.json

REM Extract tokens using PowerShell
FOR /F "tokens=*" %%A IN ('powershell -Command "Get-Content register_response.json | ConvertFrom-Json | Select-Object -ExpandProperty accessToken"') DO SET ACCESS_TOKEN=%%A
FOR /F "tokens=*" %%A IN ('powershell -Command "Get-Content register_response.json | ConvertFrom-Json | Select-Object -ExpandProperty refreshToken"') DO SET REFRESH_TOKEN=%%A

ECHO.
ECHO Access Token: %ACCESS_TOKEN:~0,50%...
ECHO Refresh Token: %REFRESH_TOKEN:~0,50%...

REM Test 2: Try to register the same email again (should fail)
ECHO.
ECHO [TEST 2] POST /api/auth/register (Duplicate email - Should fail)
curl -s -X POST "%API_URL%/api/auth/register" ^
  -H "Content-Type: application/json" ^
  -d "{\"nome\": \"Another User\", \"email\": \"joao.silva@example.com\", \"password\": \"DifferentPassword123!\"}"

REM Test 3: Login with correct credentials
ECHO.
ECHO [TEST 3] POST /api/auth/login (Valid credentials)
curl -s -X POST "%API_URL%/api/auth/login" ^
  -H "Content-Type: application/json" ^
  -d "{\"email\": \"joao.silva@example.com\", \"password\": \"SecurePassword123!\"}" ^
  > login_response.json

ECHO Response:
TYPE login_response.json

REM Test 4: Login with incorrect password
ECHO.
ECHO [TEST 4] POST /api/auth/login (Invalid password - Should fail)
curl -s -X POST "%API_URL%/api/auth/login" ^
  -H "Content-Type: application/json" ^
  -d "{\"email\": \"joao.silva@example.com\", \"password\": \"WrongPassword123!\"}"

REM Test 5: Refresh token
ECHO.
ECHO [TEST 5] POST /api/auth/refresh (Refresh access token)
curl -s -X POST "%API_URL%/api/auth/refresh" ^
  -H "Content-Type: application/json" ^
  -d "{\"refreshToken\": \"%REFRESH_TOKEN%\"}"

REM Test 6: Test invalid refresh token
ECHO.
ECHO [TEST 6] POST /api/auth/refresh (Invalid refresh token - Should fail)
curl -s -X POST "%API_URL%/api/auth/refresh" ^
  -H "Content-Type: application/json" ^
  -d "{\"refreshToken\": \"invalid-token-12345\"}"

ECHO.
ECHO ==========================================
ECHO Testing Complete!
ECHO ==========================================

REM Cleanup
DEL register_response.json login_response.json 2>NUL

ENDLOCAL
