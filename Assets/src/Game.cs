using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {

	public GameObject GameArea;
	public GameObject GridGRP;
	public GameObject GridH;
	public GameObject GridV;
	public GameObject PicturePieces;
	public GameObject OnePiece;
	public GameObject ActivePanel;
	public GameObject ClearPenel;
	public bool PrintLog;
	public int Level;
	public int ShuffleCount;      //シャッフル回数
	public Texture2D[] Images;

	public bool Cleared = false;
	private bool clearing = false;      //アニメーション用
	private bool moveLog = false;
	public bool DefaultCompleteMode = false;

	public const int WndWidth = 640;
	public const int WndHeight = 480;
	public int GameWidth;
	public int GameHeight;
	public int PieceOneWidth;
	public int PieceOneHeight;
	public Vector2 GameAreaBasePoint;

	//ゲーム情報
	public List<int> PlayedImageList = new List<int>();     //プレイした画像のインデックス履歴
	public CPiece[,] Pieces;            //配置されているピース情報
	public CPiece EmptySrcPanel;        //空白ピースになった部分のオブジェクト
	private List<GameObject> DynamicCreateObjects = new List<GameObject>();     //動的生成したオブジェクト群

	/// <summary>
	/// ピース情報
	/// </summary>
	public class CPiece {
		public GameObject obj;

		public int SrcX;       //全体図のどの部分を示すか: 0 Origin
		public int SrcY;

		public int PosX;       //このピースが今どの位置にあるか: 0 Origin
		public int PosY;
	}

	// Use this for initialization
	void Start() {

	}

	// Update is called once per frame
	void Update() {
		//クリアパネルの表示
		if(this.clearing == true) {
			this.ClearPenel.transform.localScale = new Vector3(
				this.ClearPenel.transform.localScale.x,
				this.ClearPenel.transform.localScale.y + 0.05f,
				this.ClearPenel.transform.localScale.z
			);
			if(this.ClearPenel.transform.localScale.y >= 1.0f) {
				this.ClearPenel.transform.localScale = new Vector3(
					this.ClearPenel.transform.localScale.x,
					1.0f,
					this.ClearPenel.transform.localScale.z
				);
				this.clearing = false;
			}
		}
	}

	/// <summary>
	/// ゲームを開始する
	/// </summary>
	public void GameStart() {
		this.GameArea.SetActive(true);
		this.DynamicCreateObjects.Clear();

		//格子を生成
		this.createGrid();

		//使用する画像を決定
		int index = -1;
		if(this.Images.Length <= this.PlayedImageList.Count) {
			this.PlayedImageList.Clear();       //全部遊んでいたら履歴を初期化
		}
		do {
			index = Random.Range(0, this.Images.Length);
		} while(this.PlayedImageList.Contains(index) == true);
		this.PlayedImageList.Add(index);
		this.OnePiece.GetComponent<UnityEngine.UI.RawImage>().texture = this.Images[index];

		//ピースを分割
		this.Pieces = new CPiece[this.Level, this.Level];
		for(int x = 0; x < this.Level; x++) {
			for(int y = 0; y < this.Level; y++) {
				this.Pieces[x, y] = new CPiece();
				this.Pieces[x, y].PosX = x;
				this.Pieces[x, y].SrcX = x;
				this.Pieces[x, y].PosY = y;
				this.Pieces[x, y].SrcY = y;
				this.Pieces[x, y].obj = Instantiate(this.OnePiece, this.PicturePieces.transform);
				this.Pieces[x, y].obj.transform.position = new Vector3(
					this.GameAreaBasePoint.x + this.PieceOneWidth / 2 + this.PieceOneWidth * x,
					this.GameAreaBasePoint.y - this.PieceOneHeight / 2 - this.PieceOneHeight * y,
					2
				);
				this.Pieces[x, y].obj.GetComponent<RectTransform>().sizeDelta = new Vector2(this.PieceOneWidth, this.PieceOneHeight);
				this.DynamicCreateObjects.Add(this.Pieces[x, y].obj);
			}
		}

		//画面上の表示位置をセットする
		for(int x = 0; x < this.Level; x++) {
			for(int y = 0; y < this.Level; y++) {
				this.Pieces[x, y].obj.GetComponent<UnityEngine.UI.RawImage>().uvRect = new Rect(
					x * (1.0f / this.Level),
					(1.0f / this.Level * (this.Level - 1)) - y * (1.0f / this.Level),
					1.0f / this.Level,
					1.0f / this.Level
				);
			}
		}

		//選択パネルを生成
		for(int x = 0; x < this.Level; x++) {
			for(int y = 0; y < this.Level; y++) {
				var selectPanel = Instantiate(this.ActivePanel, this.PicturePieces.transform);
				selectPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(this.PieceOneWidth, this.PieceOneHeight);
				selectPanel.GetComponent<RectTransform>().position = this.Pieces[x, y].obj.transform.position;
				selectPanel.GetComponent<SelectPanel>().SetPosition(x, y);
				this.DynamicCreateObjects.Add(selectPanel);
			}
		}

		//右下のピースを空けて、そのピースを完成時まで内部保管する
		this.EmptySrcPanel = this.Pieces[this.Level - 1, this.Level - 1];
		this.EmptySrcPanel.obj = this.Pieces[this.Level - 1, this.Level - 1].obj;
		this.EmptySrcPanel.obj.transform.position = new Vector3(
			this.GameAreaBasePoint.x + this.PieceOneWidth / 2 + this.PieceOneWidth * this.EmptySrcPanel.PosX,
			this.GameAreaBasePoint.y - this.PieceOneHeight / 2 - this.PieceOneHeight * this.EmptySrcPanel.PosY,
			2
		);
		this.EmptySrcPanel.obj.SetActive(false);
		this.Pieces[this.Level - 1, this.Level - 1] = null;

		if(DefaultCompleteMode == true) {
			//デバッグ用: 最初から完成状態にする
			for(int x = 0; x < this.Level; x++) {
				for(int y = 0; y < this.Level; y++) {
					if(this.Pieces[x, y] == null) {
						continue;
					}
					this.Pieces[x, y].obj.GetComponent<UnityEngine.UI.RawImage>().uvRect = new Rect(
						x * (1.0f / this.Level),
						(1.0f / this.Level * (this.Level - 1)) - y * (1.0f / this.Level),
						1.0f / this.Level,
						1.0f / this.Level
					);
				}
			}
		} else {
			//ピースを分散: 全体図のどの部分を示すかを交換方式でシャッフルする
			this.moveLog = true;
			int counter = 0;
			while(counter < ShuffleCount) {
				int x = Random.Range(0, this.Level);
				int y = Random.Range(0, this.Level);
				if(this.IsEmptyPiece(x, y) == false && this.MovePiece(x, y, false) == true) {
					counter++;
				}
			}
			this.moveLog = false;
		}
	}

	/// <summary>
	/// ゲームを終了する
	/// </summary>
	public void GameEnd() {
		this.Cleared = false;

		//動的生成したオブジェクトをすべて削除する
		foreach(var obj in this.DynamicCreateObjects) {
			Destroy(obj);
		}

		//ゲーム画面を非表示にする
		this.GameArea.SetActive(false);
		this.ClearPenel.transform.localScale = new Vector3(
			this.ClearPenel.transform.localScale.x,
			0,
			this.ClearPenel.transform.localScale.z
		);
	}

	/// <summary>
	/// グリッドを生成
	/// </summary>
	private void createGrid() {
		this.GameAreaBasePoint = this.GridH.GetComponent<LineRenderer>().GetPosition(0);
		Vector2 basePoint = this.GameAreaBasePoint;
		Vector2 destPointV = this.GridV.GetComponent<LineRenderer>().GetPosition(1);
		Vector2 destPointH = this.GridH.GetComponent<LineRenderer>().GetPosition(1);

		this.GameWidth = (int)(destPointH.x - basePoint.x);
		this.GameHeight = (int)Mathf.Abs(destPointV.y - basePoint.y);
		this.PieceOneWidth = this.GameWidth / this.Level;
		this.PieceOneHeight = this.GameHeight / this.Level;

		for(int x = this.PieceOneWidth; x < this.GameWidth; x += this.PieceOneWidth) {
			var obj = Instantiate(this.GridV, this.GridGRP.transform);
			obj.GetComponent<LineRenderer>().SetPositions(new Vector3[] {
				new Vector3(basePoint.x + x, basePoint.y, 1),
				new Vector3(basePoint.x + x, destPointV.y, 1)
			});
			this.DynamicCreateObjects.Add(obj);
		}

		for(int y = this.PieceOneHeight; y < this.GameHeight; y += this.PieceOneHeight) {
			var obj = Instantiate(this.GridH, this.GridGRP.transform);
			obj.GetComponent<LineRenderer>().SetPositions(new Vector3[] {
				new Vector3(basePoint.x, basePoint.y - y, 1),
				new Vector3(destPointH.x, basePoint.y - y, 1)
			});
			this.DynamicCreateObjects.Add(obj);
		}
	}

	/// <summary>
	/// ピースが完成したかどうかを調べる
	/// </summary>
	public bool JudgeClear() {
		for(int x = 0; x < this.Level; x++) {
			for(int y = 0; y < this.Level; y++) {
				if(this.Pieces[x, y] == null) {
					//空白タイルは正しい配置になっていると仮定する
					continue;
				}
				if(this.Pieces[x, y].SrcX != x || this.Pieces[x, y].SrcY != y) {
					//正解と違うピースを発見した時点でアウト
					return false;
				}
			}
		}
		return true;
	}

	/// <summary>
	/// 指定位置のピースが空白になっているか調べる
	/// </summary>
	public bool IsEmptyPiece(int x, int y) {
		if(x < 0 || this.Level <= x || y < 0 || this.Level <= y) {
			return false;
		}
		return (this.Pieces[x, y] == null);
	}

	/// <summary>
	/// 指定位置のピースを移動させる
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns>移動できたかどうか</returns>
	public bool MovePiece(int x, int y, bool judgeClear = true) {
		//このピースの隣に空白タイルがあれば移動する
		bool ret = false;
		int tx = 0;
		int ty = 0;
		if(this.IsEmptyPiece(x + 1, y) == true) {
			tx = 1;
		} else if(this.IsEmptyPiece(x - 1, y) == true) {
			tx = -1;
		} else if(this.IsEmptyPiece(x, y + 1) == true) {
			ty = 1;
		} else if(this.IsEmptyPiece(x, y - 1) == true) {
			ty = -1;
		}

		//移動できる場合
		if(tx != 0 || ty != 0) {
			this.Pieces[x + tx, y + ty] = this.Pieces[x, y];
			this.Pieces[x + tx, y + ty].PosX += tx;
			this.Pieces[x + tx, y + ty].PosY += ty;
			this.Pieces[x + tx, y + ty].obj.transform.position = new Vector3(
				this.GameAreaBasePoint.x + this.PieceOneWidth / 2 + this.PieceOneWidth * this.Pieces[x + tx, y + ty].PosX,
				this.GameAreaBasePoint.y - this.PieceOneHeight / 2 - this.PieceOneHeight * this.Pieces[x + tx, y + ty].PosY,
				2
			);
			this.Pieces[x, y] = null;
			ret = true;

			if(this.moveLog == true && this.PrintLog == true) {
				Debug.Log(x.ToString() + ":" + y.ToString() + " -> " + (x + tx).ToString() + ":" + (y + ty).ToString());
			}
		}

		if(judgeClear == true && this.JudgeClear() == true) {
			//完成した
			this.Cleared = true;
			this.clearing = true;

			//空白ピースを埋める
			this.EmptySrcPanel.obj.SetActive(true);
		}

		return ret;
	}
}
