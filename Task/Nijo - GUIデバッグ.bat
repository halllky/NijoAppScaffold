chcp 65001
@rem ↑ dotnetコマンド実行時に強制的に書き換えられてしまいnpmの標準入出力が化けるので先に書き換えておく
 
@rem これはXMLスキーマ定義編集UIのデバッグ開始コマンドです。 
@rem デバッグは Windows Forms ではなくホットリロードがある vite で行います。 
 
@echo off 
setlocal 
 
set "NIJO_ROOT=%~dp0.." 
set "BACKEDN_PROJECT=%NIJO_ROOT%\Nijo" 
set "DEMO_200=%NIJO_ROOT%\demo\200_複雑なパターン" 
set "FRONTEND_ROOT=%NIJO_ROOT%\Nijo.GuiClient\package_schema-editor" 
 
start dotnet run --project %BACKEDN_PROJECT% -- serve --port 8081 
 
pushd %FRONTEND_ROOT% 
start npm run dev 
start http://localhost:5176/nijo-ui/ 
popd 
