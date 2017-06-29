using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	public int HEIGHT = 120;
    public Transform Squirrel;
    public Transform Bear;
    public int WIDTH = 150;
	float BLOCK_SIZE = 1.0f;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < HEIGHT + 1; i++) {
			for (int j = 0; j < WIDTH + 1; j++) {
				GameObject bCube = new GameObject();
				bCube.transform.parent = transform;
				bCube.transform.position = new Vector3(WIDTH / 2 * BLOCK_SIZE * -1 + BLOCK_SIZE * j, HEIGHT / 2 * BLOCK_SIZE * -1 + BLOCK_SIZE * i, 0);
				bCube.transform.localScale = new Vector3(BLOCK_SIZE, BLOCK_SIZE, BLOCK_SIZE);
				SpriteRenderer renderer = bCube.AddComponent<SpriteRenderer>();
				var x = Resources.Load<Sprite>("Images/bgd");
				renderer.sprite = Resources.Load<Sprite>("Images/bgd");
                bCube.transform.eulerAngles = new Vector3(0, 0, 0);
            }
		}
        for (int y = 0; y < 1; y++)
        {
            for (int l = 0; l < 5; l++)
            {
                Spawn("Squirrel", l, y);
            }
        }
        Spawn("Bear", 0, 0);
        Spawn("Bear", -2, 2);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Spawn(string animal, float x, float y)
    {
        switch (animal) {
            case "Squirrel":
                Instantiate(Squirrel, new Vector3(x, y, -0.1f), Quaternion.identity);
                break;
            case "Bear":
                Instantiate(Bear, new Vector3(x, y, -0.1f), Quaternion.identity);
                break;
            default:
                break;
    }
    }
}
