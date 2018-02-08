using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Text.RegularExpressions;

public class Init : MonoBehaviour {

    public GameObject prefabInit;
    // Use this for initialization
    void Start ()
    {
        string fileName = "Textures/pai";
        var sprites = Resources.LoadAll<Sprite>(fileName);
        for (int j = 0; j <= 3; j++)
        {
            for (int i = 0; i <= 9; i++)
            {
                int num = i + j * 10;
                string name = "pai_" + num;
                if (name == "pai_37") break;
                Sprite sp = System.Array.Find<Sprite>(sprites, (sprite) => sprite.name.Equals(name));
                GameObject pai = Instantiate(prefabInit, new Vector3((float)(i * 1.3 - 8), (float)(4 - j * 1.5), 0), Quaternion.identity) as GameObject;
                pai.name = name;
                pai.transform.localScale = new Vector3(2, 2, 2);
                pai.transform.SetParent(this.transform);
                pai.AddComponent<SpriteRenderer>().sprite = sp;
                pai.AddComponent<BoxCollider2D>().size = new Vector2(0.31f * 2, 0.44f * 2);

            }
        }
    }
}
