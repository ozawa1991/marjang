using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MakeHaipai : MonoBehaviour {
    
    public GameObject prefabDealed;
    private GameObject getClickObject()
    {
        GameObject result = null;
        // 左クリックされた場所のオブジェクトを取得
        // オブジェクトにはBox Collider 2Dが入ってないといけないのに注意
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 tapPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D collition2d = Physics2D.OverlapPoint(tapPoint);
            if (collition2d)
            {
                result = collition2d.transform.gameObject;
            }
        }
        return result;
    }
    
    private List<int> dealed = new List<int>();

    void Update()
    {
        GameObject obj = getClickObject();
        if (obj != null)
        {
            var name = obj.name;
            // 牌山から選択
            if (name.Contains("pai_"))
            {
                if (dealed.Count <= 13)
                {
                    int num = System.Int32.Parse(name.Substring(4));
                    // 同一の牌は5枚以上選択できないのは当たり前だよなあ！？
                    if (dealed.FindAll(n => n == num).Count >= 4) return;
                    dealed.Add(num);
                    dealed.Sort();
                    UpdateDealedPai();
                }

            }
            // 手牌から選択
            if (name.Contains("dealed_"))
            {
                // 手牌から除去する
                int num = System.Int32.Parse(name.Substring(7));
                dealed.Remove(num);
                UpdateDealedPai();
            }
            if (dealed.Count == 14)
            {
                isAgari();
            }
        }
    }

    private void UpdateDealedPai()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Textures/pai");
        foreach (Transform t in this.transform)
        {
            GameObject.Destroy(t.gameObject);
        }
        int k = 0;
        foreach (var d in dealed)
        {
            string name = "pai_" + d.ToString();
            Sprite sp = System.Array.Find<Sprite>(sprites, (sprite) => sprite.name.Equals(name));
            GameObject pai = Instantiate(prefabDealed, new Vector3((float)(-8 + k * 0.62), -4, 0), Quaternion.identity) as GameObject;
            pai.name = "dealed_" + name.Substring(4);
            pai.transform.localScale = new Vector3(2, 2, 2);
            pai.transform.SetParent(this.transform);
            pai.AddComponent<SpriteRenderer>().sprite = sp;
            pai.AddComponent<BoxCollider2D>().size = new Vector2(0.31f * 2, 0.44f * 2);
            k++;
        }
    }

    private void isAgari()
    {
        // shanten = 0:テンパイ、 -1:アガリ
        Debug.Log("国士" + Countkokushi() + "向聴");
        Debug.Log("七対" + Countchitoi() + "向聴");
        Debug.Log("一般" + Countippan() + "向聴");
    }

    private int Countkokushi()
    {
        int shanten = 13;
        int[] kokushiSeed = {0,8,10,18,20,28,30,31,32,33,34,35,36 };
        var dealdtmp = new List<int>(dealed);
        foreach (int num in kokushiSeed)
        {
            if (dealdtmp.Contains(num))
            {
                dealdtmp.Remove(num);
                shanten--;
            }
        }
        // 2枚持っていた場合は1種だけ向聴を下げる
        foreach (int num in kokushiSeed)
        {
            if (dealdtmp.Contains(num))
            {
                dealdtmp.Remove(num);
                shanten--;
                break;
            }
        }
        return shanten;
    }

    private int Countchitoi()
    {
        int shanten = 6;
        var dealedtmp = new List<int>(dealed);
        foreach (int num in dealedtmp.ToArray())
        {
            if (dealedtmp.FindAll(n => n == num).Count == 2)
            {
                dealedtmp.Remove(num);
                dealedtmp.Remove(num);
                shanten--;
            }
        }
        return shanten;
    }

    private int Countippan()
    {
        var t = DateTime.UtcNow.Ticks;
        // 各雀頭候補に対してシャンテン操作を行う
        var dealedHead = new List<int>(dealed);
        var dealedHeadless = new List<int>(dealed);
        var shanten = 8;
        var shanten_new = 8;
        var selectednum = -1;
        foreach (int num in dealed)
        {
            if (num == selectednum) continue;
            // 再帰がかかっているのが時間がかかりそうに見えるが、高々7つなのでそこまで時間はかからない
            if (dealedHead.FindAll(n => n == num).Count >= 2)
            {
                selectednum = num;
                Debug.Log("対子候補:" + num);
                dealedHead.Remove(num);
                dealedHead.Remove(num);
                shanten_new = CountShanten(dealedHead, 1);
                shanten = shanten_new < shanten ? shanten_new: shanten;
                dealedHead = new List<int>(dealed);
            }
        }
        // ヘッドレスに対しても同様の操作を行う
        Debug.Log("対子候補:なし");
        shanten_new = CountShanten(dealedHeadless);
        shanten = shanten_new < shanten ? shanten_new : shanten;

        Debug.Log(((DateTime.UtcNow.Ticks - t) / 10000) +"ミリ秒かかったよ！");
        return shanten;
    }

    private int CountShanten(List<int> pais, int isExistHead = 0)
    {
        int shanten = 8;

        var manzu = pais.FindAll(n => n <= 8);
        var pinzu = pais.FindAll(n => n >= 10 && n <= 18);
        var sozu = pais.FindAll(n => n >= 20 && n <= 28);
        var zihai = pais.FindAll(n => n >= 30 && n <= 36);

        var groupm = GetGroup(manzu);
        var groupp = GetGroup(pinzu);
        var groups = GetGroup(sozu);
        var groupz = GetGroup(zihai, true);

        foreach (var m in groupm)
        {
            foreach (var p in groupp)
            {
                foreach (var s in groups)
                {
                    foreach (var z in groupz)
                    {
                        var anko = m[0] + p[0] + s[0] + z[0];
                        var syuntu = m[1] + p[1] + s[1] + z[1];
                        var tatu = m[2] + p[2] + s[2] + z[2];
                        var toitu = m[3] + p[3] + s[3] + z[3] + isExistHead;
                        var shanten_new = CountFromMentu(anko, syuntu, tatu, toitu);
                        shanten = shanten_new < shanten ? shanten_new : shanten;
                        Debug.Log(anko + "," + syuntu + "," + tatu + "," + toitu + "," + shanten_new);
                    }
                }
            }
        }

        // 各色に対して、順子->暗刻->対子->塔子の順で取り除いていくが、それが最善の最短とは限らない
        // ex:111234->123 + 11 + 4 > 111 + 234
        // ex:2344456->23 + 444 + 56 > 234 + 4 + 456
        // ex:1222345->123 + 22 + 45 > 1 + 222 + 345

        return shanten;
    }

    // 向聴数 = 8 -(暗刻 + 順子) * 2 - (塔子 + 対子)
    private int CountFromMentu(int anko, int syuntu, int tatu, int toitu)
    {
        return 8 - (anko + syuntu) * 2 - (tatu + toitu)
            + IsMentuOver(anko, syuntu, tatu, toitu)
            + IsHeadLess(anko, syuntu, tatu, toitu);
    }

    // + (塔子オーバーなら + 1 (暗刻 + 順子 + 塔子 + 対子 >=6))
    private int IsMentuOver(int anko, int syuntu, int tatu, int toitu)
    {
        if (anko + syuntu + tatu + toitu >= 6)
        {
            return 1;
        }
        return 0;
    }

    // + (ヘッドレスなら + 1 (暗刻 + 順子 <= 3 && 暗刻 + 順子 + 塔子 >= 5 && 対子 == 0))
    private int IsHeadLess(int anko, int syuntu, int tatu, int toitu)
    {
        if (anko + syuntu + tatu >= 5 && toitu == 0)
        {
            return 1;
        }
        return 0;
    }
    // TODO:純カラの待ちはテンパイとは言わないので修正の必要があり

    private List<int[]> GetGroup(List<int> pais,bool zihai = false)
    {
        var paisp1 = new List<int>(pais);
        var list = new List<int[]>();
        // 字牌のみ、暗刻->対子で検証
        if (zihai)
        {
            var a = GetAnko(paisp1);
            var to = GetToitu(paisp1);
            list.Add(new int[] { a,0,0,to });
            return list;
        }

        // の3パターンについて検証
        var paisp2 = new List<int>(pais);
        var paisp3 = new List<int>(pais);
        // 暗刻->順子->塔子->対子
        var a1 = GetAnko(paisp1);
        var s1 = GetSyuntu(paisp1);
        var ta1 = GetTatu(paisp1);
        var to1 = GetToitu(paisp1);
        list.Add(new int[] { a1, s1, ta1, to1 });
        // 順子->暗刻->塔子->対子
        var s2 = GetSyuntu(paisp2);
        var a2 = GetAnko(paisp2);
        var ta2 = GetTatu(paisp2);
        var to2 = GetToitu(paisp2);
        list.Add(new int[] { a2, s2, ta2, to2 });
        if (list[0][0] == list[1][0] &&
            list[0][1] == list[1][1] &&
            list[0][2] == list[1][2] &&
            list[0][3] == list[1][3])
        {
            list.RemoveAt(1);
        }
        if (a1 <= 1)
        {
            return list;
        }
        else
        {
            // 1暗刻->順子->暗刻->塔子->対子
            var a3 = GetAnko(paisp3, true);
            var s3 = GetSyuntu(paisp3);
            var an3 = GetAnko(paisp3);
            var ta3 = GetTatu(paisp3);
            var to3 = GetToitu(paisp3);
            list.Add(new int[] { an3 + 1, s3, ta3, to3 });
            return list;
        }
    }
    private int GetSyuntu(List<int> pais)
    {
        int syuntu = 0;
        foreach (int pai in pais.ToArray())
        {
            // 順子の抜き出し
            if (pais.Contains(pai) && pais.Contains(pai + 1) && pais.Contains(pai + 2))
            {
                syuntu++;
                pais.Remove(pai);
                pais.Remove(pai + 1);
                pais.Remove(pai + 2);
            }
        }
        return syuntu;
    }
    private int GetAnko(List<int> pais, bool once = false)
    {
        int anko = 0;
        foreach (int pai in pais.ToArray())
        {
            // 暗刻の抜き出し
            if (pais.FindAll(n => n == pai).Count >= 3)
            {
                anko++;
                pais.Remove(pai);
                pais.Remove(pai);
                pais.Remove(pai);
                if (once) break;
            }
        }
        return anko;
    }
    private int GetToitu(List<int> pais)
    {
        int toitu = 0;
        foreach (int pai in pais.ToArray())
        {
            // 対子の抜き出し
            if (pais.FindAll(n => n == pai).Count == 2)
            {
                toitu++;
                pais.Remove(pai);
                pais.Remove(pai);
            }
        }
        return toitu;
    }
    private int GetTatu(List<int> pais)
    {
        int tatu = 0;
        foreach (int pai in pais.ToArray())
        {
            for (int i = 1; i <= 2; i++)
            {
                // 塔子の抜き出し
                if (pais.Contains(pai + i))
                {
                    tatu++;
                    pais.Remove(pai);
                    pais.Remove(pai + i);
                }
            }
        }
        return tatu;
    }

    // 孤立牌の除去
    private List<int> DeleteIsolatedPai(List<int> pais)
    {
        foreach(int pai in pais.ToArray())
        {
            // 下から順番に見てるので上の孤立性だけ確認すればよい
            if (pais.FindAll(n => n == pai).Count == 1 &&
                !pais.Contains(pai + 1) &&
                !pais.Contains(pai + 2))
            {
                pais.Remove(pai);
            }
        }
        return pais;
    }
}
