# UnityシミュレーションRPG
Unityでの学習を通して制作した、シミュレーションRPGのプログラム群です。
## 主な実装
ダイクストラ法を用いた移動範囲の計算,SRPGにおけるキャラクターの移動
画面の遷移やメニューの開閉をスタックで制御
## 開発環境
Unity6
C#
```text
Assets/
└── SRPG/
    ├── Input/                       # InputSystemのデータ
    │   ├── PlayerInput.cs           # InputSystemによる自動生成
    │   └── PlayerInput.inputactions
    │
    Item/                            # アイテム・装備のデータファイル
    │   ├── ItemBase.cs              # アイテムと装備の共通の親クラス
    │   ├── Item/                    # アイテムのデータと効果のデータファイル
    │   │   ├── ItemData.cs          # アイテムの親クラス
    │   │   ├── ItemEffect.cs        # アイテムを使ったときの効果
    │   │   ├── Damage/              # 攻撃アイテムのデータファイル
    │   │   │   ├── DamageEffect.cs  # 攻撃アイテムを使ったときの効果
    │   │   │   ├── DamageItem.asset # 攻撃アイテムの設定データ
 　　│   │   │   └── Bomb.asset       # 攻撃アイテムのデータ
    │   │   └── Heal/               # 回復アイテムのデータファイル
    │   │       ├── HealEffect.cs    # 回復アイテムを使ったときの効果
    │   │       ├── HealItem.asset   # 回復アイテムの設定データ
 　　│   │       └── Potion.asset     # 回復アイテムのデータ
    │   ├── Weapon/                  # 装備のデータファイル
    │   ├── WeaponData.cs            # 装備の親クラス
    │   ├── Arrow.asset              # 武器のデータ
    │   ├── Axe.asset                # 武器のデータ
    │   └── Staff.asset              # 武器のデータ
    │
    ├── Manager/                     # ゲーム全体のルールや進行、計算を行うマネージャーのデータファイル
    │   ├── BattleManager.cs  　　    # 戦闘ルール、状態管理、ターン進行
    │   ├── MapManager.cs            # マップデータ、ダイクストラ法を用いた移動・攻撃範囲計算、マップキューブの色変更
    │   ├── UIManager.cs             # UI全体の変更、メニューの階層をスタックで管理
    │   ├── CameraManager.cs         # カメラの動きの管理
    │   └── EnemyManager.cs          # 敵の動き、攻撃の管理
    │
    ├── Scene/                       # ステージのシーンデータファイル
    │   ├── BattleMap1.unity         # Cubeを10×10並べたマップ
    │   └── Cube/                    # マップの床のデータファイル
    │       ├── MapCube.cs           # マウスの動きを検知してマネージャーへ伝える
    │       └── MapCube.prefab       # マップ上に並べる枠、コライダのオブジェクト
    │       └── CubeDefalt.mat       # マスの色のマテリアルデータ
    │
    ├── UI/                          # それぞれのUIをコントロールするデータファイル
    │   ├── Command.cs               # コマンドのボタン制御
    │   ├── Inventory.cs             # 所持アイテムや装備画面の管理
    │   ├── InventoryButton.cs       # インベントリ内の各アイテムボタンの制御
    │   ├── Item_Button.prefab       # インベントリ内に並べるアイテムボタンの見た目データ
    │   ├── Confirmation.cs          # アイテム使用時などの確認ダイアログ
    │   ├── ReturnButton.cs          # キャンセルボタンの処理
    │   ├── NotoSansJP.ttf           # テキスト表示に使用しているフォントデータ
    │   ├── LeftStatusPanel.cs       # 敵のステータス表示の管理
    │   └── RightStatusPanel.cs      # プレイヤーのステータス表示の管理
    │
    └── Unit/                        # キャラクターやマップ上のギミックのデータファイル
        ├── UnitBase.cs              # すべてのユニットの親クラス
        ├── UnitStatus.cs            # ユニットのステータスデータ
        ├── IMapObject.cs            # マップ上に配置するオブジェクト用の共通インターフェース
        ├── Player/                  # プレイヤーキャラクターのデータファイル
  　　　　│   └── PlayerUnit.cs 　    　# 味方ユニット用のクラス
        ├── Enemy/                   # 敵キャラクターのデータファイル
        │   └── EnemyUnit.cs         # 敵ユニット用のクラス
        └── Gimmick/                 # マップ上のギミックのデータファイル
            └── GimmickBase.cs       # ギミックの親クラス
```
