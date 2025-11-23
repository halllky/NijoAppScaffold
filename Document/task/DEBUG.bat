chcp 65001 
@echo off 
 
@REM ------------------------ 
@REM Nijo最新化 
cd %~dp0..\..\Nijo 
dotnet build -c Debug 
 
if not errorlevel 0 ( 
  echo ビルドに失敗しました 
  exit /b 1 
)
 
@REM ------------------------ 
@REM 自動生成されるドキュメントの生成 
.\bin\Debug\net9.0\nijo.exe generate-reference --out %~dp0..\src\pages\reference 
 
if not errorlevel 0 ( 
  echo ドキュメントの生成に失敗しました 
  exit /b 1 
)
 
@REM ------------------------ 
@REM デバッグ開始 
cd %~dp0.. 
call npm run start 
exit /b 0 
