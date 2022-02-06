# vrchat-sync-npc-sdk3

SDK3(Udon)版同期NPC(AI)システム

## こういうやつ

- なにもしないとてきとうに歩く
- クリックするとついてくる
- 人について行っているやつを奪うことも出来る

![image](https://user-images.githubusercontent.com/1712548/152674381-0d8d95f6-42b3-4159-91d4-21045b6f9013.png)

## 必要アセット

事前に以下のアセットを入れてください

- VRCSDK3-WORLD https://vrchat.com/home/download
- UdonSharp https://github.com/MerlinVR/UdonSharp/releases/latest
- [StandardAssets](https://assetstore.unity.com/packages/essentials/asset-packs/standard-assets-for-unity-2018-4-32351?locale=ja-JP) のうち…… 
  - 以下をインポートしてください（Asset Storeからインポート出来ます）
  - `Standard Assets/Characters/ThirdPersonCharacter`のみ
  - ただしそのうち`Standard Assets/Characters/ThirdPersonCharacter/Scripts/ThirdPersonUserControl.cs`を除く

こんなかんじ

![image](https://user-images.githubusercontent.com/1712548/152673936-93b4bf39-5b53-4b97-91e2-aef570dd4c12.png)

## インストール

- **[SyncNPC ダウンロード]( https://github.com/Narazaka/vrchat-sync-npc-sdk3/releases )**

## 使い方

1. 任意のアバターをシーンに置く（エラーになっているコンポーネントは削除しておく）
2. Animatorコンポーネントの右上メニューから「ToNPC」を実行
3. 「再度実行してください」と言われるので、コンパイルが終わってからもう一回実行
4. NPCとしてセットアップされたアバターが出来るので好きに配置しててきとうにアップロードする

## License

- アセットの仕組み自体であるSyncNPCフォルダ内のものは[Zlib License](LICENSE)
- サンプルとして使用しているアバター[AINA](https://narazaka.booth.pm/items/1319390)は同梱の[ライセンス](Assets/AINA-Quest/license.txt)を参照してください。商用利用でなければ基本ほぼ何も気にせず使ってかまいません。
