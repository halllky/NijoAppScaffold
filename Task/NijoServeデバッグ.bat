chcp 65001
@rem ↑ dotnetコマンド実行時に強制的に書き換えられてしまいnpmの標準入出力が化けるので先に書き換えておく
 
@rem これはXMLスキーマ定義編集UIのデバッグ開始コマンドです。 
@rem デバッグは Windows Forms ではなくホットリロードがある vite で行います。 
 
@echo off 
setlocal 
 
if "%1"=="" ( 
  echo Usage: %0 ^<serveUiVersion^> 
  echo. 
  echo   serveUiVersion: package_schema-editor or package_schema-editor-v1 
  exit /b 1 
) 
 
set "BACKEDN_PROJECT=%~dp0..\Nijo" 
set "FRONTEND_ROOT=%~dp0..\Nijo.GuiClient\%1" 
 
pushd %BACKEDN_PROJECT% 
start cmd /k "dotnet watch --launch-profile DebugServe" 
popd 
 
pushd %FRONTEND_ROOT% 
start npm run dev 
popd 
