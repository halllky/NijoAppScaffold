@echo off 
chcp 65001 
 
@rem ------------------------------------ 
@rem Check required tools 
 
dotnet --version 
if errorlevel 1 ( 
    echo 'dotnet' がインストールされていません。公式サイトからインストールしてください。 
    exit /b 1 
) 
 
call npm --version 
if errorlevel 1 ( 
    echo 'npm' がインストールされていません。公式サイトからインストールしてください。 
    exit /b 1 
) 
 
@rem ------------------------------------ 
@rem Install Node.js packages if not installed 
pushd %~dp0..\\client 
if not exist node_modules ( 
    echo node_modules （このアプリで使用しているNode.jsの各種ライブラリ）がインストールされていません。インストールしますか？ 
    choice /c yn 
    if errorlevel 2 ( 
        exit /b 1 
    ) 
    call npm ci 
 
    if not errorlevel 0 ( 
        echo Node.jsの各種ライブラリのインストールに失敗しました。再度実行してください。 
        exit /b 1 
    ) 
) 
 
@rem ------------------------------------ 
@rem Launch Vite + TypeScript (in background) 
start "Vite Dev Server" cmd /c "npm run dev" 
popd 
 
@rem ------------------------------------ 
@rem Launch ASP.NET Core Web API (in background) 
pushd %~dp0..\\WebApi 
start "ASP.NET Core API" cmd /c "dotnet watch --launch-profile http & pause" 
popd 
 
@rem ------------------------------------ 
@rem Exit 
exit /b 0 
