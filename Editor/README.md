# Unity Editor Remote Control (for ClaudeCode)

Unityエディタ起動中に、**外部プロセス（例: ClaudeCode CLI, CI）**から
- エラー取得
- リフレッシュ（再インポート/再コンパイル待機）
- ビルド（Android/iOS）
をHTTPで叩ける仕組みです。

## 起動
- このフォルダをプロジェクトへ配置すると、エディタ起動時に `http://127.0.0.1:8787/` が待受けになります。
- 停止は `Window > Remote Control > Toggle Server`。

## エンドポイント（全て JSON）
- `GET /health` → `{ "ok": true, "compiling": false }`
- `POST /refresh` body: `{ "reimport": ["Assets/..."], "force": true }`
- `POST /awaitCompile` body: `{ "timeoutSec": 60 }` → コンパイル完了まで待機
- `GET /errors?level=error|warning|log&limit=200` → 最新ログを返す（エディタ起動後から取得）
- `POST /errors/clear` → 収集済みログをクリア
- `POST /build` body:
  ```json
  {
    "target": "Android" | "iOS",
    "outputPath": "Builds/Android/app.apk" | "Builds/iOS",
    "scenes": ["Assets/Scenes/SimpleCamera_Composited.unity"],
    "development": false,
    "clean": true
  }
  ```

## ヘルパー
`tools/` に curl ベースの簡易CLIを用意しました。

## 注意
- ローカルホストのみで待受けます。外部公開しないでください。
- Unity APIはメインスレッドのみ触るため、処理はキュー経由で実行します。
