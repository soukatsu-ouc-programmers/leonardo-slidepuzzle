using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectPanel : MonoBehaviour {

	public Game game;
	private int x;
	private int y;

	// Use this for initialization
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	/// <summary>
	/// このパネルの位置をセットする
	/// </summary>
	public void SetPosition(int x, int y) {
		this.x = x;
		this.y = y;
	}

	/// <summary>
	/// アクティブな状態にする
	/// </summary>
	public void Activate() {
		//完成状態or空白ピースは処理しない
		if(this.game.IsEmptyPiece(this.x, this.y) == false && this.game.Cleared == false) {
			gameObject.GetComponent<UnityEngine.UI.Image>().color = new Color(0.4f, 0.4f, 0.4f, 0.8f);
		}
	}

	/// <summary>
	/// 非アクティブな状態にする
	/// </summary>
	public void NonActivate() {
		gameObject.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0);
	}

	/// <summary>
	/// この部分のピースを移動させる
	/// </summary>
	public void MovePiece() {
		//完成状態or空白ピースは処理しない
		if(this.game.IsEmptyPiece(this.x, this.y) == false && this.game.Cleared == false) {
			this.game.MovePiece(this.x, this.y);
			this.NonActivate();
		}
	}
}
