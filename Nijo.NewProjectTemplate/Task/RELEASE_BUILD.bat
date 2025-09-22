@echo off 
chcp 65001 
 
@rem 本番リリース用ビルド処理 
set "DIST_DIR_ROOT=%~dp0..\ReleaseModules" 
set "MIGRAION_SQL_DIR=%~dp0..\Core\MigrationsScript" 
set "ASP_NET_CORE_PUBLISH_DIR=%~dp0..\WebApi\bin\Release-Publish" 
set "NODE_JS_DIST_DIR=%~dp0..\client\dist" 
 
@rem ------------------------------------ 
@rem Gitコミットしていない変更があれば警告 
for /f "delims=" %%i in ('git status --porcelain 2^>nul') do ( 
    choice /c yn /m "コミットしていない変更があります。ビルドを続行しますか？" 
    if errorlevel 2 ( 
        exit /b 1 
    ) 
    @rem yが選択された場合は、これ以上確認する必要はないのでループを抜ける 
    goto :ChangesChecked 
) 
:ChangesChecked 
 
@rem ------------------------------------ 
@rem Check required tools 
 
dotnet --version 
if errorlevel 1 ( 
    echo 'dotnet' がインストールされていません。公式サイトからインストールしてください。 
    pause 
    exit /b 1 
) 
 
call npm --version 
if errorlevel 1 ( 
    echo 'npm' がインストールされていません。公式サイトからインストールしてください。 
    pause 
    exit /b 1 
) 
 
where nijo >nul 2>&1 
if errorlevel 1 ( 
    echo 'nijo' がインストールされていないかパスが通っていません。公式サイトからインストールしてください。 
    pause 
    exit /b 1 
) 
 
@rem ------------------------------------ 
@rem ソースコード自動生成処理のかけなおし 
 
pushd %~dp0.. 
nijo generate 
if not "%errorlevel%"=="0" ( 
    echo ソースコード自動生成処理でエラーが発生しました。ビルドを中断します。 
    pause 
    exit /b 1 
) 
popd 
 
@rem ------------------------------------ 
@rem ASP.NET Core のビルド 
 
pushd %~dp0..\WebApi 
dotnet publish /p:PublishProfile=本番用ビルド --output %ASP_NET_CORE_PUBLISH_DIR% 
if not "%errorlevel%"=="0" ( 
    echo ASP.NET Core のビルドでエラーが発生しました。ビルドを中断します。 
    pause 
    exit /b 1 
) 
popd 
 
@rem 実行時設定ファイルは環境構築時にリリース先環境に用意してあるものを使用するためモジュールに含めない 
del %ASP_NET_CORE_PUBLISH_DIR%\appsettings.json >nul 2>&1 
del %ASP_NET_CORE_PUBLISH_DIR%\appsettings.Development.json >nul 2>&1 
 
@rem ------------------------------------ 
@rem Node.js のビルド 
 
pushd %~dp0..\client 
call npm run build 
if not "%errorlevel%"=="0" ( 
    echo Node.js のビルドでエラーが発生しました。ビルドを中断します。 
    pause 
    exit /b 1 
) 
popd 
 
@rem ------------------------------------ 
@rem リリースファイルの格納 
 
set "DIST_DIR=%DIST_DIR_ROOT%\RELEASE_%DATE:~0,4%%DATE:~5,2%%DATE:~8,2%_%TIME:~0,2%%TIME:~3,2%%TIME:~6,2%" 
 
mkdir %DIST_DIR% 
mkdir %DIST_DIR%\WebApi 
mkdir %DIST_DIR%\MigrationsScript 
 
xcopy /E /I /Y %ASP_NET_CORE_PUBLISH_DIR%\* %DIST_DIR%\WebApi 
xcopy /E /I /Y %NODE_JS_DIST_DIR%\* %DIST_DIR%\WebApi\wwwroot 
xcopy /E /I /Y %MIGRAION_SQL_DIR%\* %DIST_DIR%\MigrationsScript 
 
@rem ------------------------------------ 
@rem リリース記録 
 
set "RELEASE_MEMO_FILE=%DIST_DIR%\リリース記録.txt" 
 
echo.>%RELEASE_MEMO_FILE% 
 
echo リリース日時:>>%RELEASE_MEMO_FILE% 
echo %DATE% %TIME%>>%RELEASE_MEMO_FILE% 
echo.>>%RELEASE_MEMO_FILE% 
 
echo リリース時Gitリビジョン:>>%RELEASE_MEMO_FILE% 
git rev-parse HEAD>>%RELEASE_MEMO_FILE% 
echo.>>%RELEASE_MEMO_FILE% 
 
echo dotnet バージョン:>>%RELEASE_MEMO_FILE% 
dotnet --version>>%RELEASE_MEMO_FILE% 
echo.>>%RELEASE_MEMO_FILE% 
 
echo npm バージョン:>>%RELEASE_MEMO_FILE% 
call npm --version>>%RELEASE_MEMO_FILE% 
echo.>>%RELEASE_MEMO_FILE% 
 
echo nijo バージョン:>>%RELEASE_MEMO_FILE% 
nijo --version>>%RELEASE_MEMO_FILE% 
echo.>>%RELEASE_MEMO_FILE% 
 
@rem ------------------------------------ 
echo リリースモジュールのビルドが完了しました。 
echo リリース先環境に配置してください。 
 
explorer %DIST_DIR_ROOT% 
 
exit /b 0 
