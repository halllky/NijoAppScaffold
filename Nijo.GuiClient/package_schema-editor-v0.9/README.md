# nijo-schema-editor (for old version)

## 環境構築

全体的に .net 10 だがold版だけは .net 9 で動いている。
10しか入っていない場合は9をインストールする。以下は VSCode Dev Container 用。

```sh
sudo apt-get update \
    && apt-get install -y wget \
    && wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh \
    && chmod +x /tmp/dotnet-install.sh \
    && /tmp/dotnet-install.sh --channel 9.0 --install-dir /usr/share/dotnet \
    && rm /tmp/dotnet-install.sh \
    && rm -rf /var/lib/apt/lists/*
```

## デバッグ

バックエンドに nijo.exe のGUIサービス（実態はローカルホストに立つ単なるHTTPサーバー）を起動しておく。
（VSCodeの場合は Run Task の "Nijo > old版(バックエンド)" から）

それと並行してフロントエンド側（このパッケージ）は `npm run dev` コマンドで Node.js (Vite) によるWebサーバーを別ポートに起動する。

なお、バックとフロントで別々のポートにサーバーが立つ（クロスオリジン）ことになるが、
クロスオリジン状態だと色々面倒なので、Vite の設定を使ってバックエンドをプロキシすることでこれを回避している。

## リリース

JavaScript と CSS が埋め込まれた HTML ファイルを作成し、
それを nijo.exe の埋め込みリソースにする。

コマンドラインで nijo.exe のGUIを起動したとき、
nijo.exe はローカルホストに Web サービスを展開するが、
ブラウザでそのサービスのポートにアクセスすると埋め込みHTMLが返されるようにしておく。

なお、このパッケージに修正が入ることは滅多にないので、
埋め込み後のHTMLファイルはGit管理しておく。
