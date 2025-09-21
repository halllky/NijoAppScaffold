@echo off 
chcp 65001 
 
@rem ------------------------------------ 
@rem Check required tools 
 
dotnet --version 
if errorlevel 1 ( 
    echo 'dotnet' is not installed. Please install it from Official Site. 
    exit /b 1 
) 
 
call npm --version 
if errorlevel 1 ( 
    echo 'npm' is not installed. Please install it from Official Site. 
    exit /b 1 
) 
 
@rem ------------------------------------ 
@rem Install Node.js packages if not installed 
pushd %~dp0client 
if not exist node_modules ( 
    echo 'node_modules' is not installed. Would you like to install it? 
    choice /c yn 
    if errorlevel 2 ( 
        exit /b 1 
    ) 
    call npm ci 
 
    if not errorlevel 0 ( 
        echo Failed to install Node.js packages. Please try again. 
        exit /b 1 
    ) 
) 
 
@rem ------------------------------------ 
@rem Launch Vite + TypeScript (in background) 
start "Vite Dev Server" cmd /c "npm run dev" 
popd 
 
@rem ------------------------------------ 
@rem Launch ASP.NET Core Web API (in background) 
pushd %~dp0webapi 
start "ASP.NET Core API" cmd /c "dotnet run --launch-profile http & pause" 
popd 
 
@rem ------------------------------------ 
@rem Exit 
exit /b 0 
