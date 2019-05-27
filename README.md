# TWLogAnalyzer
Tales Weaverのチャットログを解析して遊ぶ

# できること
- 入手経験値/ルーン経験値の時給計算
- ベリネンルミのレイドボスの出現時間予想
    - チャットログをチェックしているため、自分が最後に出現を確認した時間を基準に計算。ブレ大きい。
- レイドボス出現予測時間近くになると棒読みちゃんが教えてくれる
- クラブ/チームチャットを棒読みちゃんでしゃべらせる


# 事前に必要な設定
## Tales Weaverの設定

Tales Weaverのチャットログ取得を有効にしておきます。

**注意事項** ログはたまり続けます。`インストールフォルダ\ChatLog`にログが保存されるため、気になる方は定期的に削除してください。

![image](https://user-images.githubusercontent.com/18679305/57971617-52717500-79cb-11e9-87ca-661088ead45d.png)

Copyrights (C) NEXON Corporation and NEXON Co., Ltd. All Rights Reserved.


## 棒読みちゃんの設定
棒読みちゃん連携をする場合、事前に棒読みちゃんをダウンロードして適当なフォルダに配置＆起動できることを確認してください。

棒読みちゃん連携時に使用される声は、棒読みちゃんの設定に依存します。
声質、音量、速度などは棒読みちゃん側で設定してください。

# 画面

![image](https://user-images.githubusercontent.com/18679305/57971708-6ff30e80-79cc-11e9-9d9b-7f4578bd01f5.png)

`棒読みちゃん連携`のチェックを入れた時点で棒読みちゃんを起動している場合、起動済みの棒読みちゃんと連携するため、棒読みちゃんを終了しないでください。また、この場合は`棒読みちゃん連携`のアンチェック/TWLogAnalyzerの終了をしても棒読みちゃんの停止をしないため、自分で棒読みちゃんを終了させてください。

`棒読みちゃん連携`のチェックを入れた時点で棒読みちゃんが起動していない場合、指定されたBouyomiChan.exeを起動します。この場合は、`棒読みちゃん連携`のアンチェック/TWLogAnalyzerの終了時に、棒読みちゃんに停止要求を投げます。ただ、棒読みちゃん側でダイアログが表示されている場合など、うまく停止できないケースがあったので、自動停止しなかった場合は自分で棒読みちゃんを終了させてください。

ビルド済みバイナリは[こちら](https://github.com/wakayuki/TWLogAnalyzer/releases)から。

# 動作環境

Talse Weaverが動く環境であればほぼ大丈夫と思いますが、自分の環境でしかテストできていないため保証はできません。

## テストに使った環境
- Windows 10 Pro x64
- メモリ 16GB

# TODO
- アイコン作る
- メンテ後はレイドボス沸き時間かわるので、予測をしないようにする
- 時給計算がちょっと怪しいのでちゃんとテストする
- 一定時間経験値入手がない場合に時給計算用のデータを自動リセット
- GUIのコントローラーとチャットログに応じた処理をする部分の設計が失敗しているので作り直す
- クラブ/チームチャット表示をもう少しましにする
- 棒読みちゃん連携で、キャラ名に記号とか入っているときに読み方指定できるようにする
- デバッグ用のログを実行時のオプション有無で履かないようにする
- 過去のチャットログの自動削除
- 棒読みちゃんにしゃべってもらう時の声を設定する(棒読みちゃんの設定ではなく、こっちの設定でしゃべってもらうようにする)
- フォレストの案内を拾う
- 任意の時間に任意の棒読みちゃんに何か喋らせる

