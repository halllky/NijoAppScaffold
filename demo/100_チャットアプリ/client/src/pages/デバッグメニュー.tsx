import React from "react";
import { callAspNetCoreApiAsync } from "../callAspNetCoreApiAsync";

/**
 * デバッグメニュー
 */
export default function () {

  // サーバーとの疎通状況
  const [serverStatus, setServerStatus] = React.useState<string>("")
  const executeServerStatusCheck = React.useCallback(async () => {
    setServerStatus("サーバーとの疎通状況を取得しています...");
    const response = await callAspNetCoreApiAsync("/example", {
      method: "GET",
    })
    if (!response.ok) {
      setServerStatus("サーバーとの疎通に失敗しました");
    } else {
      setServerStatus(await response.text());
    }
  }, [])

  React.useEffect(() => {
    executeServerStatusCheck()
  }, [])

  // データベースを削除して再作成する
  const destroyAndRecreateDatabase = React.useCallback(async () => {
    if (!window.confirm("データベースを削除して再作成します。よろしいですか？")) {
      return;
    }
    const response = await callAspNetCoreApiAsync("/example/destroy-and-recreate-database", {
      method: "POST",
    })
    if (!response.ok) {
      const error = await response.text();
      window.alert(`データベースの削除と再作成に失敗しました:\n${error}`);
    } else {
      window.alert("データベースの削除と再作成に成功しました");
    }
  }, [])

  // 本番ビルドではレンダリングしない
  if (!import.meta.env.DEV) {
    return null;
  }

  return (
    <div style={{
      display: "flex",
      flexDirection: "column",
      alignItems: "start",
      justifyContent: "center",
      border: "1px solid #ccc",
      padding: "16px",
      gap: "16px",
      borderRadius: "8px",
      backgroundColor: "#f0f0f0",
    }}>
      <h2 style={{
        fontSize: "16px",
        margin: 0,
      }}>
        デバッグメニュー（開発環境でのみ表示）
      </h2>

      <button type="button" onClick={destroyAndRecreateDatabase}>
        データベースを削除して再作成する
      </button>

      <div style={{
        alignSelf: "stretch",
        display: "flex",
        flexDirection: "column",
        gap: "4px",
      }}>
        <div style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}>
          サーバーとの疎通状況
          <button type="button" onClick={executeServerStatusCheck}>
            再読み込み
          </button>
        </div>
        <p style={{
          color: "white",
          backgroundColor: "#0d0d0d",
          padding: "8px",
          borderRadius: "4px",
          fontSize: "14px",
          margin: 0,
          height: "200px",
          overflow: "auto",
          textAlign: "left",
          whiteSpace: "pre-wrap",
        }}>
          {serverStatus}
        </p>
      </div>
    </div>
  )
}